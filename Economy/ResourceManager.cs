using CodeMonkey.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class ResourceManager : MonoBehaviour
{
    [SerializeField] MAINmanager mainManager;
    [SerializeField] TurnManager turnManager;
    Settlement settlement;
    EnemyManager enemyManager;

    ResourceBuilder resourceBuilder;
    ResourceManagerVisuals resourceManagerVisuals;

    GridHexXZ<GridBuildingSystem.GridObject> grid;

    TileObjects currentTileObject;
    
    public int currentFood = 100;
    public int currentGold = 100;
    public int currentWood = 100;
    public int currentPopulation = 5;
    public int currentHouses = 0;
    
    public int maxFood;
    public int maxGold;
    public int maxWood;

    bool IsBattling;
    bool canUseResourceManager = true;
    int warehouseModifier = 50;

    [HideInInspector] public List<TileObjects> LumberyardList = new List<TileObjects>();
    [HideInInspector] public List<TileObjects> GoldmineList = new List<TileObjects>();
    [HideInInspector] public List<TileObjects> FarmList = new List<TileObjects>();
    TileObjects settlementTile;

    List<int> lumberyardTierList;
    List<int> goldmineTierList;
    List<int> farmTierList;
    List<int> warehouseTierList;
    public List<int> tierUpgradePrice;

    int tileUpgradeCost;
    int maxTier = 3;
    int maxWorkers = 5;

    public Action OnBuildingUpgrade;
    public Action<TileObjects> OnMaxTier;
    string sentence = "";


    void Start()
    {
        resourceManagerVisuals = GetComponent<ResourceManagerVisuals>();    
        resourceBuilder = GetComponent<ResourceBuilder>();

        settlement = mainManager.GetSettlementScript();
        enemyManager = mainManager.GetEnemyManager();

        grid = turnManager.GetGrid();
        turnManager.OnEndOfTurn += OnNextTurn;


        lumberyardTierList = new List<int>() { 2, 3, 4, 5 };
        goldmineTierList = new List<int>() { 2, 3, 4, 5 };
        farmTierList = new List<int>() { 2, 3, 4, 5 };
        warehouseTierList = new List<int>() {400, 800, 1200, 1500, 2300 };
        tierUpgradePrice = new List<int>() { 100, 300, 400, 600 };

        maxFood = warehouseTierList[0]; maxGold = warehouseTierList[0]; maxWood = warehouseTierList[0];

        resourceManagerVisuals.RefreshResourceVisuals(currentWood, currentGold, currentFood,
                                                     currentPopulation, maxWood, maxGold, maxFood);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && grid.GetGridObject(UtilsClass.GetMouseWorldPosition()) != null
            && resourceBuilder.GetCurrentResourceToBuild() == null && !EventSystem.current.IsPointerOverGameObject()
            && grid.GetGridObject(UtilsClass.GetMouseWorldPosition()).GetTileObject().GetIsInteractable() && !IsBattling
            && canUseResourceManager)
        {
            

            currentTileObject = grid.GetGridObject(UtilsClass.GetMouseWorldPosition()).GetTileObject();

            if (currentTileObject.GetResourceType() == TileObjects.TileType.Village) {                 
                if(currentTileObject.GetComponent<VillageResources>().GetCurrentHealth() >= currentTileObject.GetComponent<VillageResources>().GetMaxHealth()
                || currentTileObject.GetComponent<VillageResources>().IsBeingAttacked)
            {
                    return;
                } }
            else if (currentTileObject.GetResourceType() == TileObjects.TileType.Castle)
            {
                if (enemyManager.GetIsAttackingSettlement()) return;
            }
                
                
            resourceManagerVisuals.DeactivateBuilderUI();

            if (currentTileObject.GetResourceType() == TileObjects.TileType.MagicStore)
            {
                resourceManagerVisuals.ActivateMagicShopUI();
            }
            else if (currentTileObject.GetResourceType() == TileObjects.TileType.Blacksmith)
            {
                resourceManagerVisuals.ActivateBlacksmithUI();
            }
            else
            {
                resourceManagerVisuals.ActivateResourceUpgradeUI(currentTileObject.GetTier(), currentTileObject.GetCurrentWorkers(),
                                                         currentTileObject.GetName(), currentTileObject.GetBuildingSprite(), currentTileObject);

                if (currentTileObject.GetResourceType() == TileObjects.TileType.Castle) resourceManagerVisuals.SetIsCastle(true);
            }
            resourceManagerVisuals.SetTierPrice(tileUpgradeCost);
        }
    }

    void OnNextTurn()
    {
        // settlement replenish
        ReplenishResources(3, ref currentFood, maxFood, ref sentence);
        ReplenishResources(3, ref currentWood, maxWood, ref sentence);
        ReplenishResources(3, ref currentGold, maxGold, ref sentence);

        FeedGuards();
        ReplenishResources(FarmList, farmTierList, ref currentFood, maxFood, ref sentence);
        ReplenishResources(LumberyardList, lumberyardTierList, ref currentWood, maxWood, ref sentence);
        ReplenishResources(GoldmineList, goldmineTierList, ref currentGold, maxGold, ref sentence);


        resourceManagerVisuals.RefreshResourceVisuals(currentWood, currentGold, currentFood, currentPopulation,
                                                        maxWood, maxGold, maxFood);
        if (sentence != "")
        {
            resourceManagerVisuals.ShowErrorText(sentence, 0, "", "");
            sentence = "";
        }

        ActivateMaximumWarning();
    }

    void FeedGuards()
    {
        if(currentFood >= settlementTile.GetCurrentWorkers()) currentFood -= settlementTile.GetCurrentWorkers();
        else
        {
            currentFood = 0;
            resourceManagerVisuals.ShowErrorText("Not enough food for all workers!", 0, "", "");
        }
    }
    public void OnVillageReplenish(int amount)
    {
        ReplenishResources(amount, ref currentFood, maxFood, ref sentence);
        ReplenishResources(amount, ref currentWood, maxWood, ref sentence);
        ReplenishResources(amount, ref currentGold, maxGold, ref sentence);

        resourceManagerVisuals.RefreshResourceVisuals(currentWood, currentGold, currentFood, currentPopulation,
                                                maxWood, maxGold, maxFood);
    }
    void ReplenishResources(int amount, ref int currentResource, int maxResource, ref string sentence)
    {

        if ((amount + currentResource) <= maxResource)
        {
            currentResource += amount;
        }
        else if (currentResource != maxResource)
        {
            currentResource = maxResource;
        }
    }
    void ReplenishResources(List<TileObjects> iterable, List<int> tierList, ref int currentResource, int maxResource, ref string sentence)
    {
        bool enoughFood;
        if (iterable != null)
        {
            foreach (TileObjects tile in iterable)
            {
                 enoughFood = (currentFood - tile.GetCurrentWorkers() >= 0) ? true : false;
                if(currentResource.ToString() == currentFood.ToString()) enoughFood = true;

                int resourceAmount = tierList[tile.GetTier()] * tile.GetCurrentWorkers();

                if (enoughFood && (resourceAmount + currentResource) <= maxResource)
                {
                    currentFood -= tile.GetCurrentWorkers();
                    currentResource += (resourceAmount);
                }
                else if (!enoughFood) sentence = "Not enough food for all workers!";
                else if(currentResource != maxResource)
                {
                    currentFood -= tile.GetCurrentWorkers();
                    currentResource = maxResource;
                }
            }
        }
    }

    void ActivateMaximumWarning()
    { 
        bool option;

        option = (currentWood >= maxWood) ? true : false; resourceManagerVisuals.ActivateResourceWarning(ResourceManagerVisuals.Resource.Wood, option);
        option = (currentGold >= maxGold) ? true : false; resourceManagerVisuals.ActivateResourceWarning(ResourceManagerVisuals.Resource.Gold, option);
        option = (currentFood >= maxFood) ? true : false; resourceManagerVisuals.ActivateResourceWarning(ResourceManagerVisuals.Resource.Food, option);
        option = (currentPopulation >= maxWorkers) ? true : false; resourceManagerVisuals.ActivateResourceWarning(ResourceManagerVisuals.Resource.Workers, option);
    }

    public void RefreshAfterBuilding()
    {
        resourceManagerVisuals.RefreshResourceVisuals(currentWood, currentGold, currentFood,
                                                     currentPopulation, maxWood, maxGold, maxFood);
    }

    public void AddToResourceList(TileObjects tileObject)
    {
        TileObjects.TileType tileType = tileObject.GetResourceType();

        if (tileType == TileObjects.TileType.Lumberyard) LumberyardList.Add(tileObject);
        else if(tileType == TileObjects.TileType.Goldmine) GoldmineList.Add(tileObject);
        else if(tileType == TileObjects.TileType.Farm) FarmList.Add(tileObject);
        else if (tileType == TileObjects.TileType.House)
        {
            currentHouses++; currentPopulation += 5;
            resourceManagerVisuals.RefreshResourceVisuals(currentWood, currentGold, currentFood,
                                                         currentPopulation, maxWood, maxGold, maxFood);
            if(currentHouses == tileObject.GetMaxBuildingNum())
            {
                resourceManagerVisuals.DisableBuildingPossibility(tileObject.GetResourceType());
            }        
        }
    }
    public void AddWarehouse()
    {
        maxWood += warehouseModifier;
        maxGold += warehouseModifier;
        maxFood += warehouseModifier;

        resourceManagerVisuals.RefreshResourceVisuals(currentWood, currentGold, currentFood, currentPopulation, maxWood, maxGold, maxFood);
    }

    // getters setters
    public int GetMaxTier()
    {
        return maxTier;
    }
    public int GetCurrentHouses()
    {
        return currentHouses;
    }
    public int GetCurrentFreeWorkers()
    {
        return currentPopulation;
    }
    public int GetCurrentWood()
    {
        return currentWood;
    }
    public int GetMaxWood()
    {
        return maxWood;
    }
    public void SetCurrentWood(int wood)
    {
        currentWood += wood;
    }
    public int GetCurrentGold()
    {
        return currentGold;
    }
    public void SetCurrentGold(int amount)
    {
        currentGold += amount;
    }
    public int GetCurrentFood()
    {
        return currentFood;
    }
    public void SetCurrentFood(int food)
    {
        currentFood += food;
    }
    public int GetCurrentWorkers()
    {
        return currentPopulation;
    }
    public void SetCurrentWorkers(int workers)
    {
        currentPopulation += workers;
    }
    public TileObjects GetCurrentTileObject()
    {
        return currentTileObject;
    }
    public void SetCurrentTileObject(TileObjects tileObject)
    {
        currentTileObject = tileObject;
        SetTileUpgradeCost(tileObject);
    }
    public void SetIsBattling(bool option)
    {
        IsBattling = option;
    }
    public void SetCanUse(bool option)
    {
        canUseResourceManager = option;
    }
    public void SetSettlementTile(TileObjects settlement)
    {
        settlementTile = settlement;
    }


    // buttons for upgrades
    public void RebuildSettlement()
    {
        int amount = 0;

        if (currentTileObject.GetResourceType() == TileObjects.TileType.Castle)
        {
            settlement = mainManager.GetSettlementScript();

            if (currentGold >= (settlement.GetMaxSettlementhealth() - settlement.GetSettlementHealth()))
            {
                amount = settlement.GetMaxSettlementhealth() - settlement.GetSettlementHealth();

                settlement.Repair();
            }
            else
            {
                SoundSystem.instance.PlaySound(SoundSystem.Sound.Error);
            }
        }
        else
        {
            VillageResources villageResources = currentTileObject.GetComponent<VillageResources>();
            if(currentGold >= (villageResources.GetMaxHealth() - villageResources.GetCurrentHealth()))
            {
                amount = villageResources.GetMaxHealth() - villageResources.GetCurrentHealth();
                villageResources.RepairVillage();
            }
            else
            {
                SoundSystem.instance.PlaySound(SoundSystem.Sound.Error);
            }
        }

        if(amount > 0)
        {
            currentGold -= amount;
            SoundSystem.instance.PlaySound(SoundSystem.Sound.Build);
            RefreshAfterBuilding();
        }
    }
    public void PressUpgradeButton(UpgradeType upgradeType)
    {
        if ((upgradeType == UpgradeType.Tier || upgradeType == UpgradeType.WarehouseTier)
                && currentTileObject.GetTier() < maxTier && tileUpgradeCost <= currentWood)
        {
            TierUpgrade(upgradeType);

            RefreshAfterBuilding();
            currentTileObject.ChangeBuildingSprite();

            SoundSystem.instance.PlaySound(SoundSystem.Sound.Build);
            resourceBuilder.SetCurrentResourceToBuild();
            //OnBuildingUpgrade?.Invoke();

            resourceBuilder.HighlightBuildings(false, currentTileObject.GetResourceType());
        }
        else if (upgradeType == UpgradeType.AddWorker && currentPopulation > 0
                    && currentTileObject.GetCurrentWorkers() < currentTileObject.GetMaxWorkers())
        {
            OnBuildingUpgrade?.Invoke();
            ManageWorkers(1);
        }
        else if (upgradeType == UpgradeType.RemoveWorker && currentTileObject.GetCurrentWorkers() > 0)
        {
            ManageWorkers(-1);
        }
        else
        {
            SoundSystem.instance.PlaySound(SoundSystem.Sound.Error);
            //resourceBuilder.SetCurrentResourceToBuild();
            SelectUpgradeErrorMessage(upgradeType);
        }
    }
    public void AddWorker()
    {
        PressUpgradeButton(UpgradeType.AddWorker);
    }
    public void RemoveWorker()
    {
        PressUpgradeButton(UpgradeType.RemoveWorker);
    }
    void ManageWorkers(int amount)
    {
        currentTileObject.AddWorkers(amount);
        currentPopulation -= amount;

        resourceManagerVisuals.ShowBuildingRequirements(ResourceManagerVisuals.TextType.workers);
        ShowUpgradeVisuals(UpgradeType.AddWorker, currentTileObject);
    }
    void TierUpgrade(UpgradeType upgradeType)
    {
        currentWood -= tierUpgradePrice[currentTileObject.GetTier()];
        currentTileObject.UpgradeTier();
        tileUpgradeCost = tierUpgradePrice[currentTileObject.GetTier()];

        resourceBuilder.SetCanDrag(false);
        resourceManagerVisuals.SetTierPrice(tileUpgradeCost);

        if (upgradeType == UpgradeType.WarehouseTier)
        {
            maxFood = warehouseTierList[currentTileObject.GetTier() + 1];
            maxGold = warehouseTierList[currentTileObject.GetTier() + 1];
            maxWood = warehouseTierList[currentTileObject.GetTier() + 1];

            if (resourceBuilder.GetWarehouseTile().GetTier() >= maxTier)
                resourceManagerVisuals.DisableBuildingPossibility(resourceBuilder.GetWarehouseTile().GetResourceType());
        }
        else if (currentTileObject.GetResourceType() == TileObjects.TileType.Castle)
        {
            currentTileObject.AddMaxWorkers(5);
            settlement.SetMaxSettlementHealth();
        }
        else if(currentTileObject.GetResourceType() == TileObjects.TileType.Village)
        {
            currentTileObject.GetComponent<VillageResources>().SetMaxHealth(50);
        }
        if (resourceBuilder.GetMagicShop() != null && resourceBuilder.GetMagicShop().GetTier() >= maxTier)
            resourceManagerVisuals.DisableBuildingPossibility(resourceBuilder.GetMagicShop().GetResourceType());

        if(settlementTile.GetTier() >= maxTier) 
            resourceManagerVisuals.DisableBuildingPossibility(settlementTile.GetResourceType());

        if (resourceBuilder.GetResourceTypeList().Contains(currentTileObject.GetResourceType())
            && currentTileObject.GetTier() >= maxTier) OnMaxTier?.Invoke(currentTileObject);
    }
    public int GetTierUpgradePrice(TileObjects tileObject)
    {
        return tierUpgradePrice[tileObject.GetTier()];
    }
    public void SetTileUpgradeCost(TileObjects tileObject)
    {
        tileUpgradeCost = tierUpgradePrice[tileObject.GetTier()];
    }

    // visuals
    void ShowUpgradeVisuals(UpgradeType upgradeType, TileObjects currentTileObject)
    {

        resourceManagerVisuals.ActivateResourceUpgradeUI(currentTileObject.GetTier(), currentTileObject.GetCurrentWorkers(),
                                                 currentTileObject.GetName(), currentTileObject.GetBuildingSprite(), currentTileObject);
        resourceManagerVisuals.RefreshResourceVisuals(currentWood, currentGold, currentFood,
                                                     currentPopulation, maxWood, maxGold, maxFood);
    }
    public void SelectUpgradeErrorMessage(UpgradeType upgradeType)
    {
        string sentence;

        if (currentTileObject.GetTier() >= maxTier && upgradeType == UpgradeType.Tier) sentence = "Already at maximum tier!";
        else if (tileUpgradeCost > currentWood && (upgradeType == UpgradeType.Tier || upgradeType == UpgradeType.WarehouseTier))
            sentence = $"Additional {tileUpgradeCost - currentWood} wood is required to upgrade tier";
        else if (currentTileObject.GetCurrentWorkers() >= maxWorkers && upgradeType == UpgradeType.AddWorker) sentence = "Already at maximum worker capacity!";
        else if (currentPopulation <= 0 && (upgradeType == UpgradeType.RemoveWorker || upgradeType == UpgradeType.AddWorker)) sentence = "Not enough available workers!";
        else sentence = "No workers at current building!";

        resourceManagerVisuals.ShowErrorText(sentence, 0, "", "");
    }

    public enum UpgradeType
    {
        Tier,
        WarehouseTier,
        AddWorker,
        RemoveWorker,
    }
}
