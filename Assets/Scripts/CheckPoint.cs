using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public Vector2 spawnMin;
    public Vector2 spawnMax;
    public float yHeight = 1f;
    public int checkPointCount = 0;

    private void OnTriggerEnter(Collider other){
        DogMovement dog = other.GetComponentInParent<DogMovement>();
            if(dog != null){
                RespawnCheckPoint();
                checkPointCount++;
       }
    }
    private void RespawnCheckPoint()
    {
        float randx = Random.Range(spawnMin.x, spawnMax.x);
        float randz = Random.Range(spawnMin.y, spawnMax.y);

        transform.position = new Vector3(randx, yHeight, randz);
    }
}
