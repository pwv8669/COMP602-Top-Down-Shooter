using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public enum CellState
    {
        Empty,
        Hallway,
        Intersection
    }
    public int gridWidth = 20;
    public int gridHeight = 20;
    public int numberOfWalkers = 1;
    public int walkerLifetime = 50;
    public int location = 1;
    private List<HallwayWalker> walkers;

    public MapGenerator mapGenerator;

    public CellState[,] grid; // 2D array for the grid

    [ContextMenu("Generate New Map")]
    private void GenerateNewMap()
    {
        InitializeGrid();
        GenerateHallways();
    }

    [ContextMenu("Generate Hallways")]
    void GenerateHallways()
    {
        switch (location)
        {
            case 2:
                // Three random rows and columns
                int[] randomRows = new int[3];
                randomRows[0] = 10;
                randomRows[1] = Random.Range(3, gridWidth - 3);
                randomRows[2] = Random.Range(3, gridWidth - 3);

                int[] randomColumns = new int[3];
                randomColumns[0] = Random.Range(3, gridHeight - 3);
                randomColumns[1] = Random.Range(3, gridHeight - 3);
                randomColumns[2] = Random.Range(3, gridHeight - 3);

                // Fill entire rows
                for (int i = 0; i < randomRows.Length; i++)
                {
                    int y = Mathf.Clamp(randomRows[i], 0, gridHeight - 1);
                    for (int x = Random.Range(0, 2); x < Random.Range(gridWidth - 4, gridWidth - 2); x++)
                    {
                        var state = grid[x, y];
                        if (state == CellState.Empty)
                            grid[x, y] = CellState.Hallway;
                        else if (state == CellState.Hallway)
                            grid[x, y] = CellState.Intersection;
                    }
                }

                // Fill entire columns
                for (int i = 0; i < randomColumns.Length; i++)
                {
                    int x = Mathf.Clamp(randomColumns[i], 0, gridWidth - 1);
                    for (int y = Random.Range(0, 2); y < Random.Range(gridHeight - 4, gridHeight - 2); y++)
                    {
                        var state = grid[x, y];
                        if (state == CellState.Empty)
                            grid[x, y] = CellState.Hallway;
                        else if (state == CellState.Hallway)
                            grid[x, y] = CellState.Intersection;
                    }
                }
                break;

            case 1:
            default:
                walkers = new List<HallwayWalker>();

                // Create and initialize walkers
                for (int i = 0; i < numberOfWalkers; i++)
                {
                    // Start each walker at the center of the grid
                    Vector2Int startPosition = new Vector2Int(gridWidth / 2, gridHeight / 2);

                    HallwayWalker newWalker = new HallwayWalker(startPosition, this);
                    walkers.Add(newWalker);

                    // Mark starting cell
                    grid[startPosition.x, startPosition.y] = CellState.Hallway;
                }

                // Run the simulation
                for (int i = 0; i < walkerLifetime; i++)
                {
                    foreach (HallwayWalker walker in walkers)
                    {
                        Vector2Int newPos = walker.Move();

                        // Update the grid based on walker's new position
                        // Check if cell is already a hallway
                        if (grid[newPos.x, newPos.y] == CellState.Hallway)
                        {
                            grid[newPos.x, newPos.y] = CellState.Intersection;
                        }
                        else
                        {
                            grid[newPos.x, newPos.y] = CellState.Hallway;
                        }
                    }
                }
                break;
        }

        mapGenerator.GenerateMap();
    }

    [ContextMenu("Initialize Grid")]
    void InitializeGrid()
    {
        grid = new CellState[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = CellState.Empty;
            }
        }
    }

    void OnDrawGizmos()
    {
        // Ensure the grid has been initialized
        if (grid == null)
        {
            return;
        }

        // Loop through every cell in the grid
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Set the color based on the cell state
                switch (grid[x, y])
                {
                    case CellState.Empty:
                        Gizmos.color = Color.gray;
                        break;
                    case CellState.Hallway:
                        Gizmos.color = Color.white;
                        break;
                    case CellState.Intersection:
                        Gizmos.color = Color.cyan;
                        break;
                }

                // Calculate the position for the gizmo
                Vector3 pos = new Vector3(x, 0, y);

                // Draw a small cube at the cell's position
                Gizmos.DrawCube(pos, Vector3.one * 0.5f);
            }
        }
    }

    public bool IsInBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridWidth && position.y >= 0 && position.y < gridHeight;
    }
}