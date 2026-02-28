using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static GridBuildingSystem;
using static ResourceManagerVisuals;

public class ResourceBuilder : MonoBehaviour
{
    GridHexXZ<GridObject> gridHex;
    [SerializeField] MAINmanager mainManager;

    BattleManager battleManager;
    EnemyManager enemyManager;
    ResourceManager resourceManager;
    ResourceManagerVisuals resourceManagerVisuals;
    MagicShop magicShop;
    VisualManager visualManager;

    [SerializeField] List<TileObjects> resourceBuildingsList;
    List<TileObjects.TileType> resourceTypeList;
    TileObjects currentResourceToBuild = null;

    public bool IsUsingBuilder;
    bool IsBattling;
    bool canBuild;
    bool canActivateBuilder = true;

    public Action<bool, TileObjects> onIsBuilding;
    public Action onRoadBuilt;
    public Action OnBuilding;

    public Action<Vector3> OnResourceLocationAdded;

    // highlight objects
    
    TileObjects warehouseTile = null;
    TileObjects magicShopTile = null;
    TileObjects blacksmithTile = null;
    TileObjects settlementTile = null;

    List<TileObjects> villagePositions = new List<TileObjects>();
    List<TileObjects> farmRuinPositions = new List<TileObjects>();
    List<TileObjects> lumberyardRuinPositions = new List<TileObjects>();
    List<TileObjects> goldmineRuinPositions = new List<TileObjects>();
    [SerializeField] List<GameObject> highlightObjects;

    bool activated;

    [SerializeField] GameObject road;
    void Start()
    {
        mainManager.OnMapAwake += OnMapAwake;

        gridHex = mainManager.GetGrid();
        resourceManager = GetComponent<ResourceManager>();
        resourceManagerVisuals = GetComponent<ResourceManagerVisuals>();
        visualManager = mainManager.GetVisualManager();
        battleManager = mainManager.GetBattleManager();

        magicShop = GetComponent<MagicShop>();

        resourceTypeList = new List<TileObjects.TileType>()
        {
            TileObjects.TileType.Farm,
            TileObjects.TileType.Goldmine,
            TileObjects.TileType.Lumberyard,
            TileObjects.TileType.Castle,
            TileObjects.TileType.Village,
            TileObjects.TileType.Road,
        };
        
        battleManager.OnBattleStart += SetIsBattling;
        resourceManagerVisuals.OnBuilderChange += SetIsBuilding;
    }

    void Update()
    { 
        if (Input.GetKeyDown(KeyCode.B))
        {
            ActivateBuilder();
        }

        // fix later
        if (Input.GetMouseButton(1) && IsUsingBuilder)
        {
            onIsBuilding?.Invoke(false, currentResourceToBuild);

            if (currentResourceToBuild != null) HighlightBuildings(false, currentResourceToBuild.GetResourceType());
            currentResourceToBuild = null;
        }

        if (IsUsingBuilder && currentResourceToBuild != null)
        {
            if (activated) HighlightBuildings(true, currentResourceToBuild.GetResourceType());

            resourceManagerVisuals.Highlight(ref highlightObjects);
        }

        if(Input.GetMouseButtonDown(0)  && IsUsingBuilder && currentResourceToBuild != null
            && currentResourceToBuild.GetResourceType() == TileObjects.TileType.Road)
        {
            gridHex.GetXY(UtilsClass.GetMouseWorldPosition(), out int x, out int y);
            GridObject gridObject = gridHex.GetGridObject(gridHex.GetWorldPosition(x, y));
            TileObjects chosenTile = gridObject.GetTileObject();

            if (!chosenTile.GetIsFOW()) { BuildRoad(); }
        }
        else if (Input.GetMouseButtonDown(0) && IsUsingBuilder && currentResourceToBuild != null && !EventSystem.current.IsPointerOverGameObject())
        {
            gridHex.GetXY(UtilsClass.GetMouseWorldPosition(), out int x, out int y);
            GridObject gridObject = gridHex.GetGridObject(gridHex.GetWorldPosition(x, y));
            if (gridObject == null) return;
            TileObjects chosenTile = gridObject.GetTileObject();

            canBuild = CheckIfCanBuild(gridObject, chosenTile);

            if (canBuild)
            {
                if(resourceTypeList.Contains(currentResourceToBuild.GetResourceType()) && chosenTile.GetResourceType() != currentResourceToBuild.GetResourceType())
                {
                    SoundSystem.instance.PlaySound(SoundSystem.Sound.Error);
                }
                
                if (chosenTile.GetResourceType() == currentResourceToBuild.GetResourceType() &&
                    currentResourceToBuild.GetBuildingPrice() <= resourceManager.GetCurrentWood()
                    && resourceTypeList.Contains(currentResourceToBuild.GetResourceType()))
                {
                    CheckIfUpgrade(chosenTile, x, y);
                }
                else if (!resourceTypeList.Contains(currentResourceToBuild.GetResourceType()) && 
                        (chosenTile.GetResourceType() == currentResourceToBuild.GetResourceType() || !chosenTile.GetIsOccupied()) &&
                        currentResourceToBuild.GetBuildingPrice() <= resourceManager.GetCurrentWood())
                {
                    CheckIfUpgrade(chosenTile, x, y);
                }
                
            }
        }
    }

    void OnMapAwake()
    {
        settlementTile = mainManager.GetSettlement().GetComponent<TileObjects>();
        enemyManager = mainManager.GetEnemyManager();

        mainManager.OnMapAwake -= OnMapAwake;
    }

    public void ActivateBuilder()
    {
        if (!IsUsingBuilder && !IsBattling && canActivateBuilder)
        {
            IsUsingBuilder = true;
            visualManager.BuilderActive();
            mainManager.SetCanOpenMenu(false);
        }
        else if (IsUsingBuilder && !IsBattling)
        {
            DeactivateBuilder(); 

            resourceManagerVisuals.DeactivateBuilderUI();
        }
    }
    public void DeactivateBuilder()
    {
        IsUsingBuilder = false;
        onIsBuilding?.Invoke(false, currentResourceToBuild);
        currentResourceToBuild = null;
    }

    bool CheckIfCanBuild(GridObject gridObject, TileObjects chosenTile)
    {
        bool canBuild = true;

        if(chosenTile.GetResourceType() == TileObjects.TileType.Road ||
            chosenTile.GetResourceType() == TileObjects.TileType.Other
            || chosenTile.GetIsFOW())
        {
            return false;
        }

        if (gridObject == null || !gridObject.CanBuild())
        {
            canBuild =  false;
        }
        if (gridObject != null && chosenTile.GetTier() < resourceManager.GetMaxTier())
        {
            canBuild = true;
        }
        else if (chosenTile.GetTier() >= resourceManager.GetMaxTier())
        {
            resourceManager.SetCurrentTileObject(chosenTile);
            resourceManager.SelectUpgradeErrorMessage(ResourceManager.UpgradeType.Tier);
            return false;
        }

        if (currentResourceToBuild.GetResourceType() == TileObjects.TileType.Warehouse && warehouseTile != null
              && chosenTile.GetResourceType() != currentResourceToBuild.GetResourceType())
        {
            canBuild = false;
        }
        else if (currentResourceToBuild.GetResourceType() == TileObjects.TileType.MagicStore && magicShopTile != null
              && chosenTile.GetResourceType() != currentResourceToBuild.GetResourceType())
        {
            canBuild = false;
        }

        if (currentResourceToBuild.GetResourceType() == TileObjects.TileType.House &&
            chosenTile.GetResourceType() == currentResourceToBuild.GetResourceType())
        {
            canBuild = false;
        }
        return canBuild;
    }
    void CheckIfUpgrade(TileObjects chosenTile, int x, int y)
    {
        if (chosenTile.GetTileTypeSO() == currentResourceToBuild.GetTileTypeSO() &&
            chosenTile.GetResourceType() != TileObjects.TileType.Blacksmith)
        {
            if (chosenTile.GetResourceType() == TileObjects.TileType.Castle && enemyManager.GetIsAttackingSettlement()) return;
            ResourceManager.UpgradeType type = (chosenTile.GetResourceType() == TileObjects.TileType.Warehouse)
                ? ResourceManager.UpgradeType.WarehouseTier : ResourceManager.UpgradeType.Tier;

            resourceManager.SetCurrentTileObject(chosenTile);
            resourceManager.PressUpgradeButton(type);

            if(!resourceTypeList.Contains(chosenTile.GetResourceType()) && chosenTile.GetTier() >= resourceManager.GetMaxTier())
            {
                resourceManagerVisuals.DisableBuildingPossibility(chosenTile.GetResourceType());
            }

            //currentResourceToBuild = null;
        }
        else Build(chosenTile, x, y);
    }
    void Build(TileObjects chosenTile, int x, int y)
    {
        resourceManager.SetCurrentWood(-currentResourceToBuild.GetBuildingPrice());
        OnBuilding?.Invoke();

        resourceManager.RefreshAfterBuilding();
        SoundSystem.instance.PlaySound(SoundSystem.Sound.Build);

        chosenTile.ChangeFullTile(currentResourceToBuild);
        gridHex.GetGridObject(x, y).SetIsOccupied(true);
        resourceManager.AddToResourceList(chosenTile);


        SetCanDrag(false);
        HighlightBuildings(false, currentResourceToBuild.GetResourceType());

        //currentResourceToBuild = null;
        StartCoroutine(WaitForNull());

        if (chosenTile.GetResourceType() == TileObjects.TileType.Warehouse)
        {
            resourceManager.AddWarehouse();
            warehouseTile = chosenTile;
        }
        else if(chosenTile.GetResourceType() == TileObjects.TileType.MagicStore)
        {

            SetMagicShop(chosenTile);
        }
        else if(chosenTile.GetResourceType() == TileObjects.TileType.Blacksmith)
        {
            blacksmithTile = chosenTile;
            resourceManagerVisuals.DisableBuildingPossibility(chosenTile.GetResourceType());
        }
    }
    void BuildRoad()
    {
        TileObjects tileObject = gridHex.GetGridObject(UtilsClass.GetMouseWorldPosition())?.GetTileObject();

        if (tileObject != null && (!tileObject.GetIsOccupied() || (tileObject.GetIsOccupied() && tileObject.GetResourceType() != TileObjects.TileType.Other))
            && !tileObject.GetIsRoad() && resourceManager.GetCurrentGold() >= 5)
        {
            if (resourceManager.GetCurrentGold() >= 5)
            {
                SoundSystem.instance.PlaySound(SoundSystem.Sound.Build);

                resourceManager.SetCurrentGold(-5);
                resourceManager.RefreshAfterBuilding();
                gridHex.GetGridObject(UtilsClass.GetMouseWorldPosition()).GetTileObject().SetIsRoad();
                tileObject.GetComponentInChildren<RoadBuilder>().PlaceRoad();

                onRoadBuilt?.Invoke();
            }
            else
            {
                SoundSystem.instance.PlaySound(SoundSystem.Sound.Error);
            }
        }
    }
    public IEnumerator WaitForNull()
    {
        yield return null;

        currentResourceToBuild = null;
    }

    public void HighlightBuildings(bool option, TileObjects.TileType type)
    {
        activated = false;
        List<TileObjects> currentList = new List<TileObjects>();
        Transform currentTransform = this.transform;
        CheckHighlightType(type, ref currentList, ref currentTransform);

        if (resourceTypeList.Contains(type) && type != settlementTile.GetResourceType())
        {

            for (int i = 0; i < currentList.Count; i++)
            {
                if (currentList[i].GetTier() < resourceManager.GetMaxTier())
                {
                    highlightObjects[i].SetActive(option);
                    highlightObjects[i].transform.position = currentList[i].transform.position;
                }
            }
        }
        else if(currentTransform != this.transform)
        {
            highlightObjects[0].SetActive(option);
            highlightObjects[0].transform.position = currentTransform.position;
        }
        else
        {
            for (int i = 0; i < currentList.Count; i++)
            {
                highlightObjects[i].SetActive(false);
            }
        }

        if(option == false)
        {
            for (int i = 0; i < 4; i++)
            {
                highlightObjects[i].SetActive(false);
            }
        }
    }
    void CheckHighlightType(TileObjects.TileType type, ref List<TileObjects> currentList, ref Transform currentTransform)
    {
        if (type == TileObjects.TileType.Farm) currentList = farmRuinPositions;
        else if (type == TileObjects.TileType.Lumberyard) currentList = lumberyardRuinPositions;
        else if (type == TileObjects.TileType.Goldmine) currentList = goldmineRuinPositions;
        else if (type == TileObjects.TileType.Village || type == TileObjects.TileType.Road) currentList = villagePositions;

        currentTransform = this.transform;

        if (magicShopTile != null && type == magicShopTile.GetResourceType()) currentTransform = magicShopTile.transform;
        else if (warehouseTile != null && type == warehouseTile.GetResourceType()) currentTransform = warehouseTile.transform;
        else if (settlementTile != null && type == settlementTile.GetResourceType()) currentTransform = settlementTile.transform;
    }

    public void ChangeBuildingType(TileObjects tileObject)
    {
        activated = true;
        tileObject = ChangeToUpgradeType(tileObject);

        if((IsOnlyUpgradable(tileObject) && HasEnoughCurrency(tileObject) && CanUpgradeBuilding(tileObject)) || 
            (!IsOnlyUpgradable(tileObject) && tileObject.GetBuildingPrice() <= resourceManager.GetCurrentWood()) ||
            ((tileObject.GetResourceType() == TileObjects.TileType.Road) && resourceManager.GetCurrentGold() >= 5))
        {
            TileObjects.TileType tileType = tileObject.GetResourceType();

            bool drag = true;

            if (tileType == TileObjects.TileType.Farm) { currentResourceToBuild = resourceBuildingsList[0]; }
            else if (tileType == TileObjects.TileType.Lumberyard) { currentResourceToBuild = resourceBuildingsList[1]; }
            else if (tileType == TileObjects.TileType.Goldmine) { currentResourceToBuild = resourceBuildingsList[2]; }
            else if (tileType == TileObjects.TileType.House && IsUnderMaxBuildings(tileObject)) { currentResourceToBuild = resourceBuildingsList[3]; }
            else if (tileType == TileObjects.TileType.Warehouse && (warehouseTile != null
                && warehouseTile.GetTier() < resourceManager.GetMaxTier() || warehouseTile == null))
            { currentResourceToBuild = resourceBuildingsList[4]; }

            else if (tileType == TileObjects.TileType.MagicStore && (magicShopTile != null
                && magicShopTile.GetTier() < resourceManager.GetMaxTier() || magicShopTile == null))
            { currentResourceToBuild = resourceBuildingsList[5]; }

            else if (tileType == TileObjects.TileType.Blacksmith && blacksmithTile == null)
            { currentResourceToBuild = resourceBuildingsList[6]; }
            else if (tileType == TileObjects.TileType.Castle) { currentResourceToBuild = resourceBuildingsList[7]; }
            else if (tileType == TileObjects.TileType.Village) { currentResourceToBuild = resourceBuildingsList[8]; }
            else if (tileType == TileObjects.TileType.Road) { currentResourceToBuild = resourceBuildingsList[9]; }
            else
            {
                drag = false;
                currentResourceToBuild = null;
                //resourceManagerVisuals.ShowNotEnoughRequirementsText($"Already built maximum number of {tileObject.GetName()}s!", 0, "", "");
            }

            if (currentResourceToBuild != null) resourceManager.GetTierUpgradePrice(currentResourceToBuild);

            if (drag) onIsBuilding?.Invoke(true, currentResourceToBuild);
        }
        else
        {
            ShowErrorText(tileObject);
        }
    }
    public bool IsUnderMaxBuildings(TileObjects tileObjects)
    {        
        if (resourceManager.GetCurrentHouses() < tileObjects.GetMaxBuildingNum()) return true;
        else return false;
    }
    void ShowErrorText(TileObjects tileObject)
    {
        if ((tileObject.GetResourceType() == TileObjects.TileType.Blacksmith && blacksmithTile != null) || tileObject.GetTier() >= resourceManager.GetMaxTier() ||
            (tileObject.GetResourceType() == TileObjects.TileType.House && resourceManager.currentHouses >= tileObject.GetMaxBuildingNum())) return;
        //resourceManagerVisuals.ShowNotEnoughRequirementsText($"Already built maximum number of {tileObject.GetName()}s", 0, "", "");

        else if (tileObject.GetResourceType() != TileObjects.TileType.Road && !IsOnlyUpgradable(tileObject))
            resourceManagerVisuals.ShowErrorText("", tileObject.GetBuildingPrice() - resourceManager.GetCurrentWood(), "wood", tileObject.GetName());
        else if (tileObject.GetResourceType() != TileObjects.TileType.Road)
            resourceManagerVisuals.ShowErrorText("", resourceManager.GetTierUpgradePrice(tileObject) - resourceManager.GetCurrentWood(), "wood", tileObject.GetName());

        else resourceManagerVisuals.ShowErrorText("", 5 - resourceManager.GetCurrentGold(), "gold", tileObject.GetName());

    }

    TileObjects ChangeToUpgradeType(TileObjects tileObject)
    {
        if (tileObject.GetResourceType() == TileObjects.TileType.Castle) return settlementTile;
        else if (tileObject.GetResourceType() == TileObjects.TileType.MagicStore && magicShopTile != null) return magicShopTile;
        
        else if (tileObject.GetResourceType() == TileObjects.TileType.Warehouse && warehouseTile != null) return warehouseTile;

        return tileObject;
    }
    bool IsOnlyUpgradable(TileObjects tileObject)
    {
        return (tileObject.GetResourceType() == TileObjects.TileType.MagicStore || tileObject.GetResourceType() == TileObjects.TileType.Blacksmith || tileObject.GetResourceType() == TileObjects.TileType.Castle);
    }
    bool HasEnoughCurrency(TileObjects tileObject)
    {
        return resourceManager.GetTierUpgradePrice(tileObject) <= resourceManager.GetCurrentWood();
    }
    bool CanUpgradeBuilding(TileObjects tileObject)
    {
        if(tileObject.GetResourceType() == TileObjects.TileType.MagicStore || tileObject.GetResourceType() == TileObjects.TileType.Blacksmith || tileObject.GetResourceType() == TileObjects.TileType.Castle)
        {
            if (tileObject.GetResourceType() == TileObjects.TileType.Castle && tileObject.GetTier() > 2) return false;
            
            if(tileObject.GetTier() >= resourceManager.GetMaxTier()) return false;
        }

        return true;
    }
    

    public void ShowBuildingRequirements(TileObjects tileObject)
    {
        if(tileObject.GetResourceType() == TileObjects.TileType.Warehouse && warehouseTile != null && warehouseTile.GetTier() < resourceManager.GetMaxTier())
        {
            resourceManager.SetCurrentTileObject(warehouseTile);

            resourceManagerVisuals.SetTierPrice(resourceManager.GetTierUpgradePrice(tileObject));
            resourceManagerVisuals.ShowBuildingRequirements(ResourceManagerVisuals.TextType.tier);

            return;
        }
        else if (tileObject.GetResourceType() == TileObjects.TileType.Warehouse && warehouseTile == null)
        {
            resourceManagerVisuals.ShowBuildingRequirements(tileObject);

            return;
        }

        if (tileObject.GetResourceType() == TileObjects.TileType.MagicStore && magicShopTile != null && magicShopTile.GetTier() < resourceManager.GetMaxTier())
        {
            resourceManager.SetCurrentTileObject(magicShopTile);

            resourceManagerVisuals.SetTierPrice(resourceManager.GetTierUpgradePrice(tileObject));
            resourceManagerVisuals.ShowBuildingRequirements(ResourceManagerVisuals.TextType.tier);

            return;
        }
        else if (tileObject.GetResourceType() == TileObjects.TileType.MagicStore && magicShopTile == null)
        {
            resourceManagerVisuals.ShowBuildingRequirements(tileObject);

            return;
        }
    }

    public TileObjects GetCurrentResourceToBuild()
    {
        return currentResourceToBuild;
    }
    public void SetCurrentResourceToBuild()
    {
        StartCoroutine(WaitForNull());
    }
    public void SetCanDrag(bool option)
    {
        onIsBuilding?.Invoke(option, currentResourceToBuild);
    }
    public void SetIsBuilding(bool option)
    {
        IsUsingBuilder = option;

        if (!IsUsingBuilder && currentResourceToBuild != null) HighlightBuildings(false, currentResourceToBuild.GetResourceType());
    }
    public void SetIsBattling(bool option)
    {
        IsBattling = option;
    }
    public void SetCanActivateBuilder(bool option)
    {
        canActivateBuilder = option;
    }
    public void SetSettlementTile(TileObjects tileObject)
    {
        settlementTile = tileObject;
    }
    public TileObjects GetWarehouseTile()
    {
        return warehouseTile;
    }
    public void SetWarehouseTile(TileObjects tileObject)
    {
        warehouseTile = tileObject;
    }
    public TileObjects GetMagicShop()
    {
        return magicShopTile;
    }
    public void SetMagicShop(TileObjects tileObject)
    {
        magicShopTile = tileObject;
        // move 
        magicShop.SetMagicShopTile(tileObject);
    }
    public TileObjects GetBlacksmith()
    {
        return blacksmithTile;
    }
    public void SetBlacksmith(TileObjects tileObject)
    {
        blacksmithTile = tileObject;
    }
    public TileObjects GetSettlement()
    {
        return settlementTile;
    }
    public void SetSettlement(TileObjects tileObject)
    {
        settlementTile = tileObject;
    }
    public List<TileObjects.TileType> GetResourceTypeList()
    {
        return resourceTypeList;
    }

    public void AddToRuinPositions(TileObjects.TileType type, TileObjects tileObject)
    {
        if (type != TileObjects.TileType.Village) OnResourceLocationAdded?.Invoke(tileObject.transform.position);

        switch (type)
        {
            case TileObjects.TileType.Farm:
            {
                farmRuinPositions.Add(tileObject);
                break;
            }
            case TileObjects.TileType.Lumberyard:
            {
                lumberyardRuinPositions.Add(tileObject);
                break;
            }
            case TileObjects.TileType.Goldmine:
            {
                goldmineRuinPositions.Add(tileObject);
                break;
            }
            case TileObjects.TileType.Village:
            {
                villagePositions.Add(tileObject);
                break;
            }
        }
    }
}
