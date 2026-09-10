
using UnityEngine;

public class Pepper : MonoBehaviour
{
    private GameObject DogV1Prefab;
    public float rotationSpeed = 35f;
    public float bobbingSpeed = 0.5f;
    public float bobbingHeight = 0.25f;
    public float startY;
   //Start is called once before the first execution of Update after the MonoBehaviour is created
   private void Start()
    {
       startY = transform.position.y; 
    }

     //Update is called once per frame
    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        float newY = startY + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
        transform.position = new Vector3(transform.position.x, (newY*0.5f), transform.position.z);
    }

    private void OnTriggerEnter(Collider other){
        DogMovement dog = other.GetComponentInParent<DogMovement>();
        if(dog != null){
            dog.AddPepper();
            Destroy(gameObject);
        }
    }
}
