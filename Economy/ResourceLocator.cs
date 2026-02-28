using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLocator : MonoBehaviour
{
    [SerializeField] TileObjects tileObject;
    [SerializeField] TileObjects.TileType tileType;

    ResourceBuilder resourceBuilder;
    void Start()
    {
        resourceBuilder = GameObject.Find("ResourceManager").GetComponent<ResourceBuilder>();
        resourceBuilder.AddToRuinPositions(tileType, tileObject);

        Destroy(GetComponent<ResourceLocator>());
    }
}
