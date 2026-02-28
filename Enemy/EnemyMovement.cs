using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using static GridBuildingSystem;

public class EnemyMovement : MonoBehaviour
{
    TurnManager turnManager;
    EnemyManager enemyManager;
    BattleManager battleManager;
    [SerializeField] EnemyBuildingAttack enemyBuildingAttack;

    Settlement settlement;
    TileObjects villageTile;

    GridHexXZ<GridObject> grid;

    List<Vector2Int> tileList = new List<Vector2Int>();
    Vector2Int enemyGridPosition;
    Vector2Int closestTile = new Vector2Int();

    List<Vector2Int> villageLocations = new List<Vector2Int>();
    Vector2Int chosenVillage;
    Vector2Int settlementCenter;
    TileObjects settlementTile;

    PlayerMovement playerMovement;
    Vector2Int playerLocation;
    int playerX, playerY;

    bool isFollowingPlayer;
    bool IsMovingToVillage;

    [SerializeField] int turnCount;
    int timer;
    int enemyFOV = 5;

    private IEnumerator Start()
    {
        while (turnManager == null || turnManager.GetGrid() == null)
        {
            yield return null;
        }
        grid = turnManager.GetGrid();

        if (!TryUpdateGridPosition())
        {
            yield break;
        }

    }
    bool TryUpdateGridPosition()
    {
        if (grid == null) return false;

        try
        {
            grid.GetXY(transform.position, out int newX, out int newY);
            enemyGridPosition = new Vector2Int(newX, newY);
            grid.GetGridObject(enemyGridPosition.x, enemyGridPosition.y).SetIsOccupied(true);
            grid.GetGridObject(enemyGridPosition.x, enemyGridPosition.y).SetIsEnemy(true);
            playerMovement = enemyManager.GetPlayerMovement();
            playerMovement.OnPlayerMovement += OnUpdatePlayerCoords;
            playerLocation = enemyManager.GetPlayerPosition();
            battleManager = enemyManager.GetBattleManager();

            settlementTile = grid.GetGridObject(settlementCenter.x, settlementCenter.y).GetTileObject();

            return true;
        }
        catch
        {
            return false;
        }
    }
    void OnBeginNextTurn()
    {
        turnCount++;

        AddSurroundingTiles();

        ChooseClosestTile();

        CheckEnemyMovement();
    }

    void AddSurroundingTiles()
    {
        tileList.Clear();

        closestTile = new Vector2Int();

        playerLocation = enemyManager.GetPlayerPosition();

        foreach (Vector2Int tile in grid.GetSurroundingGridObjects(transform.position))
        {
            if (!grid.GetGridObject(tile.x, tile.y).GetIsEnemy() ||
                (tile.x == playerX && tile.y == playerY && !battleManager.GetIsBattling()) || grid.GetGridObject(tile.x, tile.y).GetTileObject().GetResourceType() == TileObjects.TileType.Village)
            {
                tileList.Add(tile);
            }
            if (tile.x == playerX && tile.y == playerY && !battleManager.GetIsBattling())
            {
                closestTile = tile;
                CheckEnemyMovement();

                return;
            }

        }
    }
    void ChooseClosestTile()
    {
        if ((Mathf.Abs(playerX - enemyGridPosition.x) < enemyFOV && Mathf.Abs(playerY - enemyGridPosition.y) < enemyFOV) || isFollowingPlayer)
        {
            isFollowingPlayer = true;
            closestTile = FindClosestToTarget(enemyGridPosition, new Vector2Int((int)playerLocation.x, (int)playerLocation.y), tileList);
        }
        else if (IsNearVillage())
        {
            IsMovingToVillage = true;
            closestTile = FindClosestToTarget(enemyGridPosition, chosenVillage, tileList);
        }
        else if (!isFollowingPlayer && !IsMovingToVillage)
        {
            if (turnCount < timer)
            {
                closestTile = tileList[Random.Range(0, tileList.Count)];
            }
            else
            {
                closestTile = FindClosestToTarget(enemyGridPosition, settlementCenter, tileList);
            }
        }
    }
    public void CheckEnemyMovement()
    {
        if(enemyGridPosition == new Vector2(playerX, playerY) && !battleManager.GetIsBattling())
        {
            battleManager.StartBattle(this.gameObject);
        }
        else if(enemyGridPosition == settlementCenter)
        {
            enemyBuildingAttack.AttackSettlement(settlement, grid.GetGridObject(settlementCenter.x, settlementCenter.y).GetTileObject().gameObject);
            grid.GetGridObject(enemyGridPosition.x, enemyGridPosition.y).SetIsOccupied(false);

            return;
        }
        else if(enemyGridPosition == chosenVillage && IsMovingToVillage)
        {
            IsMovingToVillage = enemyBuildingAttack.AttackVillage(ref villageTile, ref chosenVillage, ref villageLocations);

        }
        else
        {
            BasicMove();
        }

        if (enemyGridPosition == new Vector2(playerX, playerY))
        {
            battleManager.StartBattle(this.gameObject);
        }
    }
    void BasicMove()
    {
        grid.GetGridObject(enemyGridPosition.x, enemyGridPosition.y).SetIsOccupied(false);
        grid.GetGridObject(enemyGridPosition.x, enemyGridPosition.y).SetIsEnemy(false);

        transform.position = grid.GetWorldPosition(closestTile.x, closestTile.y);

        grid.GetGridObject(closestTile.x, closestTile.y).SetIsOccupied(true);
        grid.GetGridObject(closestTile.x, closestTile.y).SetIsEnemy(true);
        enemyGridPosition = closestTile;
    }

    bool IsNearVillage()
    {
        foreach(Vector2Int vec in villageLocations)
        {
            VillageResources villageResources = grid.GetGridObject(vec.x, vec.y).GetTileObject().GetComponent<VillageResources>();

            if (Mathf.Abs(vec.x - enemyGridPosition.x) < 5 && Mathf.Abs(vec.y - enemyGridPosition.y) < 5
                && villageResources.GetCurrentHealth() > 0 && villageResources.GetIsConnected())
            {
                villageTile = grid.GetGridObject(vec.x, vec.y).GetTileObject();
                chosenVillage = vec;
                return true;
            }
        }
        return false;

    }
    Vector2Int FindClosestToTarget(Vector2Int center, Vector2Int target, List<Vector2Int> surroundingTiles)
    {
        Vector3Int targetCube = OffsetToCube(target);
        Vector2Int closestTile = surroundingTiles[0];
        int minDistance = int.MaxValue;

        foreach (Vector2Int tile in surroundingTiles)
        {
            Vector3Int tileCube = OffsetToCube(tile);
            int distance = HexDistance(tileCube, targetCube);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestTile = tile;
            }
        }

        return closestTile;
    }
    Vector3Int OffsetToCube(Vector2Int offsetPos)
    {
        int x = offsetPos.x - (offsetPos.y / 2);
        int z = offsetPos.y;
        int y = -x - z;
        return new Vector3Int(x, y, z);
    }
    int HexDistance(Vector3Int a, Vector3Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;
    }

    void OnUpdatePlayerCoords(Vector2Int playerCoords)
    {
        playerX = playerCoords.x;
        playerY = playerCoords.y;
    }

    
    private void OnDestroy()
    {

        if (turnManager != null)
        {
            turnManager.OnEndOfTurn -= OnBeginNextTurn;
            enemyManager.SetEnemyCount(-1);

            if(grid != null) grid.GetGridObject(closestTile.x, closestTile.y).SetIsOccupied(false);
        }
        if (enemyBuildingAttack.GetIsAttackingVillage())
        {
            enemyManager.SetIsBuildingAttacked(villageTile.gameObject, false);
        }
        else if (enemyBuildingAttack.GetIsAttackingSettlement() && settlementTile != null)
        {
            enemyManager.SetIsBuildingAttacked(settlementTile.gameObject, false);
            enemyManager.SetIsAttackingSettlement(false);

        }
        grid.GetGridObject(enemyGridPosition.x, enemyGridPosition.y).SetIsEnemy(false);
    }

    public int GetEnemyTurnCount()
    {
        return turnCount;
    }
    public void SetEnemyTurnCount(int num)
    {
        turnCount = num;
    }

    void SetTimer()
    {
        timer = Random.Range(20, 50);
    }
    public void SetManagers(TurnManager turnManager, EnemyManager enemyManager, Settlement settlement)
    {
        this.turnManager = turnManager;
        this.enemyManager = enemyManager;
        this.settlement = settlement;

        villageLocations = enemyManager.GetVillagePositions();
        settlementCenter = enemyManager.GetSettlementPosition();

        enemyBuildingAttack = GetComponent<EnemyBuildingAttack>();
        enemyBuildingAttack.SetEnemymanager(enemyManager);

        turnManager.OnEndOfTurn += OnBeginNextTurn;


        SetTimer();
    }
}
