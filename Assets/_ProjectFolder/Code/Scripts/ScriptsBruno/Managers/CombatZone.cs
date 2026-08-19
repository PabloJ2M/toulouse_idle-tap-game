using System.Collections;
using UnityEngine;

public class CombatZone : MonoBehaviour
{
    [SerializeField] private EnemyGroup enemyGroup;
    private Coroutine combatCoroutine;
    public void StartCombat() => Combat();
    private void Combat() 
    {
        enemyGroup = EnemyManager.Instance.GetEnemyGroup();
        if (combatCoroutine == null)
            StartCoroutine(CombatRoutine());
    }
    private IEnumerator CombatRoutine() 
    {
        while (HeroParty.Instance.IsPartyAlive() && enemyGroup.IsEnemiesAlive())
        {
            enemyGroup.AttackHero(HeroParty.Instance.GetParty());
            HeroParty.Instance.AttackEnemy(enemyGroup.GetLastAttacker());
            enemyGroup.CheckEnemiesAlive();
            yield return new WaitForSecondsRealtime(0.5f);
        }
        combatCoroutine = null;
    }
}
