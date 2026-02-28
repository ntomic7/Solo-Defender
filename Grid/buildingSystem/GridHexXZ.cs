using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class GridHexXZ<TGridObject>
{
    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x, y;
    }

    int width;
    int height;
    float cellSize;
    public TGridObject[,] gridArray;
    private Vector3 originPosition;

    const float HEX_VERTICAL_OFFSET = 0.75f;

    public GridHexXZ(int width, int height, float cellSize, Vector3 originPosition, Func<GridHexXZ<TGridObject>, int, int, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }
    }

    public int GetWidth()
    {
        return width;
    }
    public int GetHeight()
    {
        return height;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, 0, 0) * cellSize +
               new Vector3(0, y, 0) * cellSize * HEX_VERTICAL_OFFSET +
               ((Mathf.Abs(y) % 2) == 1 ? new Vector3(1, 0, 0) * cellSize * .5f : Vector3.zero)
               + originPosition;
    }
    public Vector3 GetWorldPosition(int x, int y, float cellSize)
    {
        return new Vector3(x, 0, 0) * cellSize +
               new Vector3(0, y, 0) * cellSize * HEX_VERTICAL_OFFSET;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        int roughY = Mathf.RoundToInt((worldPosition - originPosition).y / (cellSize * HEX_VERTICAL_OFFSET));
        int roughX = Mathf.RoundToInt(((worldPosition - originPosition).x - ((roughY % 2 == 1) ? cellSize * .5f : 0f)) / cellSize);

        Vector2Int roughXY = new Vector2Int(roughX, roughY);
        bool oddRow = roughY % 2 == 1;

        List<Vector2Int> roughXYList = new List<Vector2Int>
        {
            roughXY + new Vector2Int(-1, 0),
            roughXY + new Vector2Int(+1, 0),
            roughXY + new Vector2Int(oddRow ? +1 : -1, +1),
            roughXY + new Vector2Int(0, +1),
            roughXY + new Vector2Int(oddRow ? + 1 : -1, -1),
            roughXY + new Vector2Int(0, -1),
        };

        Vector2Int closestXY = roughXY;
        float closestDistance = Vector3.Distance(worldPosition, GetWorldPosition(roughXY.x, roughXY.y));

        foreach (Vector2Int neighborXY in roughXYList)
        {
            if (neighborXY.x >= 0 && neighborXY.x < width && neighborXY.y >= 0 && neighborXY.y < height)
            {
                float distance = Vector3.Distance(worldPosition, GetWorldPosition(neighborXY.x, neighborXY.y));
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestXY = neighborXY;
                }
            }
        }

        x = closestXY.x;
        y = closestXY.y;
    }

    public List<Vector2Int> GetSurroundingGridObjects(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        bool oddRow = (y % 2 == 1);

        List<Vector2Int> surroundingTilePositionList = new List<Vector2Int>();
        TryAddTile(x - 1, y, surroundingTilePositionList);
        TryAddTile(x + 1, y, surroundingTilePositionList);
        TryAddTile(oddRow ? x + 1 : x - 1, y + 1, surroundingTilePositionList);
        TryAddTile(x, y + 1, surroundingTilePositionList);
        TryAddTile(oddRow ? x + 1 : x - 1, y - 1, surroundingTilePositionList);
        TryAddTile(x, y - 1, surroundingTilePositionList);

        return surroundingTilePositionList;
    }

    void TryAddTile(int x, int y, List<Vector2Int> surroundingTilePositionList)
    {
        if(x >= 0 && x < gridArray.GetLength(0) &&  
           y >= 0 && y < gridArray.GetLength(1))
        {
            surroundingTilePositionList.Add(new Vector2Int(x, y));
        }
    }
    public void GetNeighboursForRoad(Vector2 worldPosition, ref Vector2Int[] neighbourocations)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        bool oddRow = (y % 2 == 1);

        TryAddTileForRoad(oddRow ? x + 1 : x, y + 1, 0, ref neighbourocations);
        TryAddTileForRoad(x + 1, y, 1, ref neighbourocations);
        TryAddTileForRoad(oddRow ? x + 1 : x , y - 1, 2, ref neighbourocations);
        TryAddTileForRoad(oddRow ? x : x - 1, y - 1, 3, ref neighbourocations);
        TryAddTileForRoad(x - 1, y, 4, ref neighbourocations);

        TryAddTileForRoad(oddRow ? x : x - 1, y + 1, 5, ref neighbourocations);


        //TryAddTileForRoad(x, y + 1, 0, ref neighbourocations);


    }
    public void TryAddTileForRoad(int x, int y, int index, ref Vector2Int[] neighbourLocations)
    {
        if (x >= 0 && x < gridArray.GetLength(0) &&
            y >= 0 && y <= gridArray.GetLength(1))
        {
            neighbourLocations[index] = new Vector2Int(x, y);
        }
    }

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }

    }
    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }
    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            if (OnGridObjectChanged != null)
                OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
        }
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        if (OnGridObjectChanged != null)
            OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }
}
