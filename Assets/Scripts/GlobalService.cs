using UnityEngine;

public class GlobalService : MonoBehaviour
{
    [SerializeField] private Transform player;
   
    [SerializeField] private Transform directionArrow;
    [SerializeField] private float arrowOrbitRadius = 1.5f;

    [SerializeField] private float arrowHeightOffset;
    [SerializeField] private float targetRefreshInterval = 0.25f;

    private DogMovement dog;
    private Transform currentTarget;
    private float nextTargetRefreshTime;
    private bool lastHisPackage;
    private bool hasCachedTargetMode;
    private Renderer[] arrowRenderers;
    private bool arrowVisible;
    private bool arrowVisibilityInitailzed;

    private void Awake()
    {
        if(player != null)
        {
            dog = player.GetComponentInParent<DogMovement>();
        }
        if(directionArrow != null)
        {
            arrowRenderers = directionArrow.GetComponentsInChildren<Renderer>(true);
        }
    }

    private void Update()
    {
        if(dog == null)
        {
            dog = FindAnyObjectByType<DogMovement>();
            if(dog == null)
            {
                return;
            }
            player = dog.transform;
        }

        if(directionArrow == null)
        {
            enabled = false;
            return;
        }

        bool targetModelChanged = !hasCachedTargetMode || dog.hasPackage != lastHisPackage;
        if(targetModelChanged || (Time.time >= nextTargetRefreshTime))
        {
            currentTarget = FindNearestTarget();
            nextTargetRefreshTime = Time.time + targetRefreshInterval;
            lastHisPackage = dog.hasPackage;
            hasCachedTargetMode = true;
        }

        if(currentTarget == null)
        {
            SetArrowVisible(false);
            return;
        }

        SetArrowVisible(true);
        Vector3 direction = currentTarget.position - player.position;
        direction.y = 0f;
        if(direction.sqrMagnitude > 0.001f)
        {
            direction.Normalize();
            directionArrow.position = player.position + direction * arrowOrbitRadius +
                Vector3.up * arrowHeightOffset;
            directionArrow.rotation = Quaternion.LookRotation(direction, Vector3.up) *
                Quaternion.Euler(0f, 90f, 0f);
        }
    }
    private void SetArrowVisible(bool visible)
    {
        if((arrowVisibilityInitailzed && arrowVisible == visible) || arrowRenderers == null)
        {
            return;
        }
        foreach(Renderer arrowRenderer in arrowRenderers)
        {
            arrowRenderer.enabled = visible;
        }
        arrowVisible = visible;
        arrowVisibilityInitailzed = true;
    }

    private Transform FindNearestTarget()
    {
        if(dog.hasPackage)
        {
            CheckPoint currentCheckpoint = CheckPoint.CurrentTarget != null ? CheckPoint.CurrentTarget.GetComponent<CheckPoint>() : null;
            if(currentCheckpoint != null && !currentCheckpoint.HasBeenActivated && 
                currentCheckpoint.gameObject.activeInHierarchy)
            {
                return currentCheckpoint.transform;
            }
            CheckPoint[] checkpoints = FindObjectsByType<CheckPoint>(FindObjectsSortMode.None);
            return FindNearest(checkpoints);
        }
        Package[] packages = FindObjectsByType<Package>(FindObjectsSortMode.None);
        return FindNearest(packages);

    }

    private Transform FindNearest<T>(T[] targets) where T : Component
    {
        Transform nearest = null;
        float nearestDistance = float.PositiveInfinity;

        foreach(T candidate in targets)
        {
            if(candidate == null || !candidate.gameObject.activeInHierarchy)
            {
                continue;
            }
            Vector3 offset = candidate.transform.position - player.position;
            offset.y = 0f;
            float distance = offset.sqrMagnitude;
            if(distance < nearestDistance)
            {
                nearest = candidate.transform;
                nearestDistance = distance;
            }
        }
        return nearest;
    }
    private Transform UnactiveCheckpoint(CheckPoint[] checkpoints)
    {
        Transform nearest = null;
        float nearestDistance = float.PositiveInfinity;

        foreach(CheckPoint checkpoint in checkpoints)
        {
            if(checkpoint == null || checkpoint.HasBeenActivated || !checkpoint.gameObject.activeInHierarchy)
            {
                continue;
            }
            Vector3 offset = checkpoint.transform.position - player.position;
            offset.y = 0f;
            float distance = offset.sqrMagnitude;
            if(distance < nearestDistance)
            {
                nearest = checkpoint.transform;
                nearestDistance = distance;
            }
        }
        return nearest;
    }
}
