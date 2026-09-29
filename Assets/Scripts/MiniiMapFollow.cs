using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player;
    public float height = 10f;

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPosition = player.position;
            newPosition.y += height;
            transform.position = newPosition;
            
            // Keep camera fixed upright, ignoring player rotation
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
