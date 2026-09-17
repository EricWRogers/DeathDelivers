using UnityEngine;

public class ParticTrack : MonoBehaviour
{
    public Transform tracking;
    private float positionSmoothness = 1f;

    // Update is called once per frame
    void Update()
    {
        Vector3 trackPos = tracking.position;
        transform.position = Vector3.Lerp(
            transform.position,
            trackPos,
            positionSmoothness * Time.deltaTime
        );
    }
}
