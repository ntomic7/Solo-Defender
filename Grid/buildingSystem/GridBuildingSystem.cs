using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


[DefaultExecutionOrder(-100)]
public class GridBuildingSystem : MonoBehaviour
{
    GridHexXZ<GridObject> grid;
    PathFinding pathFinding;

    [SerializeField] GameObject castleObject;
    [SerializeField] List<TileTypesSO> tileTypesToSave;

    // setting up references
    [SerializeField] MAINmanager mainManager;
    [SerializeField] Settlement settlement;
    ResourceManager resourceManager;

    [SerializeField] FogOfWar FOW;
    [SerializeField] List<GameObject> statusBars;
    int villageIndex;

    // chunk type variables

    [SerializeField] List<GameObject> randomHexChunks;
    public List<GameObject> finishedHexChunkList = new List<GameObject>();

    [SerializeField] List<GameObject> enemyCampChunk;
    [SerializeField] List<GameObject> villageChunks;
    [SerializeField] List<GameObject> woodChunks;
    [SerializeField] List<GameObject> goldChunks;
    [SerializeField] List<GameObject> sheepChunks;

    List<VillageResources> villageScripts = new List<VillageResources>();
    List<GameObject> chosenEnemyCamps = new List<GameObject>();

    [HideInInspector] public List<Vector2Int> villageLocations;
    [HideInInspector] public List<Vector2Int> enemyCampLocations;
    [HideInInspector] public List<Vector2Int> woodResourceLocation;
    [HideInInspector] public List<Vector2Int> goldResourceLocation;
    [HideInInspector] public List<Vector2Int> foodResourceLocation;

    List<GameObject> allResourceChunks = new List<GameObject>();   
    List<Vector2Int> allResourceLocations = new List<Vector2Int>();

    // grid size variables

    [HideInInspector] public int fullWidth;
    [HideInInspector] public int fullHeight;
    Vector2Int center;

    float tileCellSize = 1.01f;

    [SerializeField] int chunkAmountX = 2;
    [SerializeField] int chunkAmountY = 2;

    float chunkCellSize;
    [HideInInspector] public int chunkWidth = 6;
    [HideInInspector] public int chunkHeight = 6;

    private void Awake()
    {

        fullWidth = chunkWidth * chunkAmountX;
        fullHeight = chunkHeight * chunkAmountY;
        chunkCellSize = chunkHeight * tileCellSize;
        center = new Vector2Int(fullWidth / 2, fullHeight / 2);

        grid = new GridHexXZ<GridObject>(fullWidth, fullHeight, tileCellSize, Vector3.zero,
                                        (g, x, y) => new GridObject(g, x, y));
    }


    public void NewGame()
    {
        GenerateMap();
        SetUpGrid();


        SetUpSettlement();
        SetUpPathfinding();

    }
    public void LoadGame()
    {
        LoadGrid();

        SetUpGrid();


        SetUpSettlement();
        SetUpPathfinding();
    }

    [EditorCools.Button]
    void GenerateMap()
    {
        List<Vector2Int> freeChunks = new List<Vector2Int>();

        for (int i = 0; i < chunkAmountX; i++)
        {
            for (int j = 0; j < chunkAmountY; j++)
            {
                freeChunks.Add(new Vector2Int(i, j));
            }
        }

        int enemyIndex = 0;

            villageLocations = GetRandomVillagePosition(ref freeChunks);
            enemyCampLocations = GetRandomCornerPosition(ref freeChunks);
            woodResourceLocation = GetRandomResourcePosition(ref freeChunks, 4);
            goldResourceLocation = GetRandomResourcePosition(ref freeChunks, 4);
            foodResourceLocation = GetRandomResourcePosition(ref freeChunks, 4);
        
        for (int i = 0; i < chunkAmountX; i++)
        {
            for (int j = 0; j < chunkAmountY; j++)
            {
                Vector2Int currentChunk = new Vector2Int(i, j);
                
                if (enemyCampLocations.Contains(currentChunk))
                {
                    enemyIndex++;
                    GameObject chunk = ChooseEnemyCampType(enemyIndex); 

                    finishedHexChunkList.Add(Instantiate(chunk, grid.GetWorldPosition(i, j, chunkCellSize), Quaternion.identity));

                    mainManager.AddEnemyCamp(chunk);
                }
                else if (villageLocations.Contains(currentChunk))
                {
                    GameObject newVillageChunk = Instantiate(villageChunks[UnityEngine.Random.Range(0, goldChunks.Count)],
                        grid.GetWorldPosition(i, j, chunkCellSize), Quaternion.identity);

                    finishedHexChunkList.Add(newVillageChunk);

                    SetUpVillage(newVillageChunk);
                }
                else if (woodResourceLocation.Contains(currentChunk))
                {
                    GameObject chunk = Instantiate(woodChunks[UnityEngine.Random.Range(0, woodChunks.Count)], grid.GetWorldPosition(i, j, chunkCellSize), Quaternion.identity);

                    finishedHexChunkList.Add(chunk);
                    allResourceChunks.Add(chunk);
                }
                else if (goldResourceLocation.Contains(currentChunk))
                {
                    GameObject chunk = Instantiate(goldChunks[UnityEngine.Random.Range(0, goldChunks.Count)], grid.GetWorldPosition(i, j, chunkCellSize), Quaternion.identity);

                    finishedHexChunkList.Add(chunk);
                    allResourceChunks.Add(chunk);
                }
                else if (foodResourceLocation.Contains(currentChunk))
                {
                    GameObject chunk = Instantiate(sheepChunks[UnityEngine.Random.Range(0, sheepChunks.Count)],
                        grid.GetWorldPosition(i, j, chunkCellSize), Quaternion.identity);
                    
                    finishedHexChunkList.Add(chunk);
                    allResourceChunks.Add(chunk);
                }
                else
                {
                    finishedHexChunkList.Add(Instantiate(randomHexChunks[UnityEngine.Random.Range(0, randomHexChunks.Count)],
                                        grid.GetWorldPosition(i, j, chunkCellSize), Quaternion.identity));
                }
            }
        }


    }
    void LoadGrid()
    {
        int index = 0;
        for (int i = 0; i < chunkAmountX; i++)
        {
            for (int j = 0; j < chunkAmountY; j++)
            {
                Vector2Int currentChunk = new Vector2Int(i, j);

                if (enemyCampLocations.Contains(currentChunk))
                {
                    finishedHexChunkList[index] = Instantiate(finishedHexChunkList[index], grid.GetWorldPosition(i, j, chunkCellSize), Quaternion.identity);

                    mainManager.AddEnemyCamp(finishedHexChunkList[index]);
                }
                else if (villageLocations.Contains(currentChunk))
                {
                    GameObject newVillageChunk = Instantiate(finishedHexChunkList[index],
                        grid.GetWorldPosition(i, j, chunkCellSize), Quaternion.identity);

                    finishedHexChunkList[index] = newVillageChunk;

                    SetUpVillage(newVillageChunk);
                }
                else
                {
                    finishedHexChunkList[index] = Instantiate(finishedHexChunkList[index],
                        grid.GetWorldPosition(i, j, chunkCellSize), Quaternion.identity);
                }

                    index++;
            }
        }
    }
    void SetUpGrid()
    {
        int index = 0;
        int currentChunkY = 0;
        int currentChunkX = 0;

        foreach (GameObject hexChunk in finishedHexChunkList)
        {
            int chunkStartX = currentChunkX;
            int chunkStartY = currentChunkY;
            index = 0;

            for (int x = 0; x < chunkWidth; x++)
            {
                for (int y = 0; y < chunkHeight; y++)
                {

                    int gridX = chunkStartX + x;
                    int gridY = chunkStartY + y;

                    TileObjects placedObject = hexChunk.transform.GetChild(index).GetComponent<TileObjects>();

                    grid.GetGridObject(gridX, gridY).SetTileObject(placedObject);
                    grid.GetGridObject(gridX, gridY).visualTransform = placedObject.transform;
                    index++;

                }
            }

            currentChunkY += chunkHeight;
            if (currentChunkY >= fullHeight)
            {
                currentChunkY = 0;
                currentChunkX += chunkWidth;
            }            
        }

    }
    void SetUpPathfinding()
    {
        pathFinding = new PathFinding(fullWidth, fullHeight, grid);
        foreach (VillageResources village in villageScripts)
        {
            
           village.SetPathNodeGrid(pathFinding.GetGrid(), pathFinding);
            village.SetVillageXY();
        }
    }

    List<Vector2Int> GetRandomVillagePosition(ref List<Vector2Int> freeChunks)
    {
        List<Vector2Int> villageLocations = new List<Vector2Int>();

        int closestY = chunkAmountY / 2;
        int closestX = closestY - 1;
        List<Vector2Int> forbiddenChunks = new List<Vector2Int>() {
            new Vector2Int(closestX, closestX), new Vector2Int(closestX, closestY),
            new Vector2Int(closestY, closestY), new Vector2Int(closestY, closestX)};

        Vector2Int X = new Vector2Int(1, 4);
        Vector2Int Y = new Vector2Int(1, 4);


        for (int i = 0; i < 3; i++)
        {
            int randomX = UnityEngine.Random.Range(X.x, X.y + 1);
            int randomY = UnityEngine.Random.Range(Y.x, Y.y + 1);
            

            Vector2Int chunk = new Vector2Int(randomX, randomY);
            while (!freeChunks.Contains(chunk) || forbiddenChunks.Contains(chunk))
            {
                randomX = UnityEngine.Random.Range(X.x, X.y + 1);
                randomY = UnityEngine.Random.Range(Y.x, Y.y + 1);

                chunk = new Vector2Int(randomX, randomY);
            }

            villageLocations.Add(chunk);
            freeChunks.Remove(chunk);

        }
        return villageLocations;
    }
    List<Vector2Int> GetRandomResourcePosition(ref List<Vector2Int> freeChunks, int resourceAmount)
    {
        List<Vector2Int> resourceLocations = new List<Vector2Int>();
        List<BoundsInt> squares = new List<BoundsInt>();


        // switch the 2s if you want it to change the grid into 9 parts
        int squareWidth = chunkAmountX / 2;
        int squareHeight = chunkAmountY / 2;

        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 2; x++)
            {
                squares.Add(new BoundsInt(
                    x * squareWidth,
                    y * squareHeight,
                    0,
                    squareWidth,
                    squareHeight,
                    1
                ));
            }
        }

        for (int i = 0; i < resourceAmount; i++)
        {
            BoundsInt square = squares[UnityEngine.Random.Range(0, squares.Count)];

            int randomX = UnityEngine.Random.Range(square.xMin, square.xMax);
            int randomY = UnityEngine.Random.Range(square.yMin, square.yMax);
            while (!freeChunks.Contains(new Vector2Int(randomX, randomY)))
            {
                square = squares[UnityEngine.Random.Range(0, squares.Count)];

                randomX = UnityEngine.Random.Range(square.xMin, square.xMax);
                randomY = UnityEngine.Random.Range(square.yMin, square.yMax);
            }


            resourceLocations.Add(new Vector2Int(randomX, randomY));
            squares.Remove(square);
            freeChunks.Remove(new Vector2Int(randomX, randomY));
        }

        return resourceLocations;
    }
    List<Vector2Int> GetRandomCornerPosition(ref List<Vector2Int> freeChunks)
    {
        // changed from 3 to 4? works?
        int qWidth = Mathf.FloorToInt(chunkAmountX / 4f);
        int qHeight = Mathf.FloorToInt(chunkAmountY / 4f);
        List<BoundsInt> corners;


        corners = new List<BoundsInt>
        {
            new BoundsInt(0, 0, 0, qWidth, qHeight, 1),
            new BoundsInt(chunkAmountX - qWidth, 0, 0, qWidth, qHeight, 1),
            new BoundsInt(0, chunkAmountY - qHeight, 0, qWidth, qHeight, 1),
            new BoundsInt(chunkAmountX - qWidth, chunkAmountY - qHeight, 0, qWidth, qHeight, 1)
        };


        List<Vector2Int> randomPositions = new List<Vector2Int>();
        corners.Remove(corners[UnityEngine.Random.Range(0, corners.Count)]);

        foreach (var corner in corners)
        {
            int randomX = UnityEngine.Random.Range(corner.xMin, corner.xMax);
            int randomY = UnityEngine.Random.Range(corner.yMin, corner.yMax);
            randomPositions.Add(new Vector2Int(randomX, randomY));
            freeChunks.Remove(new Vector2Int(randomX, randomY));
        }

        return randomPositions;
    }
    GameObject ChooseEnemyCampType(int index)
    {
        if (index == 2 && !chosenEnemyCamps.Contains(enemyCampChunk[0]))
        {
            chosenEnemyCamps.Add(enemyCampChunk[0]);
            return enemyCampChunk[0]; }
        else if (index == 2 && !chosenEnemyCamps.Contains(enemyCampChunk[1]))
        {
            chosenEnemyCamps.Add(enemyCampChunk[1]);
            return enemyCampChunk[1];
        }
        GameObject chunk = enemyCampChunk[UnityEngine.Random.Range(0, enemyCampChunk.Count)];
        chosenEnemyCamps.Add(chunk);

        return chunk;
    }

    void SetUpSettlement()
    {
        TileObjects tileObject = grid.GetGridObject(center.x, center.y).GetTileObject();
        GameObject chunkParent = tileObject.transform.parent.gameObject;
        
        GameObject newCastle = Instantiate(castleObject, tileObject.transform.position, Quaternion.identity);
        newCastle.transform.SetParent(chunkParent.transform);
        Destroy(tileObject.gameObject);
        grid.GetGridObject(center.x, center.y).SetTileObject(newCastle.GetComponent<TileObjects>());
       
       
        mainManager.SetSettlementTile(newCastle);
        settlement.SetTileObject(newCastle.GetComponent<TileObjects>());

        resourceManager = mainManager.GetResourceManager();
        resourceManager.SetSettlementTile(newCastle.GetComponent<TileObjects>());
    }
    void SetUpVillage(GameObject villageChunk)
    {
        for (int i = 0; i < villageChunk.transform.childCount; i++)
        {
            if (villageChunk.transform.GetChild(i).GetComponent<VillageResources>() != null)
            {
                GameObject villageTile = villageChunk.transform.GetChild(i).gameObject;
                
                villageTile.GetComponent<VillageResources>().SetReferences(mainManager, center, statusBars[villageIndex]);
                villageIndex++;

                villageScripts.Add(villageTile.GetComponent<VillageResources>());

                int x; int y;
                grid.GetXY(villageTile.transform.position, out x, out y);

                mainManager.AddVillage(villageTile.gameObject);
                return;
            }
        }
    }

    public GridHexXZ<GridObject> GetGrid()
    {
        return grid;
    }
    public List<GameObject> GetFullChunkList()
    {
        return finishedHexChunkList;
    }
    public List<TileTypesSO> GetTileTypes()
    {
        return tileTypesToSave;
    }
    public GridHexXZ<PathNode> GetPathNodeGrid()
    {
        return pathFinding.GetGrid();
    }
    public List<VillageResources> GetVillageResources()
    {
        return villageScripts;
    }
    public List<Vector2Int> GetResourceLocations()
    {
        List<Vector2Int> locations;
        locations = woodResourceLocation;
        foreach(Vector2Int vec in goldResourceLocation) locations.Add(vec);
        foreach(Vector2Int vec in foodResourceLocation) locations.Add(vec);
        return locations;
    }

    public class GridObject
    {
        GridHexXZ<GridObject> grid;
        int x, y;
        bool IsEnemy = false;
        TileObjects tileObject;
        public Transform visualTransform;

        public GridObject(GridHexXZ<GridObject> grid, int x, int y)
        {
            this.x = x;
            this.y = y;
            this.grid = grid;
        }

        // remove later
        public void Show()
        {
            visualTransform.gameObject.SetActive(true);
        }
        public void Hide()
        {
            visualTransform.gameObject.SetActive(false);
        }

        public TileObjects GetTileObject()
        {
            return tileObject;
        }
        public void SetTileObject(TileObjects placedObject)
        {
            this.tileObject = placedObject;
            grid.TriggerGridObjectChanged(x, y);
        }
        public Vector2Int GetXY()
        {
            return new Vector2Int(x, y);
        }

        public bool CanBuild()
        {
            return tileObject.GetIsBuildingTile();
        }
        public bool GetIsOccupied()
        {
            return tileObject.GetIsOccupied();
        }
        public void SetIsOccupied(bool option)
        {
            tileObject.SetIsOccupied(option);
        }

        public bool GetIsEnemy()
        {
            return IsEnemy;
        }
        public void SetIsEnemy(bool option)
        {
            IsEnemy = option;
        }
    }
}
