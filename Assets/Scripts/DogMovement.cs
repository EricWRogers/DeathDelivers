using UnityEngine;
using UnityEngine.InputSystem;

public class DogMovement : MonoBehaviour
{
    public DogBase dogBase;

    [Header("Soul & Speed")]
    public float soulCount;
    public int totalSoulCount;
    public float speed;

    [Header("Turning")]
    public float turn;
    public float turnSmoothness = 8f;
    public float maxTiltAngle = 15f;
    public float uprightSmoothness = 8f;

    [Header("Quick Turn")]
    public float quickTurnDuration = 0.25f;
    public float quickTurnMultiplier = 1f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    public Transform AnchorL;
    public Transform AnchorR;

    [Header("Wall Collision")]
    public string wallTag = "Wall";
    public float wallSkin = 0.03f;
    public float wallCheckDistance = 0.1f;
    public int wallSlideIterations = 3;
    
    [Header("Firework & Pepper & Bone")]
    private float fireworkTimer;
    private float fireworkjump;
    private float pepperTimer;
    private float pepperMult;
    private float boneTimer;
    private float boneScale;
    private float targetTurn;

    private bool quickTurning = false;
    private float quickTurnDirection = 0f;

    private Transform activeAnchor;
    private Vector3 activeAnchorLockedPosition;
    private Vector3 dogPositionRelativeToAnchor;

    private Collider dogCollider;
    private Vector3 startScale;
    private RaycastHit hit;

    void Start()
    {
        dogCollider = GetComponent<Collider>();
        startScale = transform.localScale;
        speed = dogBase.startBoost;
        soulCount = 30f;
        totalSoulCount += 30;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        soulCount -= dogBase.soulConsump * dt;
        soulCount = Mathf.Max(soulCount, 0f);

        speed = soulCount * dogBase.speed;
        if(fireworkTimer > 0f)
        {
            fireworkTimer -= Time.deltaTime;
            float upward = fireworkjump * Time.deltaTime;
            MoveWithWallCollision(Vector3.up * upward);
        }

        if(pepperTimer > 0f)
        {
            pepperTimer -= Time.deltaTime;
            speed *= pepperMult;

        }
        if(boneTimer > 0f)
        {
            boneTimer -= Time.deltaTime;

            float target = boneScale;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * target, Time.deltaTime * 5f);
        }
        else
        {
            transform.localScale = Vector3.Lerp(startScale, Vector3.one, Time.deltaTime * 5f);
        }
        if (!quickTurning)
        {
            if (Keyboard.current.dKey.isPressed &&
                Keyboard.current.spaceKey.isPressed &&
                IsGrounded())
            {
                StartQuickTurn(1f);
            }
            else if (Keyboard.current.aKey.isPressed &&
                     Keyboard.current.spaceKey.isPressed &&
                     IsGrounded())
            {
                StartQuickTurn(-1f);
            }
        }

        if (quickTurning)
        {
            if (Keyboard.current.spaceKey.isPressed)
            {
                DoQuickTurn();
                return;
            }

            EndQuickTurn();
        }

        MoveForward(dt);
        HandleTurning(dt);
        KeepDogUpright();
    }

    void MoveForward(float dt)
    {
        Vector3 movement =
            transform.forward *
            speed *
            dt;

        MoveWithWallCollision(movement);
    }

    void HandleTurning(float dt)
    {
        targetTurn = 0f;

        if (Keyboard.current.aKey.isPressed)
        {
            targetTurn = -dogBase.turnSpeed;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            targetTurn = dogBase.turnSpeed;
        }

        turn = Mathf.Lerp(
            turn,
            targetTurn,
            turnSmoothness * dt
        );

        transform.Rotate(
            Vector3.up *
            turn *
            dt,
            Space.Self
        );
    }

    void StartQuickTurn(float direction)
    {
        quickTurning = true;
        quickTurnDirection = direction;
        turn = 0f;

        activeAnchor =
            direction > 0f
                ? AnchorR
                : AnchorL;

        if (activeAnchor == null)
        {
            quickTurning = false;
            return;
        }

        activeAnchorLockedPosition =
            activeAnchor.position;

        dogPositionRelativeToAnchor =
            transform.position -
            activeAnchorLockedPosition;
    }

    void DoQuickTurn()
    {
        if (activeAnchor == null)
            return;

        float quickTurnSpeed =
            speed *
            dogBase.turnSpeed *
            quickTurnMultiplier;

        float rotationAmount =
            quickTurnDirection *
            quickTurnSpeed *
            Time.deltaTime;

        Quaternion rotation =
            Quaternion.Euler(
                0f,
                rotationAmount,
                0f
            );

        Vector3 targetRelativePosition =
            rotation *
            dogPositionRelativeToAnchor;

        Vector3 targetPosition =
            activeAnchorLockedPosition +
            targetRelativePosition;

        Vector3 movement =
            targetPosition -
            transform.position;

        MoveWithWallCollision(movement);

        dogPositionRelativeToAnchor =
            transform.position -
            activeAnchorLockedPosition;

        if (movement.sqrMagnitude > 0.000001f)
        {
            transform.rotation =
                rotation *
                transform.rotation;
        }

        KeepDogUpright();
    }

    void EndQuickTurn()
    {
        quickTurning = false;
        activeAnchor = null;
        turn = 0f;

        KeepDogUpright();
    }

    void MoveWithWallCollision(Vector3 movement)
    {
        if (movement.sqrMagnitude <= 0.000001f)
            return;

        if (dogCollider == null)
        {
            transform.position += movement;
            return;
        }

        ResolveWallOverlap();

        Vector3 remaining = movement;

        for (int i = 0; i < wallSlideIterations; i++)
        {
            if (remaining.sqrMagnitude <= 0.000001f)
                break;

            Vector3 direction =
                remaining.normalized;

            float distance =
                remaining.magnitude;

            if (!CheckWall(
                direction,
                distance + wallCheckDistance,
                out RaycastHit hit))
            {
                transform.position += remaining;
                break;
            }

            float safeDistance =
                Mathf.Max(
                    0f,
                    hit.distance - wallSkin
                );

            if (safeDistance > 0f)
            {
                transform.position +=
                    direction * safeDistance;
            }

            Vector3 usedMovement =
                direction * safeDistance;

            Vector3 leftover =
                remaining -
                usedMovement;

            Vector3 slide =
                Vector3.ProjectOnPlane(
                    leftover,
                    hit.normal
                );

            remaining = slide;
        }

        ResolveWallOverlap();
    }

    bool CheckWall(
        Vector3 direction,
        float distance,
        out RaycastHit closestHit)
    {
        closestHit = default;

        Bounds bounds =
            dogCollider.bounds;

        Vector3 center =
            bounds.center;

        float radius =
            Mathf.Min(
                bounds.extents.x,
                bounds.extents.z
            );

        float height =
            bounds.size.y;

        RaycastHit[] hits;

        if (height > radius * 2f)
        {
            Vector3 bottom =
                center +
                Vector3.down *
                (height * 0.5f - radius);

            Vector3 top =
                center +
                Vector3.up *
                (height * 0.5f - radius);

            hits =
                Physics.CapsuleCastAll(
                    bottom,
                    top,
                    radius,
                    direction,
                    distance,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore
                );
        }
        else
        {
            hits =
                Physics.SphereCastAll(
                    center,
                    radius,
                    direction,
                    distance,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore
                );
        }

        return GetClosestWallHit(
            hits,
            out closestHit
        );
    }

    bool GetClosestWallHit(
        RaycastHit[] hits,
        out RaycastHit closestHit)
    {
        closestHit = default;

        float closestDistance =
            float.MaxValue;

        bool found = false;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider =
                hits[i].collider;

            if (hitCollider == null)
                continue;

            if (hitCollider == dogCollider)
                continue;

            if (hitCollider.transform == transform)
                continue;

            if (hitCollider.transform.IsChildOf(transform))
                continue;

            if (!hitCollider.CompareTag(wallTag) &&
                !hitCollider.transform.root.CompareTag(wallTag))
                continue;

            if (hits[i].distance < closestDistance)
            {
                closestDistance =
                    hits[i].distance;

                closestHit =
                    hits[i];

                found = true;
            }
        }

        return found;
    }

    void ResolveWallOverlap()
    {
        if (dogCollider == null)
            return;

        Collider[] overlaps =
            Physics.OverlapBox(
                dogCollider.bounds.center,
                dogCollider.bounds.extents +
                Vector3.one * wallSkin,
                Quaternion.identity,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            );

        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider wall =
                overlaps[i];

            if (wall == null)
                continue;

            if (wall == dogCollider)
                continue;

            if (wall.transform == transform)
                continue;

            if (wall.transform.IsChildOf(transform))
                continue;

            if (!wall.CompareTag(wallTag) &&
                !wall.transform.root.CompareTag(wallTag))
                continue;

            bool penetrating =
                Physics.ComputePenetration(
                    dogCollider,
                    transform.position,
                    transform.rotation,
                    wall,
                    wall.transform.position,
                    wall.transform.rotation,
                    out Vector3 direction,
                    out float distance
                );

            if (penetrating)
            {
                transform.position +=
                    direction *
                    (distance + wallSkin);
            }
        }
    }

    bool IsGrounded()
    {
        if (dogCollider == null)
            return false;

        Vector3 origin =
            dogCollider.bounds.center;

        origin.y =
            dogCollider.bounds.min.y +
            0.05f;
        
        
        bool isGrounded = Physics.Raycast(
            origin,
            Vector3.down,
            out hit,
            groundCheckDistance + 0.5f,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (isGrounded)
        {
            transform.up = hit.normal;
        }

        return isGrounded;
    }

    void KeepDogUpright()
    {
        Vector3 currentEuler =
            transform.rotation.eulerAngles;

        float xAngle =
            NormalizeAngle(currentEuler.x);

        float zAngle =
            NormalizeAngle(currentEuler.z);

        xAngle =
            Mathf.Clamp(
                xAngle,
                -maxTiltAngle,
                maxTiltAngle
            );

        zAngle =
            Mathf.Clamp(
                zAngle,
                -maxTiltAngle,
                maxTiltAngle
            );

        Quaternion targetRotation =
            Quaternion.Euler(
                xAngle,
                currentEuler.y,
                zAngle
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                uprightSmoothness *
                Time.deltaTime
            );
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }

    public bool IsQuickTurning()
    {
        return quickTurning;
    }
    public void ApplyFireWork(float duration, float jumpStrength)
    {
        fireworkTimer = duration;
        fireworkjump = jumpStrength;

    }
    public void ApplyPepper(float duration, float multi)
    {
        pepperTimer = duration;
        pepperMult = multi;

    }
        public void ApplyBone(float duration, float scale)
    {
        boneTimer = duration;
        boneScale = scale;

    }
}
