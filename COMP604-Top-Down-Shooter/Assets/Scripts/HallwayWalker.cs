using System.Collections.Generic;
using UnityEngine;

public class HallwayWalker
{
    public Vector2Int Position { get; set; }
    private GridManager gridManager;

    public HallwayWalker(Vector2Int startPosition, GridManager manager)
    {
        Position = startPosition;
        gridManager = manager;
    }

    // Dictionary maps an integer (0-3) to a direction vector
    private readonly Dictionary<int, Vector2Int> directionMap = new Dictionary<int, Vector2Int>
    {
        { 0, Vector2Int.up },    // North
        { 1, Vector2Int.right }, // East
        { 2, Vector2Int.down },  // South
        { 3, Vector2Int.left }   // West
    };

    public Vector2Int Move()
    {
        Vector2Int newPosition;

        // Loop until a valid next position is found within the grid
        do
        {
            // Choose a random direction (0, 1, 2, or 3)
            int randomDirectionKey = Random.Range(0, directionMap.Count);
            Vector2Int direction = directionMap[randomDirectionKey];

            // Calculate new position
            newPosition = Position + direction;

        } while (!gridManager.IsInBounds(newPosition)); // Check if the new position is inside the grid

        // Update the walker's position
        Position = newPosition;
        return Position;
    }
}
