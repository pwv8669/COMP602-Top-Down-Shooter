using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public GridManager gridManager;

    // Prefabs for the new map generation logic
    public GameObject floorPrefab;
    public GameObject grassPrefab;
    public GameObject wallPrefab;
    public GameObject minimapPrefab;

    public void GenerateMap()
    {
        ClearMap();

        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                // Check if the cell is part of the dungeon
                if (gridManager.grid[x, y] != GridManager.CellState.Empty)
                {
                    PlaceTile(x, y);
                }
            }
        }
    }

    private void ClearMap()
    {
        // Find all child objects of the MapGenerator transform and destroy them
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            // Use DestroyImmediate in editor scripts if needed, Destroy is fine for runtime
            if (Application.isPlaying)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }

    private void PlaceTile(int x, int y)
    {
        // The "10" is assumed to be the size of the tiles
        float tileSize = 10f;
        Vector3 tilePosition = new Vector3(x * tileSize, 0, y * tileSize);

        bool northEmpty = (y + 1 >= gridManager.gridHeight || gridManager.grid[x, y + 1] == GridManager.CellState.Empty);
        bool eastEmpty = (x + 1 >= gridManager.gridWidth || gridManager.grid[x + 1, y] == GridManager.CellState.Empty);
        bool southEmpty = (y - 1 < 0 || gridManager.grid[x, y - 1] == GridManager.CellState.Empty);
        bool westEmpty = (x - 1 < 0 || gridManager.grid[x - 1, y] == GridManager.CellState.Empty);
        bool northEastEmpty = (y + 1 >= gridManager.gridHeight || x + 1 >= gridManager.gridWidth || gridManager.grid[x + 1, y + 1] == GridManager.CellState.Empty);
        bool northWestEmpty = (y + 1 >= gridManager.gridHeight || x - 1 < 0 || gridManager.grid[x - 1, y + 1] == GridManager.CellState.Empty);
        bool southEastEmpty = (y - 1 < 0 || x + 1 >= gridManager.gridWidth || gridManager.grid[x + 1, y - 1] == GridManager.CellState.Empty);
        bool southWestEmpty = (y - 1 < 0 || x - 1 < 0 || gridManager.grid[x - 1, y - 1] == GridManager.CellState.Empty);

        // Place the floor prefab at the center of the tile
        if (floorPrefab != null)
        {
            if (!northEmpty && !eastEmpty && !southEmpty && !westEmpty && !northEastEmpty && !northWestEmpty && !southEastEmpty && !southWestEmpty)
                Instantiate(grassPrefab, tilePosition, Quaternion.identity, this.transform);
            else
                Instantiate(floorPrefab, tilePosition, Quaternion.identity, this.transform);
        }
        // Place the minimap prefab slightly below the tile
        if (minimapPrefab != null)
        {
            Vector3 minimapPosition = tilePosition + new Vector3(0, -10, 0);
            Instantiate(minimapPrefab, minimapPosition, Quaternion.identity, this.transform);
        }

        // Check for empty neighbors to place walls
        if (wallPrefab == null) return;

        // North
        if (northEmpty)
        {
            Vector3 wallPos = tilePosition + new Vector3(-0.5f, 2, tileSize / 2);
            Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
        }

        // East
        if (eastEmpty)
        {
            Vector3 wallPos = tilePosition + new Vector3(tileSize / 2, 2, 0.5f);
            Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
        }

        // South
        if (southEmpty)
        {
            Vector3 wallPos = tilePosition + new Vector3(0.5f, 2, -tileSize / 2);
            Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 180, 0), this.transform);
        }

        // West
        if (westEmpty)
        {
            Vector3 wallPos = tilePosition + new Vector3(-tileSize / 2, 2, -0.5f);
            Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 270, 0), this.transform);
        }
    }
}