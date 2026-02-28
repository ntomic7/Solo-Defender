using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    GridHexXZ<PathNode> pathNodeGrid;
    GridHexXZ<GridBuildingSystem.GridObject> gridObjectGrid;

    TileObjects tileObject;
    RoadBuilder road;

    public int x, y;
    public int gCost, hCost, fCost;


    public PathNode cameFromNode;

    public PathNode(GridHexXZ<PathNode> grid,  int x, int y, GridHexXZ<GridBuildingSystem.GridObject> gridObjectGrid)
    {
        this.pathNodeGrid = grid;
        this.x = x;
        this.y = y;
        this.gridObjectGrid = gridObjectGrid;

        tileObject = gridObjectGrid.GetGridObject(x, y).GetTileObject();
        road = tileObject.GetComponentInChildren<RoadBuilder>();
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public TileObjects.TileType GetTileType()
    {
        return tileObject.GetResourceType();
    }
    public bool GetIsRoad()
    {
        return tileObject.GetIsRoad();
    }
    public RoadBuilder GetRoadBuilder()
    {
        return road;
    }

    public override string ToString()
    {
        return x + " " + y;
    }
}
