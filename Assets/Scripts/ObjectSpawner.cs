
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject spawnThis;
    public float timeBetweenSpawns;
    private float tracker;
    public int maxSpawns = 10;

    public bool hasRares = false;
    public float rareChance = 0.25f;
    public GameObject[] rareSpawns;

    void Update()
    {
        tracker += Time.deltaTime;

        if (tracker >= timeBetweenSpawns && maxSpawns > 0)
        {
            tracker = 0f;
            maxSpawns--;

            GameObject spawnObject = spawnThis;

            if (hasRares && rareSpawns.Length > 0 && Random.value <= rareChance)
            {
                spawnObject = rareSpawns[Random.Range(0, rareSpawns.Length)];
            }

            Instantiate(spawnObject, transform.position, transform.rotation);
        }
    }
}