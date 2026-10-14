using UnityEngine;

public class CheckPoint : MonoBehaviour
{

    public static int checkPointCount = 0;

    private static CheckPoint Instance;
    private GameObject[] Checkpoint;
    private int currentIndex = -1;

    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        Checkpoint = new GameObject[transform.childCount];
        for(int i = 0; i < transform.childCount; i++)
        {
            Checkpoint[i] = transform.GetChild(i).gameObject;
            Checkpoint[i].SetActive(false);
        }
    }
    private void OnTriggerEnter(Collider other){
        DogMovement dog = other.GetComponentInParent<DogMovement>();
            if(dog != null){
               DeliverPackage(other); 
                
       }
    }
    public void DeliverPackage(Collider other)
    {
        DogMovement dog = other.GetComponentInParent<DogMovement>();
        if(dog.hasPackage == true)
        {
            dog.SetHasPackage(false);
            TurningOnCheckPoint();
            checkPointCount++;
            SelectRandCheckPoint();
        }
        
    }
    private void SelectRandCheckPoint()
    {
        if(currentIndex != -1)
        {
            Checkpoint[currentIndex].SetActive(false);
        }
        currentIndex = Random.Range(0, Checkpoint.Length);
        Checkpoint[currentIndex].SetActive(true);
    }
    private void TurningOnCheckPoint()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            Checkpoint[i] = transform.GetChild(i).gameObject;
            Checkpoint[i].SetActive(true);
        }
    }
}
