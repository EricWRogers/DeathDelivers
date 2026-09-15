using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject spawnThis;
    public float timeBetweenSpawns;
    private float tracker;

    // Update is called once per frame
    void Update()
    {
        tracker += Time.deltaTime;

        if (tracker >= timeBetweenSpawns)
        {
            tracker = 0f;
            Instantiate(spawnThis, transform.position, transform.rotation);
        }
    }
}
