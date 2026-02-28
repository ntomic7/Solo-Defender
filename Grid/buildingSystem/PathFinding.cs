using System.Collections.Generic;
using UnityEngine;

public class PathFinding
{
    const int MOVE_STRAIGHT_COST = 10;
    const int MOVE_DIAGONAL_COST = 12;

    GridHexXZ<PathNode> pathNodeGrid;
    GridHexXZ<GridBuildingSystem.GridObject> mainGrid;

    List<PathNode> openList;
    List<PathNode> closedList;

    public PathFinding(int width, int height, GridHexXZ<GridBuildingSystem.GridObject> grid)
    {
        mainGrid = grid;


        pathNodeGrid = new GridHexXZ<PathNode>(width, height, 1.01f, Vector3.zero, (GridHexXZ<PathNode> g, int x, int y) => new PathNode(g, x, y, mainGrid));
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = pathNodeGrid.GetGridObject(startX, startY);
        PathNode endNode = pathNodeGrid.GetGridObject(endX, endY);

        openList = new List<PathNode>() { startNode};
        closedList = new List<PathNode>();

        for(int x = 0; x < pathNodeGrid.GetWidth(); x++)
        {
            for(int y = 0; y < pathNodeGrid.GetHeight(); y++)
            {
                PathNode pathNode = pathNodeGrid.GetGridObject(x, y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();


        while (openList.Count > 0)
        {

            PathNode currentNode = GetLowestFCostNode(openList);

            if (currentNode == endNode) return Calculatepath(endNode);
                    
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            

            foreach(PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                neighbourNode.GetIsRoad();

                if (closedList.Contains(neighbourNode)) continue;
                if (!neighbourNode.GetIsRoad())
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + MOVE_STRAIGHT_COST;
                if(tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }
        return null;
    }

    List<PathNode> Calculatepath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
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

    int CalculateDistanceCost(PathNode a, PathNode b)
    {
        return Mathf.RoundToInt(MOVE_STRAIGHT_COST * Vector3.Distance(pathNodeGrid.GetWorldPosition(a.x, a.y), pathNodeGrid.GetWorldPosition(b.x, b.y)));
    }

    PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];

        for(int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode =pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }

    List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbours = new List<PathNode>();

        bool oddRow = currentNode.y % 2 == 1;

        if (currentNode.x - 1 >= 0) neighbours.Add(GetNode(currentNode.x - 1, currentNode.y));
        if (currentNode.x + 1 < pathNodeGrid.GetWidth()) neighbours.Add(GetNode(currentNode.x + 1, currentNode.y));
        if (currentNode.y - 1 >= 0) neighbours.Add(GetNode(currentNode.x, currentNode.y - 1));
        if (currentNode.y + 1 < pathNodeGrid.GetHeight()) neighbours.Add(GetNode(currentNode.x, currentNode.y + 1));

        if (oddRow)
        {
            if (currentNode.y + 1 < pathNodeGrid.GetHeight() && currentNode.x + 1 < pathNodeGrid.GetWidth()) neighbours.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
            if (currentNode.y - 1 >= 0 && currentNode.x + 1 < pathNodeGrid.GetWidth()) neighbours.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
        }
        else
        {
            if (currentNode.y + 1 < pathNodeGrid.GetHeight() && currentNode.x - 1 >= 0) neighbours.Add(GetNode(currentNode.x - 1, currentNode.y + 1));

            if (currentNode.y - 1 >= 0 && currentNode.x - 1 >= 0) neighbours.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
        }



        return neighbours;
    }
    PathNode GetNode(int x, int y)
    {
        return pathNodeGrid.GetGridObject(x, y);
    }

    public GridHexXZ<PathNode> GetGrid()
    {
        return pathNodeGrid;
    }

}
