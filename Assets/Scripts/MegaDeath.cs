using UnityEngine;

public class MegaDeath : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public float spawnDistance = 12f;
    public float spawnHeight = 0f;

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

    void Update()
    {
        if (isEnemy)
        {
            ChasePlayer();
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

            Vector3 movement = direction.normalized * chaseSpeed * Time.deltaTime;
            transform.position += movement;
        }
    }
}
