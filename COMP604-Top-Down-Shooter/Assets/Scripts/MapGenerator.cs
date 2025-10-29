using UnityEngine;
using Unity.AI.Navigation;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public GridManager gridManager;

    // Prefabs for the new map generation logic
    public GameObject W1floorPrefab1;
    public GameObject W1floorPrefab2;
    public GameObject W1connectingPrefab1;
    public GameObject W1connectingPrefab2;
    public GameObject W1wallPrefab1;
    public GameObject W1wallPrefab2;
    public GameObject W1wallPrefab3;
    public GameObject W2floorPrefab1;
    public GameObject W2floorPrefab2;
    public GameObject W2wallPrefab1;
    public GameObject W2wallPrefab2;
    public GameObject W3floorPrefab1;
    public GameObject W3wallPrefab1;
    public GameObject W3wallPrefab2;
    public GameObject W3wallPrefab3;
    public GameObject minimapPrefab;
    public GameObject enemySpawnerPrefab;
    public GameObject safezonePrefab;

    // Enemy navmesh to rebake when map regenerates.
    public NavMeshSurface navMeshSurface;

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

        // Rebuild the navmesh for the new layout
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
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
        float tileSize = 10f;
        bool northEmpty = (y + 1 >= gridManager.gridHeight || gridManager.grid[x, y + 1] == GridManager.CellState.Empty);
        bool eastEmpty = (x + 1 >= gridManager.gridWidth || gridManager.grid[x + 1, y] == GridManager.CellState.Empty);
        bool southEmpty = (y - 1 < 0 || gridManager.grid[x, y - 1] == GridManager.CellState.Empty);
        bool westEmpty = (x - 1 < 0 || gridManager.grid[x - 1, y] == GridManager.CellState.Empty);
        bool northEastEmpty = (y + 1 >= gridManager.gridHeight || x + 1 >= gridManager.gridWidth || gridManager.grid[x + 1, y + 1] == GridManager.CellState.Empty);
        bool northWestEmpty = (y + 1 >= gridManager.gridHeight || x - 1 < 0 || gridManager.grid[x - 1, y + 1] == GridManager.CellState.Empty);
        bool southEastEmpty = (y - 1 < 0 || x + 1 >= gridManager.gridWidth || gridManager.grid[x + 1, y - 1] == GridManager.CellState.Empty);
        bool southWestEmpty = (y - 1 < 0 || x - 1 < 0 || gridManager.grid[x - 1, y - 1] == GridManager.CellState.Empty);
        bool spawnerTile = false;
        bool safezoneTile = false;
        switch (gridManager.location)
        {
            case 1:
                Vector3 tilePosition = new Vector3(x * tileSize, 0, y * tileSize);

                // Place enemy spawn where needed.
                if (gridManager.grid[x, y] == GridManager.CellState.EnemySpawnPoint)
                {
                    if (enemySpawnerPrefab != null)
                    {
                        Instantiate(enemySpawnerPrefab, tilePosition, Quaternion.identity, this.transform);
                    }
                    spawnerTile = true;
                }

                // Place safezone where needed.
                if (gridManager.grid[x, y] == GridManager.CellState.Safezone)
                {
                    if (safezonePrefab != null)
                    {
                        Instantiate(safezonePrefab, tilePosition, Quaternion.identity, this.transform);
                    }
                    safezoneTile = true;
                }

                // Hallway
                if (gridManager.grid[x, y] == GridManager.CellState.Hallway)
                {
                    // Place the floor prefab at the center of the tile
                    if (W1floorPrefab1 != null && !spawnerTile && !safezoneTile)
                    {
                        Instantiate(W1floorPrefab1, tilePosition, Quaternion.identity, this.transform);
                    }
                    // Place the minimap prefab slightly below the tile
                    if (minimapPrefab != null)
                    {
                        Vector3 minimapPosition = tilePosition + new Vector3(0, -10, 0);
                        Instantiate(minimapPrefab, minimapPosition, Quaternion.identity, this.transform);
                    }

                    // Check for empty neighbors to place walls
                    if (W1wallPrefab1 == null) return;

                    // North
                    if (northEmpty)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(-0.25f, 2, 3);
                        Instantiate(W1wallPrefab1, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                    }
                    else
                    {
                        Vector3 connectPos = tilePosition + new Vector3(0, 0, 4);
                        Instantiate(W1connectingPrefab2, connectPos, Quaternion.Euler(0, 0, 0), this.transform);
                        Vector3 wallPos = tilePosition + new Vector3(3, 2, 4.25f);
                        Instantiate(W1wallPrefab2, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                        Vector3 wall2Pos = tilePosition + new Vector3(-3, 2, 3.75f);
                        Instantiate(W1wallPrefab2, wall2Pos, Quaternion.Euler(0, 0, 0), this.transform);
                    }

                    // East
                    if (eastEmpty)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(3, 2, 0.25f);
                        Instantiate(W1wallPrefab1, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                    }
                    else
                    {
                        Vector3 connectPos = tilePosition + new Vector3(4, 0, 0);
                        Instantiate(W1connectingPrefab1, connectPos, Quaternion.Euler(0, 0, 0), this.transform);
                        Vector3 wallPos = tilePosition + new Vector3(3.75f, 2, 3);
                        Instantiate(W1wallPrefab2, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                        Vector3 wall2Pos = tilePosition + new Vector3(4.25f, 2, -3);
                        Instantiate(W1wallPrefab2, wall2Pos, Quaternion.Euler(0, 90, 0), this.transform);
                    }

                    // South
                    if (southEmpty)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(0.25f, 2, -3);
                        Instantiate(W1wallPrefab1, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                    }
                    else
                    {
                        Vector3 connectPos = tilePosition + new Vector3(0, 0, -4);
                        Instantiate(W1connectingPrefab2, connectPos, Quaternion.Euler(0, 0, 0), this.transform);
                        Vector3 wallPos = tilePosition + new Vector3(3, 2, -3.75f);
                        Instantiate(W1wallPrefab2, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                        Vector3 wall2Pos = tilePosition + new Vector3(-3, 2, -4.25f);
                        Instantiate(W1wallPrefab2, wall2Pos, Quaternion.Euler(0, 0, 0), this.transform);
                    }

                    // West
                    if (westEmpty)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(-3, 2, -0.25f);
                        Instantiate(W1wallPrefab1, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                    }
                    else
                    {
                        Vector3 connectPos = tilePosition + new Vector3(-4, 0, 0);
                        Instantiate(W1connectingPrefab1, connectPos, Quaternion.Euler(0, 0, 0), this.transform);
                        Vector3 wallPos = tilePosition + new Vector3(-4.25f, 2, 3);
                        Instantiate(W1wallPrefab2, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                        Vector3 wall2Pos = tilePosition + new Vector3(-3.75f, 2, -3);
                        Instantiate(W1wallPrefab2, wall2Pos, Quaternion.Euler(0, 90, 0), this.transform);
                    }
                }
                // Room
                else if (gridManager.grid[x, y] == GridManager.CellState.Room || gridManager.grid[x, y] == GridManager.CellState.EnemySpawnPoint || gridManager.grid[x, y] == GridManager.CellState.Safezone)
                {
                    // Place the floor prefab at the center of the tile
                    if (W1floorPrefab2 != null && !spawnerTile && !safezoneTile)
                    {
                        Instantiate(W1floorPrefab2, tilePosition, Quaternion.identity, this.transform);
                    }
                    // Place the minimap prefab slightly below the tile
                    if (minimapPrefab != null)
                    {
                        Vector3 minimapPosition = tilePosition + new Vector3(0, -10, 0);
                        Instantiate(minimapPrefab, minimapPosition, Quaternion.identity, this.transform);
                    }

                    // North
                    if (northEmpty)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(-0.25f, 2, 5);
                        Instantiate(W1wallPrefab3, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                    }
                    else if (gridManager.grid[x, y + 1] == GridManager.CellState.Hallway)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(3.75f, 2, 5f);
                        Instantiate(W1wallPrefab2, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                        Vector3 wall2Pos = tilePosition + new Vector3(-4.25f, 2, 5f);
                        Instantiate(W1wallPrefab2, wall2Pos, Quaternion.Euler(0, 90, 0), this.transform);
                    }

                    // East
                    if (eastEmpty)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(5, 2, 0.25f);
                        Instantiate(W1wallPrefab3, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                    }
                    else if (gridManager.grid[x + 1, y] == GridManager.CellState.Hallway)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(5, 2, 4.25f);
                        Instantiate(W1wallPrefab2, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                        Vector3 wall2Pos = tilePosition + new Vector3(5, 2, -3.75f);
                        Instantiate(W1wallPrefab2, wall2Pos, Quaternion.Euler(0, 0, 0), this.transform);
                    }

                    // South
                    if (southEmpty)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(0.25f, 2, -5);
                        Instantiate(W1wallPrefab3, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                    }
                    else if (gridManager.grid[x, y - 1] == GridManager.CellState.Hallway)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(4.25f, 2, -5f);
                        Instantiate(W1wallPrefab2, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                        Vector3 wall2Pos = tilePosition + new Vector3(-3.75f, 2, -5f);
                        Instantiate(W1wallPrefab2, wall2Pos, Quaternion.Euler(0, 90, 0), this.transform);
                    }

                    // West
                    if (westEmpty)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(-5, 2, -0.25f);
                        Instantiate(W1wallPrefab3, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                    }
                    else if (gridManager.grid[x - 1, y] == GridManager.CellState.Hallway)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(-5, 2, -4.25f);
                        Instantiate(W1wallPrefab2, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                        Vector3 wall2Pos = tilePosition + new Vector3(-5, 2, 3.75f);
                        Instantiate(W1wallPrefab2, wall2Pos, Quaternion.Euler(0, 0, 0), this.transform);
                    }
                }
                break;
            case 2:
                tilePosition = new Vector3(x * tileSize, 0, y * tileSize);

                // Place enemy spawn where needed.
                if (gridManager.grid[x, y] == GridManager.CellState.EnemySpawnPoint)
                {
                    if (enemySpawnerPrefab != null)
                    {
                        Instantiate(enemySpawnerPrefab, tilePosition, Quaternion.identity, this.transform);
                    }
                    spawnerTile = true;
                }

                // Place safezone where needed.
                if (gridManager.grid[x, y] == GridManager.CellState.Safezone)
                {
                    if (safezonePrefab != null)
                    {
                        Instantiate(safezonePrefab, tilePosition, Quaternion.identity, this.transform);
                    }
                    safezoneTile = true;
                }

                // Road
                if (gridManager.grid[x, y] == GridManager.CellState.Room || gridManager.grid[x, y] == GridManager.CellState.Room2 || gridManager.grid[x, y] == GridManager.CellState.EnemySpawnPoint || gridManager.grid[x, y] == GridManager.CellState.Safezone)
                {
                    // Place the floor prefab at the center of the tile
                    if (gridManager.grid[x, y] == GridManager.CellState.Room2 && W2floorPrefab2 != null && !spawnerTile && !safezoneTile)
                    {
                        Instantiate(W2floorPrefab2, tilePosition, Quaternion.identity, this.transform);
                    }
                    else if (W2floorPrefab1 != null && !spawnerTile && !safezoneTile)
                    {
                        Instantiate(W2floorPrefab1, tilePosition, Quaternion.identity, this.transform);
                    }
                    // Place the minimap prefab slightly below the tile
                    if (minimapPrefab != null)
                    {
                        Vector3 minimapPosition = tilePosition + new Vector3(0, -10, 0);
                        Instantiate(minimapPrefab, minimapPosition, Quaternion.identity, this.transform);
                    }

                    if (gridManager.grid[x, y] == GridManager.CellState.Room2)
                    {
                        // North
                        if (northEmpty)
                        {
                            Vector3 wallPos = tilePosition + new Vector3(-0.25f, 4, 5);
                            Instantiate(W2wallPrefab1, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                        }

                        // East
                        if (true)
                        {
                            Vector3 wallPos = tilePosition + new Vector3(2, 4, 0.25f);
                            Instantiate(W2wallPrefab1, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                        }

                        // South
                        if (southEmpty)
                        {
                            Vector3 wallPos = tilePosition + new Vector3(0.25f, 4, -5);
                            Instantiate(W2wallPrefab1, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                        }

                        // West
                        if (true)
                        {
                            Vector3 wallPos = tilePosition + new Vector3(-2, 4, -0.25f);
                            Instantiate(W2wallPrefab1, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                        }
                    }
                    else
                    {
                        // North
                        if (northEmpty)
                        {
                            Vector3 wallPos = tilePosition + new Vector3(-0.25f, 4, 5);
                            Instantiate(W2wallPrefab1, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                        }
                        else if (gridManager.grid[x, y + 1] == GridManager.CellState.Room2)
                        {
                            Vector3 wallPos = tilePosition + new Vector3(-3.75f, 4, 5);
                            Instantiate(W2wallPrefab2, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                            Vector3 wallPos2 = tilePosition + new Vector3(3.25f, 4, 5);
                            Instantiate(W2wallPrefab2, wallPos2, Quaternion.Euler(0, 90, 0), this.transform);
                        }

                        // East
                        if (eastEmpty)
                        {
                            Vector3 wallPos = tilePosition + new Vector3(5, 4, 0.25f);
                            Instantiate(W2wallPrefab1, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                        }

                        // South
                        if (southEmpty)
                        {
                            Vector3 wallPos = tilePosition + new Vector3(0.25f, 4, -5);
                            Instantiate(W2wallPrefab1, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                        }
                        else if (gridManager.grid[x, y - 1] == GridManager.CellState.Room2)
                        {
                            Vector3 wallPos = tilePosition + new Vector3(-3.25f, 4, -5);
                            Instantiate(W2wallPrefab2, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                            Vector3 wallPos2 = tilePosition + new Vector3(3.75f, 4, -5);
                            Instantiate(W2wallPrefab2, wallPos2, Quaternion.Euler(0, 90, 0), this.transform);
                        }

                        // West
                        if (westEmpty)
                        {
                            Vector3 wallPos = tilePosition + new Vector3(-5, 4, -0.25f);
                            Instantiate(W2wallPrefab1, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                        }
                    }
                }
                break;
            case 3:
                tilePosition = new Vector3(x * tileSize, 0, y * tileSize);

                // Place enemy spawn where needed.
                if (gridManager.grid[x, y] == GridManager.CellState.EnemySpawnPoint)
                {
                    if (enemySpawnerPrefab != null)
                    {
                        Instantiate(enemySpawnerPrefab, tilePosition, Quaternion.identity, this.transform);
                    }
                    spawnerTile = true;
                }

                // Place safezone where needed.
                if (gridManager.grid[x, y] == GridManager.CellState.Safezone)
                {
                    if (safezonePrefab != null)
                    {
                        Instantiate(safezonePrefab, tilePosition, Quaternion.identity, this.transform);
                    }
                    safezoneTile = true;
                }

                if (gridManager.grid[x, y] == GridManager.CellState.Room || gridManager.grid[x, y] == GridManager.CellState.Room2 || gridManager.grid[x, y] == GridManager.CellState.EnemySpawnPoint || gridManager.grid[x, y] == GridManager.CellState.Safezone)
                {
                    // Place the floor prefab at the center of the tile
                    Instantiate(W3floorPrefab1, tilePosition, Quaternion.identity, this.transform);

                    // North
                    if (northEmpty)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(-0.25f, 2, 5);
                        Instantiate(W3wallPrefab3, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                    }

                    // East
                    if (eastEmpty)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(5, 2, 0.25f);
                        Instantiate(W3wallPrefab3, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                    }

                    // South
                    if (southEmpty)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(0.25f, 2, -5);
                        Instantiate(W3wallPrefab3, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                    }

                    // West
                    if (westEmpty)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(-5, 2, -0.25f);
                        Instantiate(W3wallPrefab3, wallPos, Quaternion.Euler(0, 90, 0), this.transform);
                    }

                    // Inner wall 1
                    if (y == gridManager.W3roomSizes[0] + 5)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(0.25f, 2, -5);
                        Instantiate(W3wallPrefab2, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                    }

                    // Inner wall 2
                    else if (y == gridManager.W3roomSizes[0] + gridManager.W3roomSizes[1] + 5)
                    {
                        Vector3 wallPos = tilePosition + new Vector3(0.25f, 2, -5);
                        Instantiate(W3wallPrefab2, wallPos, Quaternion.Euler(0, 0, 0), this.transform);
                    }
                }
                break;
        }
    }
}