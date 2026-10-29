
using UnityEngine;

public class CarNPCMove : MonoBehaviour
{
    [Header("Explosion Effect")]
    [SerializeField] private GameObject fvx_explosionsPrefab;

    [Header("Movement")]
    public float speed = 5f;
    public float acceleration = 2f;
    public float deceleration = 4f;

    public Transform targetPoint;
    public CarNode CurrentCarNode;
    public CarNode PrevCarNode;

    [Header("Curved Turns")]
    [Tooltip("Larger values create wider, gentler turns.")]
    public float turnRadius = 2.5f;

    [Tooltip("Maximum steering rotation in degrees per second.")]
    public float turnSpeed = 100f;

    [Tooltip("How early the car slows down near a junction.")]
    public float slowDownDistance = 4f;

    [Range(0.1f, 1f)]
    public float cornerSpeedMultiplier = 0.55f;

    [Header("Player Detection")]
    public float warnedDist = 20f;
    public float scaredTurnAmount = 20f;
    public float chaosTurningAmount = 0.9f;
    public int carValue = 2;

    private bool isScared;
    private float chaosTimer;
    private float scaredTimer;
    private float currentSpeed;

    // Fixed world height for normal driving.
    private float drivingHeight;

    // Bézier curve points.
    private Vector3 curveStart;
    private Vector3 curveControl1;
    private Vector3 curveControl2;
    private Vector3 curveEnd;

    private float curveProgress;
    private float curveLength = 1f;
    private float cornerFactor = 1f;
    private Transform previousTarget;

    private bool isDestroyed;

    private void Start()
    {
        currentSpeed = speed;
        previousTarget = targetPoint;

        // Remember the car's initial height.
        drivingHeight = transform.position.y;

        if (targetPoint != null)
            BuildCurve();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (isScared)
        {
            ScaredDriving(dt);
            return;
        }

        // Build a new curve when the target node changes.
        if (targetPoint != previousTarget)
        {
            previousTarget = targetPoint;

            if (targetPoint != null)
                BuildCurve();
        }

        NormalDriving(dt);
        CheckForPlayer();
    }

    private void BuildCurve()
    {
        curveStart = transform.position;
        curveStart.y = drivingHeight;

        curveEnd = targetPoint.position;
        curveEnd.y = drivingHeight;

        curveProgress = 0f;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        Vector3 toTarget = curveEnd - curveStart;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;

        if (distance < 0.01f)
        {
            curveLength = 0.01f;
            return;
        }

        Vector3 outgoing = toTarget.normalized;

        if (forward.sqrMagnitude < 0.01f)
            forward = outgoing;
        else
            forward.Normalize();

        // Create a smooth curve toward the next node.
        float handle = Mathf.Min(turnRadius, distance * 0.45f);

        curveControl1 = curveStart + forward * handle;
        curveControl2 = curveEnd - outgoing * handle;

        // Keep every curve point at the same world height.
        curveControl1.y = drivingHeight;
        curveControl2.y = drivingHeight;

        curveLength =
            Vector3.Distance(curveStart, curveControl1) +
            Vector3.Distance(curveControl1, curveControl2) +
            Vector3.Distance(curveControl2, curveEnd);

        curveLength = Mathf.Max(curveLength, 0.01f);

        float angle = Vector3.Angle(forward, outgoing);

        cornerFactor = Mathf.Lerp(
            1f,
            cornerSpeedMultiplier,
            Mathf.InverseLerp(15f, 100f, angle)
        );
    }

    private Vector3 EvaluateCurve(float t)
    {
        float u = 1f - t;

        return
            u * u * u * curveStart +
            3f * u * u * t * curveControl1 +
            3f * u * t * t * curveControl2 +
            t * t * t * curveEnd;
    }

    private Vector3 EvaluateCurveDirection(float t)
    {
        float u = 1f - t;

        Vector3 tangent =
            3f * u * u * (curveControl1 - curveStart) +
            6f * u * t * (curveControl2 - curveControl1) +
            3f * t * t * (curveEnd - curveControl2);

        tangent.y = 0f;

        return tangent.normalized;
    }

    private void NormalDriving(float dt)
    {
        if (targetPoint == null)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                deceleration * dt
            );

            return;
        }

        float remainingDistance =
            Vector3.Distance(transform.position, curveEnd);

        float brakingFactor = Mathf.Clamp01(
            remainingDistance / Mathf.Max(slowDownDistance, 0.1f)
        );

        float desiredSpeed = speed * cornerFactor *
            Mathf.Lerp(0.35f, 1f, brakingFactor);

        float rate = desiredSpeed < currentSpeed
            ? deceleration
            : acceleration;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            desiredSpeed,
            rate * dt
        );

        // Advance along the curve.
        curveProgress += currentSpeed * dt / curveLength;
        curveProgress = Mathf.Clamp01(curveProgress);

        Vector3 nextPosition = EvaluateCurve(curveProgress);
        Vector3 tangent = EvaluateCurveDirection(curveProgress);

        // Lock the car to its original world height.
        nextPosition.y = drivingHeight;
        transform.position = nextPosition;

        // Rotate to face the direction of travel.
        if (tangent.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(tangent);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                desiredRotation,
                turnSpeed * dt
            );
        }

        // Finish at the target without changing height.
        if (curveProgress >= 1f)
        {
            Vector3 finalPosition = curveEnd;
            finalPosition.y = drivingHeight;

            transform.position = finalPosition;
        }
    }

    private void CheckForPlayer()
    {
        if (Physics.Raycast(
            transform.position,
            transform.forward,
            out RaycastHit hit,
            warnedDist))
        {
            if (hit.collider.CompareTag("Player"))
            {
                isScared = true;
                chaosTimer = 0f;
                scaredTimer = 0f;
            }
        }
    }

    private void ScaredDriving(float dt)
    {
        chaosTimer += dt;
        scaredTimer += dt;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            speed,
            acceleration * dt
        );

        transform.Translate(
            Vector3.forward * currentSpeed * dt,
            Space.Self
        );

        transform.Rotate(
            Vector3.up * scaredTurnAmount * dt,
            Space.Self
        );

        // Keep the car at its fixed height while scared.
        Vector3 position = transform.position;
        position.y = drivingHeight;
        transform.position = position;

        if (chaosTimer >= chaosTurningAmount)
        {
            chaosTimer = 0f;

            if (Random.Range(0, 2) == 1)
                scaredTurnAmount *= -1f;
        }

        if (scaredTimer >= 3f)
        {
            scaredTimer = 0f;
            isScared = false;

            if (targetPoint != null)
            {
                previousTarget = targetPoint;
                BuildCurve();
            }
        }
    }

    private void Explode()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        if (fvx_explosionsPrefab != null)
        {
            GameObject explosion = Instantiate(
                fvx_explosionsPrefab,
                transform.position,
                Quaternion.identity
            );

            Debug.Log("Explosion prefab spawned!");
            Destroy(explosion, 5f);
        }
        else
        {
            Debug.LogWarning("Explosion prefab is NOT assigned!");
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DogMovement dm =
                collision.gameObject.GetComponent<DogMovement>();

            if (dm != null)
            {
                dm.soulCount += carValue;
                dm.totalSoulCount += carValue;
            }

            Explode();
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Explode();
        }
    }
}
