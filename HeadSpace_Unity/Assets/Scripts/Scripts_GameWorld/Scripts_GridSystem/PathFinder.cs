using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Besoin de fcns : 
 * - Linear pathfinding + vérif bool
 * - Diagonal pathfinding + vérif bool
 * - Fastest pathfinding
 */

public class PathFinder : MonoBehaviour
{
    public static PathFinder instance;

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private GridInfo currentGridInfo;
    private GridTile[,] grid;
    private List<PathNode> openList;
    private List<PathNode> closedList;

    //private void Update()
    //{
    //    if (currentGridInfo == null)
    //        return;

    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //        TileCoordinates tileCoords = GridCoords.FromWorldToTilePosition(mousePos);
    //        //List<PathNode> path = FindPath(0, 0, tileCoords.tileX, tileCoords.tileY);
    //        List<PathNode> path = FindLinearPath(2, 2, tileCoords.tileX, tileCoords.tileY);

    //        for (int i = 0; i < path.Count; i++)
    //        {
    //            if (i + 1 < path.Count)
    //            {
    //                Debug.DrawLine(path[i].tile.TileCenter, path[i + 1].tile.TileCenter, Color.green, 3f);
    //            }
    //        }
    //    }
    //}

    private void Awake()
    {
        // Déclaration du singleton
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        GridManager.newGameGrid += OnNewGameGrid;
    }

    private void OnDisable()
    {
        GridManager.newGameGrid -= OnNewGameGrid;
    }

    private void OnNewGameGrid(GridInfo info)
    {
        currentGridInfo = info;
        grid = info.gameGridTiles;
    }

    public List<PathNode> FindLinearPath(int startX, int startY, int endX, int endY, out float nodeCost)
    {
        nodeCost = 0f;

        if (!GridCoords.IsInGrid(startX, startY))
            return null;
        if (!GridCoords.IsInGrid(endX, endY))
            return null;

        PathNode startNode = grid[startX, startY].PathNode;
        PathNode endNode = grid[endX, endY].PathNode;

        // vertical
        if (startNode.x == endNode.x)
        {
            nodeCost = 1f;
            return GetVerticalPath(startNode, endNode);
        }
        // horizontal
        else if (startNode.y == endNode.y)
        {
            nodeCost = 1f;
            return GetHorizontalPath(startNode, endNode);
        }

        else if (Mathf.Abs(startNode.x - endNode.x) == Mathf.Abs(startNode.y - endNode.y))
        {
            nodeCost = 1.4142f;
            return GetDiagonalPath(startNode, endNode);
        }

        return null;
    }

    private List<PathNode> GetHorizontalPath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> horizontalPath = new List<PathNode>();

        // left
        if (endNode.x < startNode.x)
        {
            for (int x = startNode.x; x >= endNode.x; x--)
            {
                horizontalPath.Add(grid[x, startNode.y].PathNode);
            }
        }
        // right
        else
        {
            for (int x = startNode.x; x <= endNode.x; x++)
            {
                horizontalPath.Add(grid[x, startNode.y].PathNode);
            }
        }
        return horizontalPath;
    }

    private List<PathNode> GetVerticalPath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> verticalPath = new List<PathNode>();

        // down
        if (endNode.y < startNode.y)
        {
            for (int y = startNode.y; y >= endNode.y; y--)
            {
                verticalPath.Add(grid[startNode.x, y].PathNode);
            }
        }
        // up
        else
        {
            for (int y = startNode.y; y <= endNode.y; y++)
            {
                verticalPath.Add(grid[startNode.x, y].PathNode);
            }
        }
        return verticalPath;
    }

    private List<PathNode> GetDiagonalPath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> diagonalPath = new List<PathNode>();
        int count = 0;
        // left
        if (endNode.x < startNode.x)
        {
            // leftDown
            if (endNode.y < startNode.y)
                for (int x = startNode.x; x >= endNode.x; x--)
                {
                    diagonalPath.Add(grid[x, startNode.y - count].PathNode);
                    count++;
                }
            // leftUp
            else if (endNode.y > startNode.y)
                for (int x = startNode.x; x >= endNode.x; x--)
                {
                    diagonalPath.Add(grid[x, startNode.y + count].PathNode);
                    count++;
                }
        }

        // Right
        else if (endNode.x > startNode.x)
        {
            // rightDown
            if (endNode.y < startNode.y)
                for (int x = startNode.x; x <= endNode.x; x++)
                {
                    diagonalPath.Add(grid[x, startNode.y - count].PathNode);
                    count++;
                }
            else if (endNode.y > startNode.y)
                for (int x = startNode.x; x <= endNode.x; x++)
                {
                    diagonalPath.Add(grid[x, startNode.y + count].PathNode);
                    count++;
                }
        }
        return diagonalPath;
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        if (!GridCoords.IsInGrid(startX, startY))
            return null;
        if (!GridCoords.IsInGrid(endX, endY))
            return null;

        PathNode startNode = grid[startX, startY].PathNode;
        PathNode endNode = grid[endX, endY].PathNode;
        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();

        for (int x = 0; x < currentGridInfo.gameGridSize.x; x++)
        {
            for (int y = 0; y < currentGridInfo.gameGridSize.y; y++)
            {
                PathNode pathNode = grid[x, y].PathNode;
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while(openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbourNode in GetValidNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode))
                    continue;
                if (!neighbourNode.isTraversable)
                {
                    closedList.Add(neighbourNode);
                    continue;
                }
                    

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                        openList.Add(neighbourNode);
                }
            }
        }

        // Out of nodes on the openList
        return null;
    }

    public List<PathNode> GetValidNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        bool upBlocked = false;
        bool downBlocked = false;
        bool leftBlocked = false;
        bool rightBlocked = false;

        // Down
        if (currentNode.y - 1 >= 0)
        {
            neighbourList.Add(grid[currentNode.x, currentNode.y - 1].PathNode);
            if (grid[currentNode.x, currentNode.y - 1].PathNode.tile.tileType == 1)
                downBlocked = true;
        }
        // Up
        if (currentNode.y + 1 < currentGridInfo.gameGridSize.y)
        {
            neighbourList.Add(grid[currentNode.x, currentNode.y + 1].PathNode);
            if (grid[currentNode.x, currentNode.y + 1].PathNode.tile.tileType == 1)
                upBlocked = true;
        }

        if (currentNode.x - 1 >= 0)
        {
            // Left
            neighbourList.Add(grid[currentNode.x - 1, currentNode.y].PathNode);
            if (grid[currentNode.x - 1, currentNode.y].PathNode.tile.tileType == 1)
                leftBlocked = true;

            // Left down
            if (leftBlocked && downBlocked)
            { }
            else
                if (currentNode.y - 1 >= 0)
                    neighbourList.Add(grid[currentNode.x - 1, currentNode.y - 1].PathNode);
            
            // Left up
            if (leftBlocked && upBlocked)
            { }
            else
                if (currentNode.y + 1 < currentGridInfo.gameGridSize.y)
                    neighbourList.Add(grid[currentNode.x - 1, currentNode.y + 1].PathNode);
        }
        if (currentNode.x + 1 < currentGridInfo.gameGridSize.x)
        {
            // Right
            neighbourList.Add(grid[currentNode.x + 1, currentNode.y].PathNode);
            if (grid[currentNode.x + 1, currentNode.y].PathNode.tile.tileType == 1)
                rightBlocked = true;

            // Right down
            if (rightBlocked && downBlocked)
            { }
            else
                if (currentNode.y - 1 >= 0)
                    neighbourList.Add(grid[currentNode.x + 1, currentNode.y - 1].PathNode);


            // Right up
            if (rightBlocked && upBlocked)
            {
            }
            else
                if (currentNode.y + 1 < currentGridInfo.gameGridSize.y)
                    neighbourList.Add(grid[currentNode.x + 1, currentNode.y + 1].PathNode);
        }
        return neighbourList;
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - a.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 0; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
                lowestFCostNode = pathNodeList[i];
        }
        return lowestFCostNode;
    }
}
