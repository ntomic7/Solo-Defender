using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class FogOfWar : MonoBehaviour
{
    [SerializeField] MAINmanager mainManager;
    GridHexXZ<GridBuildingSystem.GridObject> grid;

    TurnManager turnManager;
    void Start()
    {
        mainManager.OnMapAwake += OnMapAwake;
       
        turnManager = mainManager.GetTurnManager();
        turnManager.OnEndOfTurn += ClearFOW;
    }

    void OnMapAwake()
    {
        grid = mainManager.GetGrid();

        StartCoroutine(WaitForMapLoad());
    }
    IEnumerator WaitForMapLoad()
    {
        yield return new WaitForSeconds(.1f);
        ClearFOW();
        grid.GetXY(transform.position, out int x, out int y);
        ClearTile(x, y);

        mainManager.OnMapAwake -= OnMapAwake;

    }


    void ClearFOW()
    {

        List<Vector2Int> neighbours = grid.GetSurroundingGridObjects(transform.position);
        foreach(Vector2Int neighbor in neighbours)
        {
            if (grid.GetGridObject(neighbor.x, neighbor.y).GetTileObject().GetFOW() != null)
            {
                grid.GetGridObject(neighbor.x, neighbor.y).GetTileObject().GetFOW().gameObject.SetActive(false);
                grid.GetGridObject(neighbor.x, neighbor.y).GetTileObject().SetIsFOW();
            }
        }
    }
    public void ClearFOW(Vector3 position)
    {
        List<Vector2Int> neighbours = grid.GetSurroundingGridObjects(position);
        foreach (Vector2Int neighbor in neighbours)
        {
            if (grid.GetGridObject(neighbor.x, neighbor.y).GetTileObject().GetFOW() != null)
            {
                grid.GetGridObject(neighbor.x, neighbor.y).GetTileObject().GetFOW().gameObject.SetActive(false);
                grid.GetGridObject(neighbor.x, neighbor.y).GetTileObject().SetIsFOW();
            }
        }
    }
    public void ClearTile(int x, int y)
    {
    
        if (grid.GetGridObject(x, y).GetTileObject().GetFOW() != null)
        {
            grid.GetGridObject(x, y).GetTileObject().GetFOW().gameObject.SetActive(false);
            grid.GetGridObject(x, y).GetTileObject().SetIsFOW();
        }
    }

    public void ClearCircle()
    {
        ClearFOW();

        grid.GetXY(transform.position, out int x, out int y);
        ClearTile(x, y);
    }
}
