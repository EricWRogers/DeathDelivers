using UnityEngine;

public class FireworkSpawner : MonoBehaviour
{
    public GameObject FireworkPrefab;
    public float respawnDelay = 3f;

   private GameObject Firework;
   private float timer = 0f;
   void Start(){
    timer = respawnDelay;
   }
    // Update is called once per frame
    void Update()
    {
        if(Firework != null)
            return;
        timer -= Time.deltaTime;
        if(timer <= 0f){
            SpawnFirework();
            timer = respawnDelay;
        }
        }
    void SpawnFirework(){
        Firework = Instantiate(FireworkPrefab, transform.position, Quaternion.identity);
    }
}

