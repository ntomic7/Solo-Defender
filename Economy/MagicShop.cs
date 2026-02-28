using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MagicShop : MonoBehaviour
{
    [SerializeField] VisualManager visualManager;

    [SerializeField] List<SpellObjects> potionList;
    [SerializeField] List<SpellObjects> spellList;
    [SerializeField] List<SpellObjects> skillList;

    [SerializeField] GameObject shopImages;

    [SerializeField] Image chosenItemSprite;
    [SerializeField] TextMeshProUGUI chosenItemName;
    [SerializeField] TextMeshProUGUI chosenItemDescription;
    [SerializeField] TextMeshProUGUI chosenItemPrice;

    TileObjects magicShopTile;
    bool IsInInventory;

    int inventorySpaces = 12;
    SaleType currentPageType;
    SpellObjects currentSpellObject;

    public Action<SpellObjects> OnChooseItem;

    void Start()
    {
        visualManager.OnChangeMagicPage += ActivateShopPage;

        currentPageType = SaleType.Potion;
    }

    public void ChooseItem(int index)
    {
        if (index < potionList.Count && currentPageType == SaleType.Potion)
        {
            currentSpellObject = potionList[index];
            chosenItemSprite.rectTransform.sizeDelta = new Vector2(19, 24);
        }
        else if (index < spellList.Count && currentPageType == SaleType.Spell)
        {
            currentSpellObject = spellList[index];
            chosenItemSprite.rectTransform.sizeDelta = new Vector2(30, 30);
        }
        else if (index < skillList.Count && currentPageType == SaleType.Skill)
        {
            currentSpellObject = skillList[index];
            chosenItemSprite.rectTransform.sizeDelta = new Vector2(30, 30);
        }
        else
        {
            currentSpellObject = null;
        }

        if (currentSpellObject != null && shopImages.transform.GetChild(index).gameObject.activeSelf == true)
        {
            SetUpItemUI();
            OnChooseItem?.Invoke(currentSpellObject);
        }
    }
    void SetUpItemUI()
    {
        chosenItemSprite.gameObject.SetActive(true);

        chosenItemSprite.sprite = currentSpellObject.GetItemSprite();
        chosenItemName.text = currentSpellObject.GetName();
        chosenItemPrice.text = currentSpellObject.GetPrice().ToString();
        chosenItemDescription.text = currentSpellObject.GetDescription();
    }
    public void RemoveFromShopInventory(SpellObjects objectToRemove)
    {
        Debug.Log(objectToRemove.GetName());
        if (objectToRemove.IsSkill)
        {
            skillList.Remove(objectToRemove);
            ActivateShopPage(SaleType.Skill);
        }
        else
        {
            spellList.Remove(objectToRemove);
            ActivateShopPage(SaleType.Spell);
        }

        chosenItemSprite.gameObject.SetActive(false);
        chosenItemName.text = "";
        chosenItemPrice.text = "";
        chosenItemDescription.text = "";

        OnChooseItem?.Invoke(null);

    }

    public void ActivateShopPage(SaleType type)
    {
        if (!IsInInventory)
        {
            DeactivateShopPage(type);

            int itemAmount = (magicShopTile.GetTier() + 1) * 2;
            int currentTier = magicShopTile.GetTier();

            List<SpellObjects> currentList;
            float scale = 0;
            currentList = (type == SaleType.Potion) ? potionList : (type == SaleType.Spell) ? spellList : skillList;
            scale = (type == SaleType.Potion) ? 0.8f : 0.9f;
            currentPageType = type;

            for(int i = 0; i < currentList.Count; i++)
            {
                if (currentList[i].GetTier() <= currentTier)
                {
                    GameObject image = shopImages.transform.GetChild(i).gameObject;
                    image.SetActive(true);
                    image.transform.localScale = new Vector3(scale, scale, scale);
                    image.GetComponent<Image>().sprite = currentList[i].GetItemSprite();
                }
            }
        }
    }
    public void DeactivateShopPage(SaleType type)
    {
        int index = 0;

        while(index < inventorySpaces)
        {
            shopImages.transform.GetChild(index).gameObject.SetActive(false);

            index++;
        }
    }
    
    public void SetMagicShopTile(TileObjects tileObject)
    {
        magicShopTile = tileObject;
    }
    public void SetIsInInventory(bool option)
    {
        IsInInventory = option;
    }

    public List<SpellObjects> GetPotionList()
    {
        return potionList;
    }
    public List<SpellObjects> GetSkillList()
    {
        return skillList;
    }
    public List<SpellObjects> GetSpellList()
    {
        return spellList;
    }
    public enum SaleType
    {
        Potion,
        Spell,
        Skill
    }
}
