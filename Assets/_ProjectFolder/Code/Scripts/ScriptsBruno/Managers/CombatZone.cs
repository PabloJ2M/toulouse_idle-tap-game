using System.Collections;
using UnityEngine;

public class CombatZone : MonoBehaviour
{
    [SerializeField] private HeroParty party;
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
        while (party.IsPartyAlive() && enemyGroup.IsEnemiesAlive())
        {
            enemyGroup.AttackHero(party.GetParty());
            party.AttackEnemy(enemyGroup.GetLastAttacker());
            enemyGroup.CheckEnemiesAlive();
            yield return new WaitForSecondsRealtime(0.5f);
        }
        
        combatCoroutine = null;
    }
}