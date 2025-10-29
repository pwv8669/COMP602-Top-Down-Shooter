using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public enum CellState
    {
        Empty,
        Hallway,
        Intersection,
        EnemySpawnPoint,
        Room,
        Room2,
        Room3,
        Safezone  // New: Blue safe zone in center where enemies cannot enter
    }
    public int gridWidth = 20;
    public int gridHeight = 20;
    public int numberOfWalkers = 1;
    public int walkerLifetime = 50;
    public int location = 1;
    public int[] W3roomSizes = new int[] { 3, 4, 5 };
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
            case 1:
            default:
                // Room-based generation with connecting hallways
                // int roomCount = Random.Range(5, 10);
                int roomCount = 10;
                Vector2Int currentPos = new Vector2Int(gridWidth / 2, gridHeight / 2);
                grid[currentPos.x, currentPos.y] = CellState.Hallway;

                for (int i = 0; i < roomCount; i++)
                {
                    Vector2Int startPos = currentPos;
                    Vector2Int endPos;

                    // Determine direction based on distance from edges
                    float horizontalBias = 0.5f;
                    float verticalBias = 0.5f;
                    
                    // Bias away from left edge
                    if (startPos.x < gridWidth / 4)
                        horizontalBias += 0.3f;
                    // Bias away from right edge
                    else if (startPos.x > gridWidth * 3 / 4)
                        horizontalBias -= 0.3f;
                    
                    // Bias away from bottom edge
                    if (startPos.y < gridHeight / 4)
                        verticalBias += 0.3f;
                    // Bias away from top edge
                    else if (startPos.y > gridHeight * 3 / 4)
                        verticalBias -= 0.3f;
                    
                    bool moveHorizontally = Random.value > 0.5f;
                    
                    if (moveHorizontally)
                    {
                        // Choose direction: positive (right) if bias > 0.5, negative (left) otherwise
                        int direction = Random.value > 0.5f ? -1 : 1;
                        int distance = Random.Range(3, 8);
                        int newX = Mathf.Clamp(startPos.x + (distance * direction), 0, gridWidth - 1);
                        endPos = new Vector2Int(newX, startPos.y);
                    }
                    else
                    {
                        // Choose direction: positive (up) if bias > 0.5, negative (down) otherwise
                        int direction = Random.value > 0.5f ? -1 : 1;
                        int distance = Random.Range(3, 8);
                        int newY = Mathf.Clamp(startPos.y + (distance * direction), 0, gridHeight - 1);
                        endPos = new Vector2Int(startPos.x, newY);
                    }

                    // Draw hallway between start and end
                    if (startPos.x == endPos.x)
                    {
                        // Vertical hallway
                        int minY = Mathf.Min(startPos.y, endPos.y);
                        int maxY = Mathf.Max(startPos.y, endPos.y);
                        for (int y = minY; y <= maxY; y++)
                        {
                            var state = grid[startPos.x, y];
                            if (state == CellState.Empty) grid[startPos.x, y] = CellState.Hallway;
                        }
                    }
                    else
                    {
                        // Horizontal hallway
                        int minX = Mathf.Min(startPos.x, endPos.x);
                        int maxX = Mathf.Max(startPos.x, endPos.x);
                        for (int x = minX; x <= maxX; x++)
                        {
                            var state = grid[x, startPos.y];
                            if (state == CellState.Empty) grid[x, startPos.y] = CellState.Hallway;
                        }
                    }

                    // Generate room at end point
                    int roomWidth = Random.Range(2, 4);
                    int roomHeight = Random.Range(2, 4);

                    // Calculate possible room positions that overlap the end point
                    int minRoomX = endPos.x - roomWidth + 1;
                    int maxRoomX = endPos.x;
                    int minRoomY = endPos.y - roomHeight + 1;
                    int maxRoomY = endPos.y;

                    int roomX = Random.Range(minRoomX, maxRoomX + 1);
                    int roomY = Random.Range(minRoomY, maxRoomY + 1);

                    // Clamp room bounds to grid
                    int clampedStartX = Mathf.Max(0, roomX);
                    int clampedEndX = Mathf.Min(gridWidth - 1, roomX + roomWidth - 1);
                    int clampedStartY = Mathf.Max(0, roomY);
                    int clampedEndY = Mathf.Min(gridHeight - 1, roomY + roomHeight - 1);

                    // Fill room
                    for (int x = clampedStartX; x <= clampedEndX; x++)
                    {
                        for (int y = clampedStartY; y <= clampedEndY; y++)
                        {
                            var state = grid[x, y];
                            grid[x, y] = CellState.Room;
                        }
                    }

                    // Update current position to a random point in the generated room
                    currentPos = new Vector2Int(
                        Random.Range(clampedStartX, clampedEndX + 1),
                        Random.Range(clampedStartY, clampedEndY + 1)
                    );
                }
                break;
            case 2:
                // Generate vertical hallways (columns)
                int xtemp = Random.Range(0, 5);
                while (xtemp < gridWidth)
                {
                    if (Random.value < 0.3f)
                    {
                        for (int y = 0; y < gridHeight; y++)
                        {
                            var state = grid[xtemp, y];
                            grid[xtemp, y] = CellState.Room2;
                        }
                    }
                    else
                    {
                        for (int y = 0; y < gridHeight; y++)
                        {
                            var state = grid[xtemp, y];
                            grid[xtemp, y] = CellState.Room;
                        }
                    }
                    
                    xtemp += Random.Range(2, 5);
                }

                // Generate horizontal hallways (rows)
                int ytemp = Random.Range(0, 5);
                while (ytemp < gridHeight)
                {
                    for (int x_coord = 0; x_coord < gridWidth; x_coord++)
                    {
                        var state = grid[x_coord, ytemp];
                        grid[x_coord, ytemp] = CellState.Room;
                    }
                    ytemp += Random.Range(2, 5);
                }
                break;

            case 3:
                int roomSize1 = Random.Range(2, 4);
                int roomSize2 = Random.Range(2, 4);
                int roomSize3 = Random.Range(2, 4);
                W3roomSizes = new int[] { roomSize1, roomSize2, roomSize3 };
                int totalHeight = roomSize1 + roomSize2 + roomSize3;
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 5; y < totalHeight + 5; y++)
                    {
                        grid[x, y] = CellState.Room;
                    }
                }
                break;
        }

        Vector2Int? first = null;
        Vector2Int? last = null;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == CellState.Room)
                {
                    if (first == null)
                        first = new Vector2Int(x, y);
                    last = new Vector2Int(x, y);
                }
            }
        }

        if (first.HasValue) grid[first.Value.x, first.Value.y] = CellState.EnemySpawnPoint;
        if (last.HasValue) grid[last.Value.x, last.Value.y] = CellState.EnemySpawnPoint;

        // New: Create safe zone at center of grid
        int midX = gridWidth / 2;
        int midY = gridHeight / 2;
        grid[midX, midY] = CellState.Safezone;

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
                    case CellState.EnemySpawnPoint:
                        Gizmos.color = Color.red;
                        break;
                    case CellState.Safezone:
                        Gizmos.color = Color.blue;
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