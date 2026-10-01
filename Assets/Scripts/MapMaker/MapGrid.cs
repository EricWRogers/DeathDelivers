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

    public List<GridCell> cells = new();

    Camera mapCamera;

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

        if (cell == null)
            return;

        SelectCell(cell);
    }

    public void SelectCell(GridCell cell)
    {
        selectedCell = cell;
        currentObject = cell.currentObject;

        selectionObject.SetActive(true);

        selectionObject.transform.position =
            cell.transform.position +
            Vector3.up * 0.03f;

        Debug.Log(
            "Selected Cell: " +
            cell.gridPosition
        );

        Debug.Log(
            "Current Object: " +
            currentObject
        );
    }
    void CreateTileDropdown()
    {
        if (tileDropdown == null)
            return;

        tileDropdown.ClearOptions();

        List<string> options = new();

        foreach (MapTile tile in tiles)
        {
            options.Add(tile.tileName);
        }

        tileDropdown.AddOptions(options);

        tileDropdown.onValueChanged.AddListener(
            SelectTile
        );
    }

    void SelectTile(int index)
    {
        if (selectedCell == null)
            return;
    
        if (index < 0 || index >= tiles.Count)
            return;
    
        MapTile tile = tiles[index];
    
        if (tile == null || tile.prefab == null)
            return;
    
        // Remove old object
        if (selectedCell.currentObject != null)
        {
            Destroy(selectedCell.currentObject);
        }
    
        // Create new object
        GameObject newObject = Instantiate(
            tile.prefab,
            GridToWorld(selectedCell.gridPosition),
            Quaternion.identity
        );
    
        // Store the object in the cell
        selectedCell.currentObject = newObject;
    
        // Update currently selected object
        currentObject = newObject;
    
        Debug.Log(
            "Placed " +
            tile.tileName +
            " on " +
            selectedCell.gridPosition
        );
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