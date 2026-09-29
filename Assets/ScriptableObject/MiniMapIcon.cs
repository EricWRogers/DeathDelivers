using UnityEngine;

public class MiniMapIcon : MonoBehaviour
{
    public GameObject miniIcon;
    public bool isTurn;
    void Update()
    {
        miniIcon.transform.position = new Vector3(transform.position.x, transform.position.y + 50f, transform.position.z);
        if (isTurn)
        {
            miniIcon.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + 90f, 0f);
        }
    }
}
