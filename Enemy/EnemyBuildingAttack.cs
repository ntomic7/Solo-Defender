using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class EnemyBuildingAttack : MonoBehaviour
{
    EnemyManager enemyManager;
    BattleVisuals battleVisuals;  
    BattleManager battleManager;

    ResourceManagerVisuals rmv;

    Attributes enemyAttributes;
    Animator enemyAnimator;

    public bool IsAttackingVillage;
    public bool IsAttackingSettlement;

    GameObject go;

    void Start()
    {
        enemyAttributes = GetComponent<Attributes>();
        enemyAnimator = GetComponentInChildren<Animator>();

        battleVisuals = enemyManager.GetBattleVisuals();
        battleManager = battleVisuals.GetComponent<BattleManager>();

        go = this.gameObject;
    }


    public void AttackSettlement(Settlement settlement, GameObject settlementGO)
    {
        if (!IsAttackingSettlement)
        {
            SoundSystem.instance.PlaySound(SoundSystem.Sound.Alarm);
            enemyManager.WarningText("Settlement");
            enemyManager.SetIsAttackingSettlement(true);
            enemyManager.SetIsBuildingAttacked(settlementGO, true);
        }

        IsAttackingSettlement = true;

        int enemyAttack = enemyAttributes.baseAttack + (Random.Range(enemyAttributes.weaponAttack[0], enemyAttributes.weaponAttack[1]));
        int settlementAttack = -settlement.DamageSettlement(enemyAttack);

        enemyAnimator.SetTrigger("OnSettlementAttack");
        enemyAttributes.SetValue(settlementAttack, "health");

        if(enemyAttributes.GetHealth() <= 0)
        {
            SoundSystem.instance.PlaySound(SoundSystem.Sound.EnemyDeath);
            enemyManager.RemoveEnemy(this.gameObject);
            Destroy(gameObject);
        }
    }
    public bool AttackVillage(ref TileObjects villageTile, ref Vector2Int chosenVillage, ref List<Vector2Int> villageLocations)
    {
        VillageResources villageResources = villageTile.GetComponent<VillageResources>();

        if (!IsAttackingVillage)
        {
            SoundSystem.instance.PlaySound(SoundSystem.Sound.Alarm);
            enemyManager.WarningText("Village");
        }
        villageResources.IsBeingAttacked = true;
        IsAttackingVillage = true;

        int enemyAttack = enemyAttributes.baseAttack + (Random.Range(enemyAttributes.weaponAttack[0], enemyAttributes.weaponAttack[1]));


        villageResources.DamageVillage(enemyAttack);
        enemyManager.SetIsBuildingAttacked(villageTile.gameObject, true);

        if (villageResources.GetCurrentHealth() <= 0)
        {
            enemyManager.SetIsBuildingAttacked(villageTile.gameObject, false);
            
            IsAttackingVillage = false;
            villageResources.IsBeingAttacked = false;
            
            villageTile = null;
            return false;
        }
        return true;
    }

    public void SetEnemymanager(EnemyManager enemyManager)
    {
        this.enemyManager = enemyManager;
    }
    public bool GetIsAttackingVillage()
    {
        return IsAttackingVillage;
    }
    public bool GetIsAttackingSettlement()
    {
        return IsAttackingSettlement;
    }
}
