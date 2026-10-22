#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class MapGridSaveEditor
{
    [MenuItem("Tools/Map Maker/Save Map as Prefab")]
    public static void SaveRuntimeMap()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning(
                "Enter Play Mode and build your map first."
            );
            return;
        }

        MapGrid grid = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<MapGrid>()
            : null;

        if (grid == null)
        {
            Debug.LogWarning(
                "Select the GameObject containing MapGrid."
            );
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Map Prefab",
            "SavedMap",
            "prefab",
            "Choose where to save your map prefab."
        );

        if (string.IsNullOrEmpty(path))
            return;

        GameObject root = new GameObject("SavedMap");

        int savedCount = 0;

        try
        {
            foreach (GridCell cell in grid.cells)
            {
                if (cell == null || cell.currentObject == null)
                    continue;

                GameObject runtimeObject = cell.currentObject;

                MapTile tile =
                    grid.GetMapTileFromObject(runtimeObject);

                if (tile == null || tile.prefab == null)
                {
                    Debug.LogWarning(
                        $"Could not find source prefab for " +
                        $"{runtimeObject.name}. Skipping tile."
                    );
                    continue;
                }

                GameObject instance =
                    PrefabUtility.InstantiatePrefab(
                        tile.prefab
                    ) as GameObject;

                if (instance == null)
                    continue;

                instance.transform.SetParent(
                    root.transform,
                    false
                );

                instance.transform.SetPositionAndRotation(
                    runtimeObject.transform.position,
                    runtimeObject.transform.rotation
                );

                instance.transform.localScale =
                    runtimeObject.transform.lossyScale;

                instance.name = tile.prefab.name;

                savedCount++;
            }

            if (savedCount == 0)
            {
                Debug.LogWarning("No tiles were saved.");
                return;
            }

            GameObject savedPrefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    path,
                    out bool success
                );

            if (!success || savedPrefab == null)
            {
                Debug.LogError("Failed to save the map prefab.");
                return;
            }

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "Map Saved",
                $"Saved {savedCount} tiles to:\n{path}",
                "OK"
            );

            Selection.activeObject = savedPrefab;
        }
        finally
        {
            // Remove the temporary root from the running scene.
            if (root != null)
                Object.DestroyImmediate(root);
        }
    }
}
#endif