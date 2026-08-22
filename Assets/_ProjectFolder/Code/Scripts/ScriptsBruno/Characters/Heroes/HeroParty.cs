using System.Collections.Generic;
using UnityEngine;

public class HeroParty : MonoBehaviour
{
    [SerializeField] private Hero party;
    [SerializeField] private BuffUtil buff;
    [SerializeField] private SerializedDictionary<HeroID, SoHero> availablesHeroes;
    
    private readonly Dictionary<HeroID, SoHero> selectedHeroes = new();
    
    private void Awake()
    {
        AssingHero(HeroID.Mage, availablesHeroes[HeroID.Mage]);
        AssingHero(HeroID.Warrior, availablesHeroes[HeroID.Warrior]);
    }
    private void Start() => AssignStats();

    public void AssignStats() // calculadora de danio + buffs
    {
        float hp = 0, atk = 0, def = 0;
        buff.DeActiveBuffs();

        foreach (var item in selectedHeroes)
        {
            hp += item.Value.hp;
            atk += item.Value.atk;
            def += item.Value.def;
            buff.ActivateBuff(item.Value.pasive);
        }
        // aplicar buffs y multiplicadores
        hp = (hp + StatUtil.Instance.GetBonusHealth()) * buff.GetBuff(BuffID.Hp);
        atk = (atk + StatUtil.Instance.GetBonusDamage()) * buff.GetBuff(BuffID.Atk);
        def = (def + StatUtil.Instance.GetBonusDefense()) * buff.GetBuff(BuffID.Def);
        // despues
        party?.SetStats(hp, atk, def);
    }
    
    public Hero GetParty() => party;
    public void AssingHero(HeroID heroID, SoHero hero) => selectedHeroes.Add(heroID, hero);
    public void AttackEnemy(Enemy enemy) => enemy.ReciveDamage(party.GetTotalAttack());
    public void RecieveDamage(int amount) => party.ReciveDamage(amount);
    public bool IsPartyAlive() => party.IsAlive();
}