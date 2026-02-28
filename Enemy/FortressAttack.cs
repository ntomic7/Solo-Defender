 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FortressAttack : MonoBehaviour
{
    [SerializeField] MAINmanager mainManager;

    [SerializeField] GameObject playerGO;
    [SerializeField] TileObjects grassTileObject;

    GridHexXZ<GridBuildingSystem.GridObject> grid;

    BattleManager battleManager;
    BattleVisuals battleVisuals;
    int round = 0;

    TileObjects fortressTile;
    List<GameObject> currentEnemyList = new List<GameObject>();

    void Start()
    {
        grid = mainManager.GetGrid();

        battleManager = GetComponent<BattleManager>();
        battleVisuals = mainManager.GetBattleVisuals();

        battleManager.OnBattleStart += NextRound;
    }

    public void StartFortressAttack(TileObjects fortressTile, List<GameObject> enemyList)
    {
        SoundSystem.instance.ChangeMusic(SoundSystem.Music.Battle);


        currentEnemyList = enemyList;
        this.fortressTile = fortressTile;

        battleManager.StartBattle(enemyList[0], IsFortressAttack:true);
        battleManager.SetIsFortressAttack(true, round);
        round = 0;
    }

    void NextRound(bool option)
    {
        if (battleManager.GetIsFortressAttack())
        {

            if (!option) round++;

            if (!option && round < 3)
            {
                StartCoroutine(WaitForNextRound());
            }
            if (round == 1) battleVisuals.SetIsFortressAttack(true);
            if (round == 2)
            {
                battleManager.SetIsFortressAttack(false, round);
                battleManager.StartBossBattle();

                SoundSystem.instance.ChangeMusic(SoundSystem.Music.Boss);
            }
        }
        else if (!battleManager.GetIsFortressAttack() && round == 2 && !battleManager.GetIsBattling()
            && SoundSystem.instance.GetCurrentBGMusic() != SoundSystem.Music.Death)
        {
            SoundSystem.instance.ChangeMusic(SoundSystem.Music.Main);
            FinishFortressAttack();
        }
   }
    IEnumerator WaitForNextRound()
    {
        yield return new WaitForSeconds(1f);

        battleManager.StartBattle(currentEnemyList[round], IsFortressAttack: true);
    }

    void FinishFortressAttack()
    {
        round = 0;

        battleManager.StopFortressAttack();
        battleVisuals.SetIsFortressAttack(false);
        fortressTile.ChangeFullTile(grassTileObject);
        fortressTile.GetComponentInParent<EnemyCampSpawner>().DestroyCamp();
        DestroyImmediate(fortressTile.GetComponentInParent<EnemyCampSpawner>());
    }
}
