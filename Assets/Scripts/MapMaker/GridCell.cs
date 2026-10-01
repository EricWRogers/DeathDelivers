using UnityEngine;

public class GridCell : MonoBehaviour
{
    public Vector3Int gridPosition;
    public GameObject currentObject;

    public void Select()
    {
        MapGrid map = FindFirstObjectByType<MapGrid>();

        if (map != null)
            map.SelectCell(this);
    }
}
