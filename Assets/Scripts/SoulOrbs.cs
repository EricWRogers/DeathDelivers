using UnityEngine;

public class SoulOrbs : MonoBehaviour
{
    public int state = 0; // 0 = shoot out, 1 = shoot in
    public float outTime = 1.4f;
    public float inSpeed = 4f;
    public GameObject goTo;
    private float m_outTime = 0f;

    void Start()
    {
        transform.localEulerAngles = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));        
    }

    // Update is called once per frame
    void Update()
    {
        if (state == 0)
        {
            m_outTime += Time.deltaTime;
            transform.position += transform.forward * Time.deltaTime * (inSpeed * 2f);
            if (m_outTime >= outTime)
            {
                state = 1;
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, goTo.transform.position, Time.deltaTime * inSpeed);
        }
    }
}
