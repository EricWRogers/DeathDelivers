using UnityEngine;

public class PersonBehavior : MonoBehaviour
{
    public enum PersonState
    {
        Roaming,
        Scared
    }

    [Header("State")] // Just to keep track for bug testing
    public PersonState state = PersonState.Roaming;
    [Header("Soul Value")]
    public int soulValue = 1;

    [Header("Movement")] // how fast it moves
    public float speed = 1.0f;
    public float scaredSpeed = 4.0f;

    [Header("Roaming")] // roman settings
    public float directionChangeTime = 2.0f;
    public float sidewalkCheckDistance = 1.5f;

    [Header("Detection")] // how good is its vision
    public float playerDetectionRadius = 5.0f;
    public float dangerDetectionRadius = 8.0f;
    public float calmDownDistance = 12.0f;

    [Header("References")]
    public Camera camera;
    public Transform body;
    public GameObject partical;
    public GameObject parentBox;

    [Header("Joy / Bobbing")] // jumping with joy
    public float bobHeight = 0.2f;
    public float bobSpeed = 5.0f;

    private Vector3 bodyStartPosition;

    private Vector3 roamingDirection;
    private float directionTimer;

    void Start()
    {
        if (camera == null)
            camera = Camera.main;

        if (body != null)
            bodyStartPosition = body.localPosition;
        ChooseRandomDirection();
    }

    void Update()
    {
        Bob();

        switch (state)
        {
            case PersonState.Roaming:
                Roam();
                break;

            case PersonState.Scared:
                Scared();
                break;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Explode/delete if the car hits the Player.
        if (collision.gameObject.CompareTag("Player"))
        {
            DogMovement dM = collision.gameObject.GetComponent<DogMovement>();
            dM.soulCount += soulValue;
            dM.totalSoulCount += soulValue;
            parentBox.GetComponent<BoxCollider>().enabled = false;
            partical.transform.position = transform.position;
            partical.SetActive(true);
            gameObject.SetActive(false);
        }

        // Explode/delete if the car hits a wall.
        if (collision.gameObject.CompareTag("Car"))
        {
            parentBox.GetComponent<BoxCollider>().enabled = false;
            partical.transform.position = transform.position;
            partical.SetActive(true);
            gameObject.SetActive(false);
        }
    }
    void Roam()
    {
        // Look for Player
        GameObject player = FindClosestObjectWithTag("Player");

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position,player.transform.position);

            if (distance <= playerDetectionRadius)
            {
                state = PersonState.Scared;
                return;
            }
        }

        directionTimer -= Time.deltaTime;

        if (directionTimer <= 0f)
        {
            ChooseRandomDirection();
        }

        // Check if the direction we're trying to walk in
        // will still be on a sidewalk.
        if (DirectionLeadsToSidewalk())
        {
            // Turn toward our random direction
            Quaternion targetRotation = Quaternion.LookRotation(roamingDirection);

            transform.rotation = Quaternion.Slerp(transform.rotation,targetRotation,5f * Time.deltaTime);

            // Move
            transform.position += roamingDirection * speed * Time.deltaTime;
        }
        else
        {
            ChooseRandomDirection();
        }
    }

    void ChooseRandomDirection()
    {
        // Pick a completely random direction
        roamingDirection = Random.insideUnitSphere;
        roamingDirection.y = 0f;

        if (roamingDirection == Vector3.zero)
            roamingDirection = Vector3.forward;

        roamingDirection.Normalize();

        directionTimer = directionChangeTime;
    }

    bool DirectionLeadsToSidewalk()
    {
        Vector3 checkPosition = transform.position + roamingDirection * sidewalkCheckDistance;

        // Start the ray above the expected position
        Vector3 rayStart = checkPosition + Vector3.up * 2f;

        RaycastHit hit;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 4f))
        {
            return hit.collider.CompareTag("Sidewalk");
        }
        return false;
    }

    void Scared()
    {
        GameObject player = FindClosestObjectWithTag("Player");
        GameObject car = FindClosestObjectWithTag("Car");

        Transform closestDanger = null;
        float closestDistance = Mathf.Infinity;

        // Check Player
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position,player.transform.position);

            if (distance < closestDistance && distance <= dangerDetectionRadius)
            {
                closestDistance = distance;
                closestDanger = player.transform;
            }
        }

        // Check Car
        if (car != null)
        {
            float distance = Vector3.Distance(transform.position,car.transform.position);

            if (distance < closestDistance && distance <= dangerDetectionRadius)
            {
                closestDistance = distance;
                closestDanger = car.transform;
            }
        }
        // Run away from scary cars and stuffs
        if (closestDanger != null)
        {
            Vector3 awayDirection = transform.position - closestDanger.position;
            awayDirection.y = 0f;

            if (awayDirection != Vector3.zero)
            {
                awayDirection.Normalize();

                // Turn toward escape direction
                Quaternion targetRotation = Quaternion.LookRotation(awayDirection);

                transform.rotation = Quaternion.Slerp(transform.rotation,targetRotation,8f * Time.deltaTime);

                // Run / Movement from things
                transform.position += awayDirection * scaredSpeed * Time.deltaTime;
            }
        }
        else
        {
            state = PersonState.Roaming;
            ChooseRandomDirection();
        }
    }

    void Bob()
    {
        if (body == null)
            return;

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        body.localPosition = bodyStartPosition + Vector3.up * bob;
    }
    GameObject FindClosestObjectWithTag(string tag)
    {
        // this is simply to make earlier stuff easier
        GameObject[] objects;

        try
        {
            objects = GameObject.FindGameObjectsWithTag(tag);
        }
        catch
        {
            return null;
        }

        GameObject closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject obj in objects)
        {
            float distance = Vector3.Distance(transform.position,obj.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = obj;
            }
        }
        return closest;
    }
    void LateUpdate()
    {
        if (camera == null)
            return;

        transform.LookAt(camera.transform);

        Vector3 currentRotation = transform.eulerAngles;

        currentRotation.x += 90f;

        transform.eulerAngles = currentRotation;
    }
}