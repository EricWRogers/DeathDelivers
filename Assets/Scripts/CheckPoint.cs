using UnityEngine;

public class CheckPoint : MonoBehaviour
{

    public static int checkPointCount = 0;
    public static Transform CurrentTarget{ get; private set; }

    private static CheckPoint Instance;
    private GameObject[] Checkpoint;
    private int currentIndex = -1;

    private void Awake()
    {
        if(Instance = this)
        {
            CurrentTarget = null;
        }
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
        if(dog.hasPackage)
        {
            dog.SetHasPackage(false);
            for(int i = 0; i < transform.childCount; i++)
            {
                Checkpoint[i] = transform.GetChild(i).gameObject;
                Checkpoint[i].SetActive(true);
            }

            checkPointCount++;
            SelectRandCheckPoint();
        }
        
    }
    private void SelectRandCheckPoint()
    {
        CheckPoint[] checkpoints = FindObjectsByType<CheckPoint>(FindObjectsSortMode.None);
        int candidateCount = 0;
        foreach(CheckPoint checkpoint in checkpoints)
        {
            if(checkpoint != this)
            {
                candidateCount++;
            }
        }

        if(candidateCount == 0)
        {
            CurrentTarget = transform;
            return;
        }

        int selectedIndex = Random.Range(0, candidateCount);
        foreach(CheckPoint checkpoint in checkpoints)
        {
            if(checkpoint != this)
            {
                if(selectedIndex == 0)
                {
                    CurrentTarget = checkpoint.transform;
                    return;
                }
                selectedIndex--;
            }
        }
    }

        
    
}
