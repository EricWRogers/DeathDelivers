using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject spawnThis;
    public float timeBetweenSpawns;
    private float tracker;
    public int maxSpawns = 10;

    // Update is called once per frame
    void Update()
    {
        tracker += Time.deltaTime;

        if (tracker >= timeBetweenSpawns && maxSpawns > 0)
        {
            tracker = 0f;
            maxSpawns--;
            Instantiate(spawnThis, transform.position, transform.rotation);
        }
    }
}
