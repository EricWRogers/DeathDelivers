using UnityEngine;
using UnityEngine.InputSystem;

public class DogMovement : MonoBehaviour
{
    public DogBase dogBase;

    [Header("Soul & Speed")]
    public float soulCount;
    public int totalSoulCount;
    public float speed;

    [Header("Movement")]
    public float acceleration = 18f;
    public float naturalDeceleration = 7f;
    public float braking = 28f;
    public float reverseSpeedMultiplier = 0.55f;
    public float reverseAcceleration = 14f;
    public float reverseDeceleration = 10f;
    public float stopThreshold = 0.05f;

    [Header("Turning")]
    public float turn;
    public float turnSmoothness = 10f;
    public float uprightSmoothness = 18f;
    public float lowSpeedTurnMultiplier = 0.45f;

    [Header("Ground")]
    public float groundCheckDistance = 0.35f;
    public float groundRayHeight = 0.5f;
    public float groundRayInset = 0.05f;
    public LayerMask groundLayer;
    public float groundAngleLerpSpeed = 15f;

    [Header("Quick Turn")]
    public float quickTurnDuration = 0.25f;
    public float quickTurnMultiplier = 1f;
    public Transform AnchorL;
    public Transform AnchorR;

    [Header("Air Turning")]
    public float maxAirTurnAngle = 90f;
    public float airTurnSmoothness = 10f;
    public float airUprightSmoothness = 25f;

    [Header("Wall Collision")]
    public string wallTag = "Wall";
    public float wallSkin = 0.03f;
    public float wallCheckDistance = 0.1f;
    public int wallSlideIterations = 3;

    private float fireworkTimer, fireworkJump;
    private float pepperTimer, pepperMult;
    private float boneTimer, boneScale;

    private float currentSpeed;
    private float targetTurn;
    private float airTurnAngle;
    private float airTurnInput;

    private bool grounded, wasGrounded, quickTurning;
    private float quickTurnDirection;

    private Vector3 groundNormal = Vector3.up;
    private Vector3 airForward;

    private Transform activeAnchor;
    private Vector3 anchorPosition;
    private Vector3 anchorOffset;

    private Collider dogCollider;
    private Vector3 startScale;


    void Start()
    {
        dogCollider = GetComponent<Collider>();
        startScale = transform.localScale;

        soulCount = 30f;
        totalSoulCount += 30;

        speed = soulCount * dogBase.speed;
        currentSpeed = 0f;

        airForward = HorizontalForward();
    }


    void Update()
    {
        float dt = Time.deltaTime;

        soulCount = Mathf.Max(
            0f,
            soulCount - dogBase.soulConsump * dt
        );

        speed = soulCount * dogBase.speed;

        UpdateGround();
        GroundStateChange();

        UpdatePowerups(dt);
        HandleQuickTurn();

        if (quickTurning)
        {
            if (!grounded)
                EndQuickTurn();
            else if (Keyboard.current.spaceKey.isPressed)
            {
                DoQuickTurn();
                return;
            }
            else
                EndQuickTurn();
        }

        Move(dt);
        Turn(dt);

        if (grounded)
            GroundUpright(dt);
        else
            AirUpright(dt);
    }


    Vector3 HorizontalForward()
    {
        Vector3 f = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        if (f.sqrMagnitude < 0.001f)
            f = Vector3.ProjectOnPlane(transform.right, Vector3.up);

        return f.sqrMagnitude > 0.001f ? f.normalized : Vector3.forward;
    }


    void Move(float dt)
    {
        if (Keyboard.current == null)
            return;

        bool w = Keyboard.current.wKey.isPressed;
        bool s = Keyboard.current.sKey.isPressed;

        float target = 0f;

        if (w && !s)
            target = speed;
        else if (s && !w)
            target = -speed * reverseSpeedMultiplier;

        float accel;

        if (w && !s)
        {
            accel = currentSpeed < 0f ? braking : acceleration;
        }
        else if (s && !w)
        {
            accel = currentSpeed > 0f ? braking : reverseAcceleration;
        }
        else
        {
            accel = currentSpeed >= 0f
                ? naturalDeceleration
                : reverseDeceleration;
        }

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            target,
            accel * dt
        );

        currentSpeed = Mathf.Clamp(
            currentSpeed,
            -speed * reverseSpeedMultiplier,
            speed
        );

        if (Mathf.Abs(currentSpeed) < stopThreshold)
            currentSpeed = 0f;

        if (currentSpeed == 0f)
            return;

        Vector3 direction = grounded
            ? Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized
            : airForward;

        MoveWithWalls(direction * currentSpeed * dt);
    }


    void Turn(float dt)
    {
        if (Keyboard.current == null)
            return;

        targetTurn = 0f;

        if (Keyboard.current.aKey.isPressed)
            targetTurn = -dogBase.turnSpeed;

        if (Keyboard.current.dKey.isPressed)
            targetTurn = dogBase.turnSpeed;

        turn = Mathf.Lerp(
            turn,
            targetTurn,
            1f - Mathf.Exp(-turnSmoothness * dt)
        );

        if (grounded)
            GroundTurn(dt);
        else
            AirTurn(dt);
    }


    void GroundTurn(float dt)
    {
        if (Mathf.Abs(turn) < 0.001f)
            return;

        float speedRatio = speed > 0.01f
            ? Mathf.Clamp01(Mathf.Abs(currentSpeed) / speed)
            : 0f;

        float strength = Mathf.Lerp(
            lowSpeedTurnMultiplier,
            1f,
            speedRatio
        );

        float direction = currentSpeed < 0f ? -1f : 1f;

        transform.Rotate(
            groundNormal,
            turn * strength * direction * dt,
            Space.World
        );
    }


    void AirTurn(float dt)
    {
        float input = 0f;

        if (Keyboard.current.aKey.isPressed)
            input = -1f;

        if (Keyboard.current.dKey.isPressed)
            input = 1f;

        airTurnInput = Mathf.Lerp(
            airTurnInput,
            input,
            1f - Mathf.Exp(-airTurnSmoothness * dt)
        );

        if (Mathf.Abs(airTurnInput) < 0.001f)
            return;

        float requested =
            airTurnInput * dogBase.turnSpeed * dt;

        float remaining =
            maxAirTurnAngle - Mathf.Abs(airTurnAngle);

        if (remaining <= 0f)
            return;

        float rotation =
            Mathf.Clamp(requested, -remaining, remaining);

        airForward =
            Quaternion.AngleAxis(rotation, Vector3.up) *
            airForward;

        airForward =
            Vector3.ProjectOnPlane(airForward, Vector3.up).normalized;

        airTurnAngle =
            Mathf.Clamp(
                airTurnAngle + rotation,
                -maxAirTurnAngle,
                maxAirTurnAngle
            );
    }


    void UpdateGround()
    {
        if (!dogCollider)
        {
            grounded = false;
            return;
        }

        Bounds b = dogCollider.bounds;
        Vector3 c = b.center;

        float x = Mathf.Max(0.01f, b.extents.x - groundRayInset);
        float z = Mathf.Max(0.01f, b.extents.z - groundRayInset);
        float y = b.min.y + groundRayHeight;
        float distance = groundRayHeight + groundCheckDistance;

        Vector3[] points =
        {
            new(c.x - x, y, c.z + z),
            new(c.x + x, y, c.z + z),
            new(c.x - x, y, c.z - z),
            new(c.x + x, y, c.z - z)
        };

        Vector3 normal = Vector3.zero;
        int hits = 0;

        foreach (Vector3 point in points)
        {
            if (Physics.Raycast(
                point,
                Vector3.down,
                out RaycastHit hit,
                distance,
                groundLayer,
                QueryTriggerInteraction.Ignore))
            {
                normal += hit.normal;
                hits++;
            }
        }

        bool centerHit = Physics.Raycast(
            new Vector3(c.x, y, c.z),
            Vector3.down,
            out RaycastHit center,
            distance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (!centerHit)
        {
            grounded = false;
            return;
        }

        grounded = true;

        Vector3 targetNormal =
            hits > 0 ? (normal / hits).normalized : center.normal;

        float lerp =
            1f - Mathf.Exp(-groundAngleLerpSpeed * Time.deltaTime);

        groundNormal =
            Vector3.Slerp(
                groundNormal,
                targetNormal,
                lerp
            ).normalized;

        if (Vector3.Dot(groundNormal, Vector3.up) < 0f)
            groundNormal = -groundNormal;
    }


    void GroundStateChange()
    {
        if (grounded && !wasGrounded)
        {
            airTurnAngle = 0f;
            airTurnInput = 0f;

            Vector3 forward =
                Vector3.ProjectOnPlane(
                    transform.forward,
                    groundNormal
                );

            if (forward.sqrMagnitude > 0.001f)
                transform.rotation =
                    Quaternion.LookRotation(
                        forward.normalized,
                        groundNormal
                    );
        }

        if (!grounded && wasGrounded)
        {
            airTurnAngle = 0f;
            airTurnInput = 0f;
            airForward = HorizontalForward();

            ForceAirUpright();

            if (quickTurning)
                EndQuickTurn();
        }

        wasGrounded = grounded;
    }


    void GroundUpright(float dt)
    {
        Vector3 forward =
            Vector3.ProjectOnPlane(
                transform.forward,
                groundNormal
            );

        if (forward.sqrMagnitude < 0.001f)
            return;

        Quaternion target =
            Quaternion.LookRotation(
                forward.normalized,
                groundNormal
            );

        float t =
            1f - Mathf.Exp(-uprightSmoothness * dt);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                target,
                t
            );
    }


    void AirUpright(float dt)
    {
        Vector3 forward =
            Vector3.ProjectOnPlane(
                airForward,
                Vector3.up
            );

        if (forward.sqrMagnitude < 0.001f)
            return;

        Quaternion target =
            Quaternion.LookRotation(
                forward.normalized,
                Vector3.up
            );

        float t =
            1f - Mathf.Exp(-airUprightSmoothness * dt);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                target,
                t
            );
    }


    void ForceAirUpright()
    {
        Vector3 forward =
            Vector3.ProjectOnPlane(
                airForward,
                Vector3.up
            );

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        transform.rotation =
            Quaternion.LookRotation(
                forward.normalized,
                Vector3.up
            );
    }


    void HandleQuickTurn()
    {
        if (Keyboard.current == null)
            return;

        if (quickTurning && !grounded)
        {
            EndQuickTurn();
            return;
        }

        if (quickTurning)
            return;

        if (!grounded || !Keyboard.current.spaceKey.isPressed)
            return;

        if (Keyboard.current.dKey.isPressed)
            StartQuickTurn(1f);
        else if (Keyboard.current.aKey.isPressed)
            StartQuickTurn(-1f);
    }


    void StartQuickTurn(float direction)
    {
        activeAnchor =
            direction > 0f ? AnchorR : AnchorL;

        if (!activeAnchor)
            return;

        quickTurning = true;
        quickTurnDirection = direction;
        turn = 0f;

        anchorPosition = activeAnchor.position;
        anchorOffset = transform.position - anchorPosition;
    }


    void DoQuickTurn()
    {
        if (!grounded || !activeAnchor)
        {
            EndQuickTurn();
            return;
        }

        float rotationAmount =
            quickTurnDirection *
            speed *
            dogBase.turnSpeed *
            quickTurnMultiplier *
            Time.deltaTime;

        Quaternion rotation =
            Quaternion.AngleAxis(
                rotationAmount,
                groundNormal
            );

        Vector3 target =
            anchorPosition +
            rotation * anchorOffset;

        Vector3 movement =
            target - transform.position;

        MoveWithWalls(movement);

        anchorOffset =
            transform.position - anchorPosition;

        if (movement.sqrMagnitude > 0.000001f)
            transform.rotation =
                rotation * transform.rotation;

        GroundUpright(Time.deltaTime);
    }


    void EndQuickTurn()
    {
        quickTurning = false;
        activeAnchor = null;
        turn = 0f;
    }


    void UpdatePowerups(float dt)
    {
        if (fireworkTimer > 0f)
        {
            fireworkTimer -= dt;
            MoveWithWalls(Vector3.up * fireworkJump * dt);
        }

        if (pepperTimer > 0f)
        {
            pepperTimer -= dt;
            speed *= pepperMult;

            currentSpeed = Mathf.Clamp(
                currentSpeed,
                -speed * reverseSpeedMultiplier,
                speed
            );
        }

        if (boneTimer > 0f)
        {
            boneTimer -= dt;

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * boneScale, dt * 5f);
        }
        else
        {
            if (transform.localScale != startScale)
            transform.localScale = Vector3.Lerp(startScale, Vector3.one, dt * 5f);
        }
    }

    void MoveWithWalls(Vector3 movement)
    {
        if (movement.sqrMagnitude < 0.000001f)
            return;

        if (!dogCollider)
        {
            transform.position += movement;
            return;
        }

        ResolveWallOverlap();

        Vector3 remaining = movement;

        for (int i = 0; i < wallSlideIterations; i++)
        {
            if (remaining.sqrMagnitude < 0.000001f)
                break;

            Vector3 direction = remaining.normalized;
            float distance = remaining.magnitude;

            if (!CheckWall(
                direction,
                distance + wallCheckDistance,
                out RaycastHit hit))
            {
                transform.position += remaining;
                break;
            }

            float safe =
                Mathf.Max(0f, hit.distance - wallSkin);

            transform.position += direction * safe;

            remaining =
                Vector3.ProjectOnPlane(
                    remaining - direction * safe,
                    hit.normal
                );
        }

        ResolveWallOverlap();
    }


    bool CheckWall(
        Vector3 direction,
        float distance,
        out RaycastHit closest)
    {
        Bounds b = dogCollider.bounds;
        Vector3 center = b.center;

        float radius =
            Mathf.Min(b.extents.x, b.extents.z);

        RaycastHit[] hits;

        if (b.size.y > radius * 2f)
        {
            Vector3 bottom =
                center + Vector3.down *
                (b.size.y * 0.5f - radius);

            Vector3 top =
                center + Vector3.up *
                (b.size.y * 0.5f - radius);

            hits = Physics.CapsuleCastAll(
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
            hits = Physics.SphereCastAll(
                center,
                radius,
                direction,
                distance,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            );
        }

        closest = default;
        float closestDistance = float.MaxValue;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            Collider c = hit.collider;

            if (!c ||
                c == dogCollider ||
                c.transform == transform ||
                c.transform.IsChildOf(transform))
                continue;

            if (!c.CompareTag(wallTag) &&
                !c.transform.root.CompareTag(wallTag))
                continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closest = hit;
                found = true;
            }
        }

        return found;
    }


    void ResolveWallOverlap()
    {
        if (!dogCollider)
            return;

        Collider[] overlaps = Physics.OverlapBox(
            dogCollider.bounds.center,
            dogCollider.bounds.extents + Vector3.one * wallSkin,
            Quaternion.identity,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider wall in overlaps)
        {
            if (!wall ||
                wall == dogCollider ||
                wall.transform == transform ||
                wall.transform.IsChildOf(transform))
                continue;

            if (!wall.CompareTag(wallTag) &&
                !wall.transform.root.CompareTag(wallTag))
                continue;

            if (Physics.ComputePenetration(
                dogCollider,
                transform.position,
                transform.rotation,
                wall,
                wall.transform.position,
                wall.transform.rotation,
                out Vector3 direction,
                out float distance))
            {
                transform.position +=
                    direction * (distance + wallSkin);
            }
        }
    }


    public bool IsQuickTurning() => quickTurning;

    public float GetCurrentSpeed() => currentSpeed;

    public float GetMaxSpeed() => speed;

    public float GetAirTurnAngle() => airTurnAngle;

    public float GetGroundDistance()
    {
        if (!dogCollider)
            return Mathf.Infinity;

        return grounded ? GetGroundDistanceInternal() : Mathf.Infinity;
    }


    float GetGroundDistanceInternal()
    {
        Bounds b = dogCollider.bounds;

        Vector3 origin =
            new Vector3(
                b.center.x,
                b.min.y + groundRayHeight,
                b.center.z
            );

        if (Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            groundRayHeight + groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            return Mathf.Max(
                0f,
                hit.distance - groundRayHeight
            );
        }

        return Mathf.Infinity;
    }


    public void ApplyFireWork(
        float duration,
        float jumpStrength)
    {
        fireworkTimer = duration;
        fireworkJump = jumpStrength;
    }


    public void ApplyPepper(
        float duration,
        float multi)
    {
        pepperTimer = duration;
        pepperMult = multi;
    }


    public void ApplyBone(
        float duration,
        float scale)
    {
        boneTimer = duration;
        boneScale = scale;
    }
}