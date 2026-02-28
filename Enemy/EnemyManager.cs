using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] MAINmanager mainManager;
    GridHexXZ<GridBuildingSystem.GridObject> grid;
    [SerializeField] Transform playerLocation;

    [SerializeField] List<GameObject> enemyTypes;
    [SerializeField] List<GameObject> currentEnemies = new List<GameObject>();

    TurnManager turnManager;
    PlayerMovement playerMovement;
    PlayerLevelManager playerLevelManager;
    BattleManager battleManager;

    ResourceManagerVisuals RMV;
    
    Settlement settlement;

    List<Vector2Int> villagePositions = new List<Vector2Int>();
    Vector2Int settlementPosition = new Vector2Int();

    public Action<int> OnReloadEnemies;

    int currentEnemyCount;
    int maxEnemyCount = 10;

    // settlement Attack

    bool IsAttackingSettlement;
    [SerializeField] GameObject settlementAttackWarning;

    private void Awake()
    {
        mainManager.OnMapAwake += OnMapAwake;

        villagePositions = mainManager.GetVillageGridPositions();
        settlement = mainManager.GetSettlementScript();
        grid = mainManager.GetGrid();

        turnManager = mainManager.GetTurnManager();
        playerMovement = mainManager.GetPlayerMovement();
        playerLevelManager = playerMovement.gameObject.GetComponent<PlayerLevelManager>();
        battleManager = mainManager.GetBattleManager();

        RMV = mainManager.GetRMV();
    }

    void OnMapAwake()
    {
        settlementPosition = mainManager.GetSettlementGridPosition();
    }

    public int GetMaxEnemyCount()
    {
        return maxEnemyCount;
    }
    public int GetEnemyCount()
    {
        return currentEnemyCount;
    }
    public void SetEnemyCount(int amount)
    {
        currentEnemyCount += amount;
        if(amount == 0) currentEnemyCount = 0;
    }

    public bool GetIsAttackingSettlement()
    {
        return IsAttackingSettlement;
    }
    public void SetIsAttackingSettlement(bool b)
    {
        IsAttackingSettlement = b;

        RMV.SetRepairSettlement(settlement.GetSettlementHealth() < settlement.GetMaxSettlementhealth(),
            IsAttackingSettlement,(settlement.GetMaxSettlementhealth() - settlement.GetSettlementHealth()));
    }

    public void WarningText(string building)
    {
        if(building == "Settlement") RMV.ShowErrorText("The settlement is being attacked!", 0, "", "");
        else if(building == "Village") RMV.ShowErrorText("A village is being attacked!", 0, "", "");
    }

    public List<GameObject> GetCurrentEnemyList()
    {
        return currentEnemies;
    }
    public void ClearEnemyList()
    {
        currentEnemies.Clear();
    }
    public void ActivateEnemies()
    {
        foreach (GameObject enemy in GetCurrentEnemyList()) enemy.SetActive(true);
    }
    public void DeactivateEnemiesDuringbattle(GameObject enemyBattling)
    {
        foreach(GameObject enemy in currentEnemies)
        {
            if(enemy != enemyBattling) enemy.SetActive(false);
        }
    }
    public void ReloadEnemy(GameObject enemyGO, Vector3 position, int turnCount)
    {
        GameObject enemy = Instantiate(enemyGO, position, Quaternion.identity);
        enemy.GetComponent<EnemyMovement>().SetManagers(turnManager, this, GetSettlement());
        enemy.GetComponent<EnemyMovement>().SetEnemyTurnCount(turnCount);
        AddEnemy(enemy);
    }
    public void SetEnemyManagers(ref GameObject enemy)
    {
        enemy.GetComponent<EnemyMovement>().SetManagers(turnManager, this, settlement);
    }

    public void AddEnemy(GameObject enemy)
    {
        currentEnemies.Add(enemy);
        SetEnemyCount(1);
    }
    public void RemoveEnemy(GameObject enemy)
    {
        currentEnemies.Remove(enemy);
        if (enemy.GetComponent<EnemyBuildingAttack>().IsAttackingSettlement) SetIsAttackingSettlement(false);
    }

    public GameObject GetEnemyType(string name)
    {
        foreach(GameObject enemy in enemyTypes)
        {
            if(enemy.GetComponent<EnemyObjects>().GetEnemyType() == name)
            {
                return enemy;
            }
        }

        return null;
    }

    public Vector2Int GetPlayerPosition()
    {
        grid.GetXY(playerLocation.position, out int x, out int y);

        return new Vector2Int(x,y);
    }

    public void AddVillagePosition(Vector2Int transform)
    {
        villagePositions.Add(transform);
    }
    public void SetSettlementPosition(Vector2Int position)
    {
        settlementPosition = position;
    }
    public void SetIsBuildingAttacked(GameObject go,  bool isAttacked)
    {
        mainManager.GetTeleport().SetIsAttacked(go, isAttacked);

        if (go == mainManager.GetSettlement() && isAttacked == false) IsAttackingSettlement = false;
        if(go != mainManager.GetSettlement() && isAttacked == false) go.GetComponent<VillageResources>().SetIsAttacked(false);

        if (IsAttackingSettlement) settlementAttackWarning.SetActive(true);
        else settlementAttackWarning.SetActive (false);
        
    }

    public List<Vector2Int> GetVillagePositions()
    {
        return villagePositions;
    }
    public Vector2Int GetSettlementPosition()
    {
        return settlementPosition;
    }
    public Settlement GetSettlement()
    {
        return settlement;
    }
    public PlayerMovement GetPlayerMovement()
    {
        return playerMovement;
    }
    public PlayerLevelManager GetPlayerLevelManager()
    {
        return playerLevelManager;
    }
    public BattleManager GetBattleManager()
    {
        return battleManager;
    }
    public BattleVisuals GetBattleVisuals()
    {
        return mainManager.GetBattleVisuals();
    }
    public TurnManager GetTurnManager()
    {
        return turnManager;
    }

}
