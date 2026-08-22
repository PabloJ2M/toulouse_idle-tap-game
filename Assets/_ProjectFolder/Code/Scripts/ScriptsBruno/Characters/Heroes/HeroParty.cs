using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HeroParty : SingletonBasic<HeroParty>
{
    [SerializeField] private SerializedDictionary<HeroID, SoHero> availablesHeroes;
    private Dictionary<HeroID, SoHero> selectedHeroes;
    private Hero party;
    protected override void Awake()
    {
        base.Awake();
        AssingHero(HeroID.Mage, availablesHeroes[HeroID.Mage]);
        AssingHero(HeroID.Warrior, availablesHeroes[HeroID.Warrior]);

        AssignStats();
    }

    public void AssignStats() // calculadora de danio + buffs
    {
        float hp = 0;
        float atk = 0;
        float def = 0;
        BuffUtil.Instance.DeActiveBuffs();

        foreach (var item in selectedHeroes)
        {
            hp += item.Value.hp;
            atk += item.Value.atk;
            def += item.Value.def;
            BuffUtil.Instance.ActivateBuff(item.Value.pasive);
        }
        // aplicar buffs y multiplicadores
        hp = (hp + StatUtil.Instance.GetBonusHealth()) * BuffUtil.Instance.GetBuff(BuffID.Hp);
        atk = (atk + StatUtil.Instance.GetBonusDamage()) * BuffUtil.Instance.GetBuff(BuffID.Atk);
        def = (def + StatUtil.Instance.GetBonusDefense()) * BuffUtil.Instance.GetBuff(BuffID.Def);
        // despues
        party.SetStats(hp, atk, def);
    }
    public Hero GetParty() => party;
    public void AssingHero(HeroID heroID, SoHero hero) => selectedHeroes.Add(heroID, hero);
    public void AttackEnemy(Enemy enemy) => enemy.ReciveDamage(party.GetTotalAttack());
    public void RecieveDamage(int amount) => party.ReciveDamage(amount);
    public bool IsPartyAlive() => party.IsAlive();
}
