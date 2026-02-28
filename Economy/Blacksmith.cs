using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Blacksmith : MonoBehaviour
{
    [SerializeField] Attributes playerAttributes;
    Inventory playerInventory;
    [SerializeField] Animator animator;
    ResourceManager resourceManager;

    [SerializeField] GameObject TierUpgradeUI;

    [SerializeField] List<Sprite> swordSprites;
    [SerializeField] List<Sprite> armorSprites;

    int currentSwordTier = 0;
    int currentArmorTier = 0;

    List<Sprite> currentList;
    int currentTier;
    Image currentImage;

    List<int> tierPrices;
    List<Vector2Int> weaponDamage;
    List<Vector2Int> armorDamage;

    // item UIs
    [SerializeField] Image swordImage;
    [SerializeField] Image armorImage;
    [SerializeField] Image currentTierImage;
    [SerializeField] Image nextTierImage;

    [SerializeField] TextMeshProUGUI priceText;
    [SerializeField] TextMeshProUGUI tierText;

    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI descriptionText;

     void Start()
    {
        resourceManager = GetComponent<ResourceManager>();
        playerInventory = playerAttributes.GetComponent<Inventory>();

        tierPrices = new List<int>{ 100, 200, 300};
        weaponDamage = new List<Vector2Int> {new Vector2Int(5, 10), new Vector2Int(10, 15), new Vector2Int(15, 20), new Vector2Int(20, 25) };
        armorDamage = new List<Vector2Int> { new Vector2Int(5, 10), new Vector2Int(7, 12), new Vector2Int(10, 15), new Vector2Int(13, 17) };
    }

    public void ShowSwordTier()
    {
        ShowUpgrade(IsSword: true);
    }
    public void ShowArmorTier()
    {
        ShowUpgrade(IsSword: false);
    }
    public void ShowUpgrade(bool IsSword)
    {
        if (!playerInventory.IsInInventory)
        {
            TierUpgradeUI.SetActive(true);

            currentList = (IsSword) ? swordSprites : armorSprites;
            currentImage = (IsSword) ? swordImage : armorImage;
            currentTier = (IsSword) ? currentSwordTier : currentArmorTier;

            RefreshTierUI();
        }
    }
    public void UpgradeTier()
    {
        if (currentList != null &&  !(currentTier == tierPrices.Count) && tierPrices[currentTier] <= resourceManager.GetCurrentGold())
        {
            SoundSystem.instance.PlaySound(SoundSystem.Sound.Purchase);

            if (currentList == swordSprites)
            {
                playerAttributes.UpgradeEquipment(IsSword: true, weaponDamage[currentSwordTier]);
                currentSwordTier++;

                animator.SetTrigger("OnSwordUpgrade");
            }
            else
            {
                playerAttributes.UpgradeEquipment(IsSword: false, armorDamage[currentArmorTier]);
                currentArmorTier++;

                animator.SetTrigger("OnArmorUpgrade");
            }
            resourceManager.SetCurrentGold(-tierPrices[currentTier]);    
            currentTier++;


            RefreshTierUI();
            resourceManager.RefreshAfterBuilding();
        }
        else
        {
            SoundSystem.instance.PlaySound(SoundSystem.Sound.Error);
        }
    }

    public int GetTier()
    {
        return currentTier;
    }
    public int GetSwordTier()
    {
        return currentSwordTier;
    }
    public int GetArmorTier()
    {
        return currentArmorTier;
    }
    public void ReloadTier(int tier, int swordTier, int armorTier)
    {
        currentTier = tier;
        currentSwordTier = swordTier;
        currentArmorTier = armorTier;
    }

    public void ShowSwordDescription()
    {
        if (playerInventory.IsInInventory)
        {
            titleText.text = swordSprites[currentSwordTier].name;
            descriptionText.text = $"{weaponDamage[currentSwordTier].x} ~ {weaponDamage[currentSwordTier].y} damage";
        }
    }
    public void ShowArmorDescription()
    {
        if (playerInventory.IsInInventory)
        {
            titleText.text = armorSprites[currentArmorTier].name;
            descriptionText.text = $"{armorDamage[currentArmorTier].x} ~ {armorDamage[currentArmorTier].y} armor";

        }
    }
    public void HideDescription()
    {
        titleText.text = "";
        descriptionText.text = "";
    }
    void RefreshTierUI()
    {
        if (currentTier < tierPrices.Count)
        {
            tierText.text = $"Upgrade to Tier {currentTier + 2}?";
            priceText.text = tierPrices[currentTier].ToString();

            nextTierImage.sprite = currentList[currentTier + 1];
        }
        else
        {
            tierText.text = $"Max Tier";
            priceText.text = "";
            
            nextTierImage.sprite = currentList[currentTier];

        }
        currentTierImage.sprite = currentList[currentTier];
        currentImage.sprite = currentList[currentTier];
    }

}
