using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Settlement : MonoBehaviour
{
    int maxSettlementHealth = 200;
    [SerializeField] int currentSettlementHealth = 200;

    [SerializeField] StatusBar settlementHealthBar;

    int baseDefence = 10;
    int baseAttack = 10;

    TileObjects tileObject;

    MAINmanager mainManager;
    EnemyManager enemyManager;
    GameObject guardVisuals;

    public System.Action<bool, bool, int> OnSettlementDamage;

    Death death;
    private void Start()
    {

        mainManager = FindAnyObjectByType<MAINmanager>();
        mainManager.OnMapAwake += OnMapAwake;
        
        death = mainManager.GetDeath();
        enemyManager = mainManager.GetEnemyManager();

        settlementHealthBar.SetMaxValue(maxSettlementHealth);
        settlementHealthBar.SetCurrentValue(currentSettlementHealth);
        
    }

    void OnMapAwake()
    {
        guardVisuals = GameObject.Find("GUARDVISUALS");

    }

    public int GetSettlementHealth()
    {
        return currentSettlementHealth;
    }
    public int GetMaxSettlementhealth()
    {
        return maxSettlementHealth;
    }
    public void SetMaxSettlementHealth()
    {
        maxSettlementHealth += 100;
        currentSettlementHealth += 100;
    }

    public void RefreshHealthBar()
    {
        settlementHealthBar.SetMaxValue(maxSettlementHealth);
        settlementHealthBar.SetCurrentValue(currentSettlementHealth);
    }

    [EditorCools.Button]
    public void Damage()
    {
        DamageSettlement(50);
    }
    public int DamageSettlement(int amount)
    {
        int damage = amount - baseDefence - tileObject.GetCurrentWorkers();
        currentSettlementHealth -= damage;

        RefreshHealthBar();
        settlementHealthBar.gameObject.SetActive(true);

        OnSettlementDamage?.Invoke(true, enemyManager.GetIsAttackingSettlement(),maxSettlementHealth - currentSettlementHealth);
        
        if(currentSettlementHealth <= 0)
        {
            death.OnDeath();
        }

        return (tileObject.GetCurrentWorkers() + baseAttack);
    }
    public void Repair()
    {
        currentSettlementHealth = maxSettlementHealth;

        OnSettlementDamage?.Invoke(false, enemyManager.GetIsAttackingSettlement(), currentSettlementHealth);

        RefreshHealthBar();

        settlementHealthBar.gameObject.SetActive(false);
    }
    public void ReloadSettlementHealth(int currentHealth, int maxHealth)
    {
        currentSettlementHealth = currentHealth;
        maxSettlementHealth = maxHealth;

        RefreshHealthBar();
    }
    public void OnGuardNumChanged()
    {
        ActivateGuardVisuals(activate:false, 5);

        int numVisible = (int)Mathf.Ceil(tileObject.GetCurrentWorkers() / 3);

        ActivateGuardVisuals(activate:true, numVisible);
        
    }
    void ActivateGuardVisuals(bool activate, int numToActivate)
    {
        for (int i = 0; i < numToActivate; i++)
        {
            guardVisuals.transform.GetChild(i).gameObject.SetActive(activate);
        }
    }

    public void SetGuardVisuals(ref GameObject gameObject)
    {
        this.guardVisuals = gameObject;
    }
    public void SetTileObject(TileObjects tileObject)
    {
        this.tileObject = tileObject;

        tileObject.OnGuardNumChanged += OnGuardNumChanged;
    }
}
