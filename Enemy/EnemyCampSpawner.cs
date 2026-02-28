using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class EnemyCampSpawner : MonoBehaviour
{
    MAINmanager mainManager;
    TurnManager turnManager;
    EnemyManager enemyManager;
    PlayerLevelManager playerLevelManager;

    [SerializeField] List<GameObject> campEnemyList;
    [SerializeField] Transform enemyCampPosition;

    int cooldown;
    
    void Start()
    {
        mainManager = FindAnyObjectByType<MAINmanager>();
        enemyManager = mainManager.GetEnemyManager();
        playerLevelManager = enemyManager.GetPlayerLevelManager();

        turnManager = enemyManager.GetTurnManager();

        turnManager.OnEndOfTurn += OnNextTurn;
        mainManager.OnPlayerDeath += ResetEnemyMap;

        { cooldown = UnityEngine.Random.Range(30, UnityEngine.Random.Range(40, 50)); }
    }

    void OnNextTurn()
    {
        cooldown--;

        if(cooldown <= 0 && (enemyManager.GetEnemyCount() < enemyManager.GetMaxEnemyCount()))
        {
            SpawnEnemy();
        }
        else if(enemyManager.GetEnemyCount() >= enemyManager.GetMaxEnemyCount())
        {
            if (turnManager.GetTurn() < 40) { cooldown = UnityEngine.Random.Range(30, UnityEngine.Random.Range(40, 50)); }
            else cooldown = UnityEngine.Random.Range(10, UnityEngine.Random.Range(20, 30));
        }
    }
    GameObject ChooseEnemy()
    {
        int playerLevel = playerLevelManager.GetPlayerLevel();
        List<GameObject> options = new List<GameObject>() { campEnemyList[0], campEnemyList[1] };

        if (playerLevel >= 15) return campEnemyList[1];
        else if (playerLevel >= 10)
        {
            for (int i = 0; i < 3; i++) options.Add(campEnemyList[1]);

            return options[UnityEngine.Random.Range(0, options.Count)];
        }
        else if (playerLevel >= 5) return options[UnityEngine.Random.Range(0, options.Count)];
        else return campEnemyList[0];
    }
    [EditorCools.Button]
    void SpawnEnemy()
    {
        GameObject enemy = Instantiate(ChooseEnemy(), enemyCampPosition.position, Quaternion.identity);
        enemy.GetComponent<EnemyMovement>().SetManagers(turnManager, enemyManager, enemyManager.GetSettlement());
        if(playerLevelManager.GetPlayerLevel() >= 15) BuffEnemyAttributes(enemy);

        if (turnManager.GetTurn() < 40) { cooldown = UnityEngine.Random.Range(30, UnityEngine.Random.Range(40, 50)); }
        else cooldown = UnityEngine.Random.Range(10, UnityEngine.Random.Range(20, 30));

        enemyManager.AddEnemy(enemy);
    }
    void BuffEnemyAttributes(GameObject enemy)
    {
        Attributes attributes = enemy.GetComponent<Attributes>();

        attributes.baseDefence += 8;

        attributes.weaponAttack = new Vector2Int(attributes.weaponAttack.x + 10, attributes.weaponAttack.y + 10);
        attributes.armourDefence = new Vector2Int(attributes.armourDefence.x + 5, attributes.armourDefence.y + 5);

    }

    public void RemoveEnemy(GameObject enemyGO)
    {
        enemyManager.RemoveEnemy(enemyGO);
    }
    public void ResetEnemyMap()
    {
        foreach(GameObject enemy in enemyManager.GetCurrentEnemyList()) Destroy(enemy);
        enemyManager.SetEnemyCount(0);
        enemyManager.ClearEnemyList();
        cooldown = UnityEngine.Random.Range(10, UnityEngine.Random.Range(20, 30));
    }

    public int GetCooldown()
    {
        return cooldown;
    }
    public void SetCooldown(int cooldown)
    {
        this.cooldown = cooldown;
    }

    public List<GameObject> GetEnemyTypesList()
    {
        return campEnemyList;
    }

    public void DestroyCamp()
    {
        turnManager.OnEnemyCampDestruction();
    }
    private void OnDestroy()
    {
        turnManager.OnEndOfTurn -= OnNextTurn;
        mainManager.OnPlayerDeath -= ResetEnemyMap;
    }
}
