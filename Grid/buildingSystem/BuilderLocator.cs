using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderLocator : MonoBehaviour
{
    GridBuildingSystem gridBuildingSystem;

    private void Start()
    {
        gridBuildingSystem = FindAnyObjectByType<GridBuildingSystem>();
    }

    public GridBuildingSystem GetGridBuildingSystem()
    {
        return gridBuildingSystem;
    }
}
