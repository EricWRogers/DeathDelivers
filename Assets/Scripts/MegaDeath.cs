using UnityEngine;

public class MegaDeath : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public float spawnDistance = 12f;
    public float spawnHeight = 5f;

    [Header("Air Drift Settings")]
    public float driftSpeed = 8f;
    public float approachDescentAmount = 0.7f;
    public float finalDescentSpeed = 3f;
    public float landingHeight = 0f;
    public float approachDistance = 3f;

    [Header("Chase Settings")]
    public float speedMultiplier = 1.15f;
    public float minimumChaseSpeed = 5f;

    [Header("References")]
    public Timer timer;
    public DogMovement playerMovement;
    public Transform player;

    [Header("Enemy Mode")]
    public bool isEnemy = false;

    private bool enemySpawned = false;
    private bool isDescending = false;
    private bool hasReachedPlayerPosition = false;

    private float startingHeight;
    private float targetHeight;

    void Update()
    {
        if (isEnemy)
        {
            if (!hasReachedPlayerPosition)
            {
                DriftToPlayer();
            }
            else if (isDescending)
            {
                DescendToPlayer();
            }
            else
            {
                ChasePlayer();
            }

            return;
        }

        CheckTimer();
    }

    void CheckTimer()
    {
        if (enemySpawned || timer == null || timer.timerText == null)
            return;

        if (timer.timerText.text == "Time: 0.00")
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || player == null)
            return;

        enemySpawned = true;

        Vector3 spawnPosition =
            player.position - player.forward * spawnDistance;

        spawnPosition.y += spawnHeight;

        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPosition,
            Quaternion.LookRotation(player.position - spawnPosition)
        );

        MegaDeath enemyScript = enemy.GetComponent<MegaDeath>();

        if (enemyScript != null)
        {
            enemyScript.isEnemy = true;
            enemyScript.player = player;
            enemyScript.playerMovement = playerMovement;
            enemyScript.startingHeight = spawnPosition.y;
            enemyScript.isDescending = false;
            enemyScript.hasReachedPlayerPosition = false;
        }
    }

    void DriftToPlayer()
    {
        if (player == null)
            return;

        Vector3 targetPosition = new Vector3(
            player.position.x,
            transform.position.y,
            player.position.z
        );

        Vector3 horizontalDifference = targetPosition - transform.position;
        float horizontalDistance = horizontalDifference.magnitude;

        if (horizontalDistance > 0.05f)
        {
            Vector3 horizontalDirection = horizontalDifference.normalized;

            float distanceFactor = Mathf.Clamp01(
                horizontalDistance / Mathf.Max(approachDistance, 0.1f)
            );

            float verticalMovement = approachDescentAmount
                * (1f - distanceFactor)
                * Time.deltaTime;

            transform.position += horizontalDirection
                * driftSpeed
                * Time.deltaTime;

            float desiredHeight = player.position.y
                + landingHeight
                + spawnHeight * distanceFactor;

            Vector3 position = transform.position;

            position.y = Mathf.MoveTowards(
                position.y,
                desiredHeight,
                verticalMovement
            );

            transform.position = position;

            Vector3 lookDirection = horizontalDirection;

            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    3f * Time.deltaTime
                );
            }
        }
        else
        {
            hasReachedPlayerPosition = true;
            isDescending = true;
            targetHeight = player.position.y + landingHeight;
        }
    }

    void DescendToPlayer()
    {
        if (player == null)
            return;

        targetHeight = player.position.y + landingHeight;

        Vector3 position = transform.position;

        position.y = Mathf.MoveTowards(
            position.y,
            targetHeight,
            finalDescentSpeed * Time.deltaTime
        );

        transform.position = position;

        if (Mathf.Abs(position.y - targetHeight) < 0.05f)
        {
            position.y = targetHeight;
            transform.position = position;

            isDescending = false;
        }
    }

    void ChasePlayer()
    {
        if (player == null)
            return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                8f * Time.deltaTime
            );

            float playerSpeed = playerMovement != null
                ? playerMovement.GetMaxSpeed()
                : minimumChaseSpeed;

            float chaseSpeed = Mathf.Max(
                playerSpeed * speedMultiplier,
                minimumChaseSpeed
            );

            transform.position += direction.normalized
                * chaseSpeed
                * Time.deltaTime;
        }
    }
}
