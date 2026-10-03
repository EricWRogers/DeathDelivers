using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class MapGrid : MonoBehaviour
{
    [Header("Grid")]
    public int width = 20;
    public int depth = 20;
    public float cellSize = 5f;

    [Header("Tiles")]
    public List<MapTile> tiles = new();

    [Header("Tile Dropdown")]
    public TMP_Dropdown tileDropdown;

    [Header("Selection")]
    public Color selectedColor = Color.yellow;

    public GridCell selectedCell;
    public GameObject currentObject;
    public GameObject selectionObject;

    public GameObject rotateButton;

    [Header("Placed Tiles Parent")]
    public GameObject placedTilesParent;

    public List<GridCell> cells = new();

    Camera mapCamera;

    Dictionary<GameObject, GridCell> placedTiles = new();

    void Awake()
    {
        mapCamera = Camera.main;

        CreateGrid();
        CreateSelection();
        CreateTileDropdown();
    }

    void Update()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            ClickGrid();
        }

        if (Mouse.current != null &&
            Mouse.current.rightButton.wasPressedThisFrame)
        {
            RightClickGrid();
        }
    }

    void CreateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                CreateCell(x, z);
            }
        }
    }

    void CreateCell(int x, int z)
    {
        GameObject cellObject =
            GameObject.CreatePrimitive(
                PrimitiveType.Quad
            );

        cellObject.name =
            $"Cell {x}, {z}";

        cellObject.transform.SetParent(transform);

        cellObject.transform.position =
            GridToWorld(
                new Vector3Int(x, 0, z)
            );

        cellObject.transform.rotation =
            Quaternion.Euler(90, 0, 0);

        cellObject.transform.localScale =
            new Vector3(
                cellSize,
                cellSize,
                1
            );

        GridCell cell =
            cellObject.AddComponent<GridCell>();

        cell.gridPosition =
            new Vector3Int(x, 0, z);

        cells.Add(cell);
    }

    void ClickGrid()
    {
        Ray ray =
            mapCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit
        ))
        {
            return;
        }

        GridCell cell =
            hit.collider.GetComponent<GridCell>();

        if (cell != null)
        {
            SelectCell(cell);
            return;
        }

        GridCell placedTileCell =
            GetCellFromPlacedTile(hit.transform);

        if (placedTileCell != null)
        {
            SelectCell(placedTileCell);
        }
    }

    void RightClickGrid()
    {
        Ray ray =
            mapCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit
        ))
        {
            return;
        }

        GridCell cell =
            hit.collider.GetComponent<GridCell>();

        if (cell == null)
        {
            cell = GetCellFromPlacedTile(hit.transform);
        }

        if (cell == null)
            return;

        ResetTile(cell);
    }

    GridCell GetCellFromPlacedTile(
        Transform hitTransform
    )
    {
        Transform current =
            hitTransform;

        while (current != null)
        {
            GameObject objectHit =
                current.gameObject;

            if (placedTiles.TryGetValue(
                objectHit,
                out GridCell cell
            ))
            {
                return cell;
            }

            current =
                current.parent;
        }

        return null;
    }

    public void SelectCell(GridCell cell)
    {
        selectedCell = cell;
        currentObject = cell.currentObject;

        selectionObject.SetActive(true);

        selectionObject.transform.position =
            cell.transform.position +
            Vector3.up * 0.03f;

        if (currentObject != null)
        {
            MapTile tile =
                GetMapTileFromObject(currentObject);

            if (tile != null)
            {
                int tileIndex =
                    tiles.IndexOf(tile);

                if (tileIndex >= 0)
                {
                    tileDropdown.SetValueWithoutNotify(
                        tileIndex + 1
                    );
                }

                rotateButton.SetActive(
                    tile.canRotate
                );
            }
            else
            {
                rotateButton.SetActive(false);
            }
        }
        else
        {
            tileDropdown.SetValueWithoutNotify(0);

            rotateButton.SetActive(false);
        }

        Debug.Log(
            "Selected Cell: " +
            cell.gridPosition
        );

        Debug.Log(
            "Current Object: " +
            currentObject
        );
    }

    public void RotateTile()
    {
        if (currentObject == null)
            return;

        currentObject.transform.Rotate(
            0,
            90,
            0
        );
    }

    void CreateTileDropdown()
    {
        if (tileDropdown == null)
            return;

        tileDropdown.ClearOptions();

        List<string> options = new();

        options.Add("None");

        foreach (MapTile tile in tiles)
        {
            options.Add(tile.tileName);
        }

        tileDropdown.AddOptions(options);

        tileDropdown.SetValueWithoutNotify(0);

        tileDropdown.onValueChanged.AddListener(
            SelectTile
        );
    }

    void SelectTile(int index)
    {
        if (selectedCell == null)
            return;

        // None
        if (index == 0)
        {
            ResetTile(selectedCell);
            return;
        }

        int tileIndex =
            index - 1;

        if (tileIndex < 0 ||
            tileIndex >= tiles.Count)
            return;

        MapTile tile =
            tiles[tileIndex];

        if (tile == null ||
            tile.prefab == null)
            return;

        // Remove old object
        if (selectedCell.currentObject != null)
        {
            GameObject oldObject =
                selectedCell.currentObject;

            placedTiles.Remove(oldObject);

            Destroy(oldObject);
        }

        // Create new prefab instance
        GameObject newObject =
            Instantiate(
                tile.prefab,
                GridToWorld(
                    selectedCell.gridPosition
                ),
                Quaternion.identity
            );

        // Parent the prefab instance
        if (placedTilesParent != null)
        {
            newObject.transform.SetParent(
                placedTilesParent.transform,
                true
            );
        }

        // Store the object in the cell
        selectedCell.currentObject =
            newObject;

        // Store the object so clicks on the
        // prefab can find its GridCell
        placedTiles[newObject] =
            selectedCell;

        // Update currently selected object
        currentObject =
            newObject;

        rotateButton.SetActive(
            tile.canRotate
        );

        Debug.Log(
            "Placed " +
            tile.tileName +
            " on " +
            selectedCell.gridPosition
        );
    }

    void ResetTile(GridCell cell)
    {
        if (cell == null)
            return;

        if (cell.currentObject != null)
        {
            GameObject oldObject =
                cell.currentObject;

            placedTiles.Remove(oldObject);

            Destroy(oldObject);
        }

        cell.currentObject = null;

        if (selectedCell == cell)
        {
            currentObject = null;

            tileDropdown.SetValueWithoutNotify(0);

            rotateButton.SetActive(false);
        }
    }

    MapTile GetMapTileFromObject(
        GameObject objectToFind
    )
    {
        foreach (MapTile tile in tiles)
        {
            if (tile == null ||
                tile.prefab == null)
                continue;

            string prefabName =
                tile.prefab.name;

            string objectName =
                objectToFind.name;

            if (objectName == prefabName ||
                objectName.StartsWith(
                    prefabName + "(Clone)"
                ))
            {
                return tile;
            }
        }

        return null;
    }

    void CreateSelection()
    {
        selectionObject =
            GameObject.CreatePrimitive(
                PrimitiveType.Quad
            );

        selectionObject.name =
            "Grid Selection";

        selectionObject.transform.SetParent(
            transform
        );

        selectionObject.transform.rotation =
            Quaternion.Euler(90, 0, 0);

        selectionObject.transform.localScale =
            new Vector3(
                cellSize,
                cellSize,
                1
            );

        Destroy(
            selectionObject.GetComponent<Collider>()
        );

        Material material =
            new Material(
                Shader.Find(
                    "Universal Render Pipeline/Unlit"
                )
            );

        material.color =
            new Color(
                selectedColor.r,
                selectedColor.g,
                selectedColor.b,
                0.35f
            );

        selectionObject
            .GetComponent<MeshRenderer>()
            .material = material;

        selectionObject.SetActive(false);
    }

    public Vector3 GridToWorld(
        Vector3Int cell
    )
    {
        return transform.position +
            new Vector3(
                cell.x * cellSize,
                0,
                cell.z * cellSize
            );
    }

    public GridCell GetCell(
        Vector3Int position
    )
    {
        foreach (GridCell cell in cells)
        {
            if (cell.gridPosition == position)
                return cell;
        }

        return null;
    }
}