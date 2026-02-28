using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RoadBuilder : MonoBehaviour
{
    GridBuildingSystem gridBuildingSystem;
    GridHexXZ<GridBuildingSystem.GridObject> grid;

    [SerializeField] RoadBuilder[] neighbours = new RoadBuilder[6];
    [SerializeField] Vector2Int[] neighbourLocations = new Vector2Int[6];
    [SerializeField] public bool[] roads;
    

    private void Start()
    {
        gridBuildingSystem = GetComponentInParent<BuilderLocator>().GetGridBuildingSystem();
        grid = gridBuildingSystem.GetGrid();

        int x, y;
        grid.GetXY(this.transform.position, out x, out y);


            SetUpNeighbours();

    }

    public bool HasRoadThroughEdge(HexDirection direction)
    {
        return roads[(int)direction];
    }

    [EditorCools.Button]
    public void PlaceRoad()
    {
        for(int i = 0; i < neighbours.Length; i++)
        {
            roads[i] = true;
            if (neighbours[i].roads[(int)((HexDirection)i).Opposite()] == true)
            {
                transform.GetChild(i).gameObject.SetActive(true);
                neighbours[i].transform.GetChild((int)((HexDirection)i).Opposite()).gameObject.SetActive(true);
            }
        }
    }
    public bool HasRoads()
    {

            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i])
                {
                    return true;
                }
            }
            return false;
    }
    public void AddRoad(HexDirection direction)
    {
        if (!roads[(int)direction])
        {
            SetRoad((int)direction, true);
        }
    }
    void SetRoad(int index, bool state)
    {
        roads[index] = state;
        neighbours[index].roads[(int)((HexDirection)index).Opposite()] = state;
    }

    public RoadBuilder[] GetNeighbours()
    {
        return neighbours;
    }
    void SetUpNeighbours()
    {
        neighbourLocations = new Vector2Int[6];

        grid.GetNeighboursForRoad(this.transform.position, ref neighbourLocations);


        for (int i = 0; i < neighbourLocations.Length; i++)
        {
            var neighbour = neighbourLocations[i];


            if (grid.GetGridObject(neighbour.x, neighbour.y) == null) continue;

            var gridObject = grid.GetGridObject(neighbour.x, neighbour.y);
            var tileObject = gridObject.GetTileObject();
            var roadBuilder = tileObject.GetComponentInChildren<RoadBuilder>();

            neighbours[i] = roadBuilder;

        }
    }
}

public enum HexDirection
{
    NE, E, SE, SW, W, NW
};

public static class HexDirectionExtensions
{
    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }
}