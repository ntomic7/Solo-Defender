using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManagerVisuals : MonoBehaviour
{
    [SerializeField] MAINmanager mainManager;
    ResourceManager resourceManager;
    ResourceBuilder resourceBuilder;
    [SerializeField] VisualManager visualManager;

    [SerializeField] Canvas canvas;

    PlayerLevelManager PLM;

    // resource UI

    [SerializeField] List<TextMeshProUGUI> resourceTexts;
    [SerializeField] List<GameObject> WarningIcons;
    //

    [SerializeField] GameObject exitButton;
    [SerializeField] List<GameObject> builderUIList;

    Tutorial tutorial;
    [SerializeField] GameObject tutorialGO;

    [SerializeField] GameObject buildingRequirementsPanel;
    TextMeshProUGUI buildingRequirementsText;

    // upgrade UI
    [SerializeField] List<GameObject> upgradeUIList;

    Settlement settlement;
    TileObjects settlementTileObject;
    [SerializeField] GameObject settlementRepair;
    bool settlementDamaged;
    int settlementRepairPrice = 0;

    // shopUIs
    [SerializeField] GameObject magicShopGO;
    [SerializeField] GameObject blacksmithGO;
    [SerializeField] GameObject inventoryUI;
    [SerializeField] GameObject inventoryBackgrounds;

    [SerializeField] List<GameObject> pageLabels;

    // inventoryUIs
    [SerializeField] GameObject attributesPanelUI;
    [SerializeField] GameObject inventoryPanelUI;

    [SerializeField] GameObject inventoryWarning;

    // attackChangeUI
    [SerializeField] List<RectTransform> attackButtonTransforms;
    [SerializeField] List<Vector3> attackButtonChangeLocations;
    List<Vector3> attackButtonOriginalLocations = new List<Vector3>();

    // error text
    [SerializeField] GameObject errorTextGO;
    List<GameObject> errorTextList = new List<GameObject>();

    [SerializeField] List<Image> builderImageList;

    public bool[] maxxedOutBuildings = new bool[5];
    [SerializeField] List<Image> maxBuildingsList;

    bool IsBattling;
    bool IsCastleUpgrade;
    int numOfAttackButtons = 0;
    int tierPrice;

    float currentAlpha = 1;
    float desiredAlpha = 0;
    float maxAlpha = 1;

    public Action<bool> OnBuilderChange;

    bool IsSettlementDamaged;

    void Start()
    {
        mainManager.OnMapAwake += OnMapAwake;

        resourceManager = GetComponent<ResourceManager>();
        tutorial = mainManager.GetTutorial();

        PLM = mainManager.GetPlayerLevelManager();

        buildingRequirementsText = buildingRequirementsPanel.GetComponentInChildren<TextMeshProUGUI>();

        foreach(RectTransform transform in attackButtonTransforms) attackButtonOriginalLocations.Add(transform.anchoredPosition);

        maxxedOutBuildings = new bool[5];
    }
    void OnMapAwake()
    {
        settlement = mainManager.GetSettlementScript();
        resourceBuilder = mainManager.GetResourceBuilder();
        settlementTileObject = resourceBuilder.GetSettlement();

        settlement.OnSettlementDamage += SetRepairSettlement;
    }
    public void SetRepairSettlement(bool option, bool IsAttackingSettlement, int repairPrice)
    {
        if(option)
        {
            settlementDamaged = true;
            settlementRepairPrice = repairPrice;
            settlementRepair.SetActive(true);
        }
        else
        {
            settlementRepair.SetActive(false);
            settlementDamaged = false;
        }
    }
    public void SetRepairVillage(bool option, int repairPrice)
    {
        if (option)
        {
            settlementRepairPrice = repairPrice;
            settlementRepair.GetComponentInChildren<TextMeshProUGUI>().text = $"Rebuild ({settlementRepairPrice})";
        }
        else
        {
            settlementRepair.SetActive(false);
        }
    }

    // resource UI
    public void RefreshResourceVisuals(int wood, int gold, int food, int people, int maxWood, int maxGold, int maxFood)
    {
        resourceTexts[GetResourceIcon(Resource.Wood)].text = $"{wood} / {maxWood}";
        resourceTexts[GetResourceIcon(Resource.Gold)].text = $"{gold} / {maxGold}";
        resourceTexts[GetResourceIcon(Resource.Food)].text = $"{food} / {maxFood}";
        resourceTexts[GetResourceIcon(Resource.Workers)].text = people.ToString();
    }

    // inventory UI

    public void ActivateInventoryUI()
    {
        visualManager.InventoryActive();

        inventoryUI.SetActive(true);
        inventoryBackgrounds.SetActive(true);

        if (PLM.hasPoints) inventoryWarning.SetActive(true);
        if (tutorial.IsInTutorial) tutorialGO.SetActive(false);

        blacksmithGO.SetActive(true);
        blacksmithGO.transform.GetChild(0).gameObject.SetActive(false);


        SetAttackButtonsActive(true);
        ActivateInventoryPage();
    }
    public void DeactivateInventoryUI()
    {
        visualManager.ShowThirdLabel();
        visualManager.SetIsInInventory(false);
        
        inventoryUI.SetActive(false);
        inventoryBackgrounds.SetActive(false);
        inventoryWarning.SetActive(false);

        if (tutorial.IsInTutorial) tutorialGO.SetActive(true);

        blacksmithGO.SetActive(false);
        blacksmithGO.transform.GetChild(0).gameObject.SetActive(true);

        SetAttackButtonsActive(false);
    }
    

    // different upgrade / builder UIs
    public void ActivateResourceUpgradeUI(int tier, int workers, string tileName, Sprite tileSprite, TileObjects currentTileObject)
    {
        ActivateUpgradeUI(tier, workers, tileName, tileSprite, currentTileObject);
    }
    public void ActivateUpgradeUI(int tier, int workers, string tileName, Sprite tileSprite, TileObjects currentTileObject)
    {
        mainManager.SetCanOpenMenu(false);

        upgradeUIList[0].SetActive(true);
        upgradeUIList[1].SetActive(true);
        exitButton.SetActive(true);

        if (settlementDamaged && currentTileObject.GetResourceType() == TileObjects.TileType.Castle)
        {
            settlementRepair.SetActive(true);
            settlementRepair.GetComponentInChildren<TextMeshProUGUI>().text = $"Rebuild ({settlementRepairPrice})";
        }
        else settlementRepair.SetActive(false);

            string typeOfWorker = (currentTileObject.GetResourceType() == TileObjects.TileType.Castle) ? "Guards" : "Workers";

        upgradeUIList[3].GetComponent<TextMeshProUGUI>().text = $"{typeOfWorker}: {workers}";
        upgradeUIList[4].GetComponent<TextMeshProUGUI>().text = $"{tileName} ({tier + 1})";
        upgradeUIList[5].GetComponent<Image>().sprite = tileSprite;

        if(currentTileObject.GetResourceType() == TileObjects.TileType.Village)
        {
            SetRepairVillage(true, currentTileObject.GetComponent<VillageResources>().GetMaxHealth()
                - currentTileObject.GetComponent<VillageResources>().GetCurrentHealth());

            upgradeUIList[3].SetActive(false);
            upgradeUIList[6].SetActive(false);

            settlementRepair.SetActive(true);
        }
    }
    public void DeactivateUpgradeUI()
    {
        upgradeUIList[0].SetActive(false);
        upgradeUIList[1].SetActive(false);
        upgradeUIList[6].SetActive(true);

        exitButton.SetActive(false);

        settlementRepair.SetActive(false);

        IsCastleUpgrade = false;
    }

    public void ActivateBuilderUI()
    {
        builderUIList[0].SetActive(true);
        builderUIList[1].SetActive(true);
        exitButton.SetActive(true);

        OnBuilderChange?.Invoke(true);
    }
    public void DeactivateBuilderUI()
    {
        builderUIList[0].SetActive(false);
        builderUIList[1].SetActive(false);
        exitButton.SetActive(false);

        OnBuilderChange?.Invoke(false);
    }
    public void Highlight(ref List<GameObject> highlightObjects)
    {
        foreach (GameObject go in highlightObjects)
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, desiredAlpha, .25f * Time.deltaTime);
            go.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, currentAlpha);
            if (currentAlpha == 0) desiredAlpha = 1;
            else if (currentAlpha == 1) desiredAlpha = 0;
        }
    }


    // shop UIs
    public void ActivateMagicShopUI()
    {
        magicShopGO.SetActive(true);
        inventoryBackgrounds.SetActive(true);

        if (tutorial.IsInTutorial) tutorialGO.SetActive(false);

        mainManager.SetMoveCamera(false);
        visualManager.ActivateFirstPage();
        visualManager.OnOpenMagicShop();
    }
    public void DeactivateMagicShopUI()
    {
        magicShopGO.SetActive(false);
        inventoryBackgrounds.SetActive(false);

        if (tutorial.IsInTutorial) tutorialGO.SetActive(true);

        mainManager.SetMoveCamera(true);
    }

    public void ActivateBlacksmithUI()
    {
        inventoryUI.SetActive(true);
        inventoryBackgrounds.SetActive(true);
        blacksmithGO.SetActive(true);

        inventoryUI.transform.GetChild(0).gameObject.SetActive(false);
        inventoryUI.transform.GetChild(1).gameObject.SetActive(false);

        if (tutorial.IsInTutorial) tutorialGO.SetActive(false);

        foreach (GameObject go in pageLabels) go.SetActive(false);

        mainManager.SetMoveCamera(false);
    }
    public void DeactivateBlacksmithUI()
    {
        inventoryUI.SetActive(false);
        inventoryBackgrounds.SetActive(false);
        blacksmithGO.SetActive(false);
        blacksmithGO.transform.GetChild(2).gameObject.SetActive(false);

        if (tutorial.IsInTutorial) tutorialGO.SetActive(true);

        foreach (GameObject go in pageLabels) go.SetActive(true);
        mainManager.SetMoveCamera(true);
    }

    public void ActivateInventoryPage()
    {
        inventoryPanelUI.SetActive(true);
        attributesPanelUI.SetActive(false);
    }
    public void ActivateAttributePage()
    {
        inventoryPanelUI.SetActive(false);
        attributesPanelUI.SetActive(true);
    }

    // attackChangeUIs
    public void SetAttackButtonsActive(bool option)
    {
        List<Vector3> currentPositions;
        float scale;

        currentPositions = (option) ? attackButtonChangeLocations : attackButtonOriginalLocations;
        scale = (option) ? 1.64f : 1.9f;

        for (int i = 0; i < numOfAttackButtons; i++)
        {
            attackButtonTransforms[i].gameObject.SetActive(option);
            attackButtonTransforms[i].anchoredPosition = currentPositions[i];
            attackButtonTransforms[i].localScale = new Vector3(scale, scale, scale);
        }
    }
    public void ShowAttackButtons(bool option)
    {
        for (int i = 0; i < numOfAttackButtons; i++)
        {
            attackButtonTransforms[i].gameObject.SetActive(option);
        }
    }

    // error texts
    public void ShowErrorText(string sentence, int amountMissing, string resource, string tile)
    {
        GameObject newErrorTextGO = Instantiate(errorTextGO, canvas.transform);

        MoveErrorTexts();
        errorTextList.Add(newErrorTextGO);

        newErrorTextGO.SetActive(true);

        if (sentence == "") newErrorTextGO.GetComponent<TextMeshProUGUI>().text = $"Additional {amountMissing} {resource} is required to build {tile}";
        else newErrorTextGO.GetComponent<TextMeshProUGUI>().text = sentence;

        StartCoroutine(HideNotEnoughRequirementsText(newErrorTextGO));
    }
    IEnumerator HideNotEnoughRequirementsText(GameObject textGO)
    {
        yield return new WaitForSeconds(3f);

        errorTextList.Remove(textGO);
        Destroy(textGO);
    }
    void MoveErrorTexts()
    {
        if (errorTextList.Count > 0)
        {
            foreach (GameObject errorTextGO in errorTextList)
            {
                errorTextGO.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, -17);
            }
        }
    }

    public void DisableBuildingPossibility(TileObjects.TileType tileType)
    {
        int one = 0, two = 0;

        ChooseBuilding(tileType, out one, out two);

        maxBuildingsList[one].color = new Color32(229, 79, 79, 255);
        maxBuildingsList[two].color = new Color32(229, 79, 79, 255);
    }
    void ChooseBuilding(TileObjects.TileType tileType, out int one, out int two)
    {
        if (tileType == TileObjects.TileType.House)
        {
            one = 0; two = 1;
            maxxedOutBuildings[0] = true;
        }
        else if (tileType == TileObjects.TileType.Warehouse)
        {
            one = 2; two = 3;
            maxxedOutBuildings[1] = true;
        }
        else if (tileType == TileObjects.TileType.MagicStore)
        {
            one = 4; two = 5;
            maxxedOutBuildings[2] = true;
        }
        else if (tileType == TileObjects.TileType.Blacksmith)
        {
            one = 6; two = 7;
            maxxedOutBuildings[3] = true;

        }
        else if (tileType == TileObjects.TileType.Castle)
        {
            one = 8; two = 9;
            maxxedOutBuildings[4] = true;
        }
        else
        {
            one = 0; two = 1;
        }
    }

    // building requirement texts
    public void ShowBuildingRequirements(TileObjects tileObject)
    {
        buildingRequirementsPanel.SetActive(true);

        if(tileObject.GetTier() >= 3 || (tileObject.GetResourceType() == TileObjects.TileType.House && resourceManager.GetCurrentHouses() >= tileObject.GetMaxBuildingNum())
            || (tileObject.GetResourceType() == TileObjects.TileType.Blacksmith && resourceBuilder.GetBlacksmith() != null))
        {
            ShowDisabledOption(tileObject);
            return;
        } 

        string nameOfBuilding = tileObject.GetName();
        int price = tileObject.GetBuildingPrice();
        if(tileObject.GetResourceType() != TileObjects.TileType.Village || tileObject.GetResourceType() != TileObjects.TileType.Castle)
            buildingRequirementsText.text = $"{nameOfBuilding}\r\nCost is {price} wood\r\n{tileObject.GetDescription()}";
        else
            buildingRequirementsText.text = $"{nameOfBuilding}\r\n{tileObject.GetDescription()}";

    }
    public void ShowBuildingRequirements(TextType textType)
    {
        TileObjects tileObject = resourceManager.GetCurrentTileObject();
        buildingRequirementsPanel.SetActive(true);        

        if (tileObject.GetTier() >= 3 || (tileObject.GetResourceType() == TileObjects.TileType.House && resourceManager.GetCurrentHouses() >= tileObject.GetMaxBuildingNum())
            || (tileObject.GetResourceType() == TileObjects.TileType.Blacksmith && resourceBuilder.GetBlacksmith() != null))
        {
            ShowDisabledOption(tileObject);
            return;
        }

        string name = tileObject.GetName();
        int num = (textType == TextType.tier) ? tileObject.GetTier() : tileObject.GetCurrentWorkers();
        string resource = (textType == TextType.tier) ? "wood" : "worker";
        string type = (IsCastleUpgrade) ? "number of guards" : "number of workers";

        if (textType == TextType.tier) buildingRequirementsText.text = $"Current tier of {name} is {num + 1}.\n\rCost to upgrade is {tierPrice} {resource}.";
        else if (textType == TextType.workers) buildingRequirementsText.text = $"Current {type} in {name} is {num}";
    
    }
    public void ShowResourceUpgradeTier(TileObjects tileObject)
    {
        buildingRequirementsPanel.SetActive(true);

        buildingRequirementsText.text = $"{tileObject.GetName()}\r\n{tileObject.GetDescription()}\n" +
            $"\r\nTier 1: {tileObject.GetBuildingPrice()}" +
            $"\tTier 2: {resourceManager.tierUpgradePrice[0]}" +
            $"\r\nTier 3: {resourceManager.tierUpgradePrice[1]}";

        if(tileObject.GetResourceType() != TileObjects.TileType.Village) buildingRequirementsText.text += $"\tTier 4: {resourceManager.tierUpgradePrice[2]}";
    }
    void ShowDisabledOption(TileObjects tileObjects)
    {
        if (tileObjects.GetResourceType() == TileObjects.TileType.House) buildingRequirementsText.text = "Built the maximum number of houses";

        else buildingRequirementsText.text = "Already at maximum tier";
    }
    public void HideBuildingRequirements()
    {
        buildingRequirementsPanel.SetActive(false);
    }

    public void ShowWorkers()
    {
        ShowBuildingRequirements(TextType.workers);
    }
    public void ShowTier(TileObjects tileObject)
    {

        if (tileObject.GetResourceType() == TileObjects.TileType.Castle) tileObject = settlementTileObject;
        else if (tileObject.GetResourceType() == TileObjects.TileType.MagicStore && resourceBuilder.GetMagicShop() != null) tileObject = resourceBuilder.GetMagicShop();
        else if(tileObject.GetResourceType() == TileObjects.TileType.MagicStore && resourceBuilder.GetMagicShop() == null)
        {
            ShowBuildingRequirements(tileObject);
            return;
        }
        else if (tileObject.GetResourceType() == TileObjects.TileType.Warehouse && resourceBuilder.GetWarehouseTile() != null) tileObject = resourceBuilder.GetWarehouseTile();
        else if (tileObject.GetResourceType() == TileObjects.TileType.Warehouse && resourceBuilder.GetWarehouseTile() == null)
        {
            ShowBuildingRequirements(tileObject);
            return;
        }

        resourceManager.SetCurrentTileObject(tileObject);
        SetTierPrice(resourceManager.GetTierUpgradePrice(tileObject));
        ShowBuildingRequirements(TextType.tier);
    }
    public void ShowTier()
    {
        ShowBuildingRequirements(TextType.tier);
    }
    public enum TextType
    {
        tier, 
        workers,
    }

    // setters

    public void SetIsBattling(bool option)
    {
        IsBattling = option;
        resourceManager.SetIsBattling(IsBattling);
    }
    public void SetNumOfAttacks(int num)
    {
        numOfAttackButtons = num;
    }
    public void SetIsCastle(bool option)
    {
        IsCastleUpgrade = option;
    }
    public void SetTierPrice(int amount)
    {
        tierPrice = amount;
    }


    // check resource type

    public void ActivateResourceWarning(Resource resource, bool option)
    {
        WarningIcons[GetResourceIcon(resource)].SetActive(option);
    }
    int GetResourceIcon(Resource resource) {

        if (resource == Resource.Wood) return 0;
        if (resource == Resource.Gold) return 1;
        if (resource == Resource.Food) return 2;
        if (resource == Resource.Workers) return 3;

        return 0;
    }

    public enum Resource
    {
        Wood, 
        Gold,
        Food,
        Workers
    }
}
