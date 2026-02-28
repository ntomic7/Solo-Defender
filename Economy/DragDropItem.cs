using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropItem : MonoBehaviour
{
    [SerializeField] ResourceBuilder resourceBuilder;
    [SerializeField] Inventory inventory;

    bool canBuild = false;

    RectTransform rectTransform;
    Vector3 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        resourceBuilder.onIsBuilding += SetCanBuild;
        inventory.onItemChange += SetCanBuild;

        originalPosition = rectTransform.position;
    }
    void Update()
    {
        if(canBuild)
        {
            rectTransform.position = Input.mousePosition;

            if (Input.GetMouseButtonDown(1))
            {
                canBuild = false;
                rectTransform.position = originalPosition;
            }
        }


    }

    void SetCanBuild(bool option, TileObjects tileObject)
    {
        if(tileObject != null && this.name == tileObject.GetName()) { canBuild = option; }
        else { canBuild = false; }

        if (canBuild == false) rectTransform.position = originalPosition;
    }
    void SetCanBuild(bool option, GameObject go)
    {
        if(go == this.gameObject && option == true) canBuild = option;
        else canBuild = false;

        if (!canBuild) rectTransform.position = originalPosition;
    }


}
