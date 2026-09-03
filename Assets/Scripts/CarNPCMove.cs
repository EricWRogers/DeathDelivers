using UnityEngine;

public class CarNPCMove : MonoBehaviour
{
    public float speed = 1.0f;
    public Transform targetPoint;

    public float warnedDist = 20.0f;
    public float scaredTurnAmount = 20.0f;

    private bool isScared = false;
    private float chaosTimer = 0f;

    public float chaosTurningAmount = 0.9f;

    private float scaredTimer = 0f;
    private float dT;

    void Update()
    {
        dT = Time.deltaTime;

        if (isScared == false)
        {
            float step = speed * dT;

            // Move toward the target point.
            if (targetPoint != null)
            {
                transform.position = Vector3.MoveTowards(transform.position,targetPoint.position,step);

                // Smoothly rotate to face the target point.
                Vector3 direction = targetPoint.position - transform.position;
                direction.y = 0f;

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    transform.rotation = Quaternion.Slerp(transform.rotation,targetRotation,5f * dT);
                }
            }

            // Check for the Player in front of the car.
            RaycastHit hit;

            if (Physics.Raycast(transform.position,transform.forward,out hit,warnedDist))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    isScared = true;
                    chaosTimer = 0f;
                    scaredTimer = 0f;
                }
            }
        }
        else
        {
            chaosTimer += dT;
            scaredTimer += dT;

            // Drive forward while scared.
            transform.Translate(
                Vector3.forward * speed * dT
            );

            // Turn while scared.
            transform.Rotate(
                Vector3.up * scaredTurnAmount * dT
            );

            // Occasionally change turning direction.
            if (chaosTimer >= chaosTurningAmount)
            {
                chaosTimer = 0f;

                int rand = Random.Range(1, 3);

                if (rand == 2)
                {
                    scaredTurnAmount *= -1;
                }
            }

            // Stop being scared after 3 seconds.
            if (scaredTimer >= 3f)
            {
                scaredTimer = 0f;
                isScared = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Explode/delete if the car hits the Player.
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }

        // Explode/delete if the car hits a wall.
        if (collision.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
