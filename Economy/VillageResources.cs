using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class VillageResources : MonoBehaviour
{
    bool IsConnected;
    TileObjects tileObject;

    Vector2Int settlementPosition;
    int x, y;

    PathFinding pathFinding;

    MAINmanager mainManager;
    ResourceManager resourceManager;
    ResourceBuilder resourceBuilder;
    ResourceManagerVisuals RMV;
    TurnManager turnManager;

    List<int> resourcePerTurn;

    GameObject statusBarGO;
    StatusBar statusBar;
    int maxHealth = 50;
    [SerializeField] int currentHealth;

    public bool IsBeingAttacked;

    void Start()
    {
        tileObject = GetComponent<TileObjects>();

        resourcePerTurn = new List<int>() { 5, 8, 12, 17 };

        turnManager.OnEndOfTurn += OnNextTurn;

        currentHealth = maxHealth;
    }

    void OnNextTurn()
    {
        if (IsConnected) resourceManager.OnVillageReplenish(resourcePerTurn[tileObject.GetTier()]);
    }

    public void SetReferences(MAINmanager mainManager, Vector2Int position, GameObject statusBarGO)
    {
        this.mainManager = mainManager;
        this.resourceManager = mainManager.GetResourceManager();
        this.resourceBuilder = mainManager.GetResourceBuilder();
        this.RMV = mainManager.GetRMV();
        this.turnManager = mainManager.GetTurnManager();
        this.settlementPosition = position;

        this.statusBarGO = statusBarGO;
        statusBarGO.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + .85f);

        this.statusBar = statusBarGO.GetComponent<StatusBar>();

        statusBar.SetMaxValue(maxHealth);
        statusBar.SetCurrentValue(maxHealth);
    }
    public void SetVillageXY()
    {
        pathFinding.GetGrid().GetXY(transform.position, out x, out y);
    }

    public void UpgradeVillageVisuals(int tier)
    {
        this.gameObject.transform.GetChild(3).GetComponent<SpriteRenderer>().sprite = tileObject.GetVillageSpriteList()[0];

        if (tier == 2)
        {
            this.gameObject.transform.GetChild(4).GetComponent<SpriteRenderer>().sprite = tileObject.GetVillageSpriteList()[1];
        }
        if (tier == 3) this.gameObject.transform.GetChild(5).GetComponent<SpriteRenderer>().sprite = tileObject.GetVillageSpriteList()[2];

    }

    public void DamageVillage(int amount)
    {
        statusBarGO.SetActive(true);

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        
        statusBar.SetCurrentValue(currentHealth);

        if(currentHealth <= 0)
        {
            IsConnected = false;
            IsBeingAttacked = false;
            RMV.ShowErrorText("A village has fallen!", 0, "", "");

        }

    }
    public void RepairVillage()
    {
        currentHealth = maxHealth;
        statusBar.SetCurrentValue(maxHealth);

        RMV.SetRepairVillage(false, 0);
        statusBar.gameObject.SetActive(false);
    }

    public void SetPathNodeGrid(GridHexXZ<PathNode> grid, PathFinding pathfining)
    {
        //pathNodeGrid = grid;
        pathFinding = pathfining;
        resourceBuilder.onRoadBuilt += CheckRoadConnection;

    }
    public void CheckRoadConnection()
    {
        List<PathNode> path = pathFinding.FindPath(x, y, settlementPosition.x, settlementPosition.y);

        if (path != null)
        {
            IsConnected = true;
            resourceBuilder.onRoadBuilt -= CheckRoadConnection;
        }
    }

    public bool GetIsConnected()
    {
        return IsConnected;
    }
    public void SetIsConnected()
    {
        IsConnected = true;
        resourceBuilder.onRoadBuilt -= CheckRoadConnection;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
    public void SetMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        statusBar.SetMaxValue(maxHealth);
        statusBar.SetCurrentValue(currentHealth);
    }
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void SetIsAttacked(bool option)
    {
        IsBeingAttacked = option;
    }
}
