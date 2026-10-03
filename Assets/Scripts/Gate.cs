using UnityEngine;

public class Gate : MonoBehaviour
{
    public float raiseHeight = 5f;
    public float raiseSpeed = 3f;
    private bool isRaising = false;

private void Update()
    {
        if (isRaising)
        {
            Vector3 target =  new Vector3(transform.position.x, raiseHeight, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, target, raiseSpeed * Time.deltaTime);
        }
    }
private void OnTriggerEnter(Collider other){
        DogMovement dog = other.GetComponentInParent<DogMovement>();
        CheckPoint checkPoint = other.GetComponentInParent<CheckPoint>();
        if(dog != null && checkPoint != null && checkPoint.checkPointCount >= 5)
        {
            isRaising = true;
        }
}
}
