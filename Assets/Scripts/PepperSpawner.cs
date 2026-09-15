using UnityEngine;

public class PepperSpawner : MonoBehaviour
{
    public GameObject pepperPrefab;
    public float respawnDelay = 3f;

   private GameObject pepper;
   private float timer = 0f;
   void Start(){
    timer = respawnDelay;
   }
    // Update is called once per frame
    void Update()
    {
        if(pepper != null)
            return;
        timer -= Time.deltaTime;
        if(timer <= 0f){
            Spawnpepper();
            timer = respawnDelay;
        }
        }
       public void PepperGrabbed()
    {
        pepper = null;
        timer = respawnDelay;
    }
    void Spawnpepper(){
        pepper = Instantiate(pepperPrefab, transform.position, Quaternion.identity);
    }
}

