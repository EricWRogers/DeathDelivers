using UnityEngine;

public class BoneSpawn : MonoBehaviour
{
    public GameObject bonePrefab;
    public float respawnDelay = 3f;

   private GameObject bone;
   private float timer = 0f;
   void Start(){
    timer = respawnDelay;
   }
    // Update is called once per frame
    void Update()
    {
        if(bone != null)
            return;
        timer -= Time.deltaTime;
        if(timer <= 0f){
            Spawnbone();
            timer = respawnDelay;
        }
        }
       public void boneGrabbed()
    {
        bone = null;
        timer = respawnDelay;
    }
    void Spawnbone(){
        bone = Instantiate(bonePrefab, transform.position, Quaternion.identity);
    }
}
