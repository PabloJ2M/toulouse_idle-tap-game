using System.Collections;
using UnityEngine;

public class CombatZone : MonoBehaviour
{
    [SerializeField] private EnemyGroup enemyGroup;
    private Coroutine combatCoroutine;


    public void StartCombat() 
    {
        
    }
    private void Combat() 
    {
        if (combatCoroutine == null)
            StartCoroutine(CombatRoutine());
    }
    private IEnumerator CombatRoutine() 
    {
        yield return null;
    }
}
