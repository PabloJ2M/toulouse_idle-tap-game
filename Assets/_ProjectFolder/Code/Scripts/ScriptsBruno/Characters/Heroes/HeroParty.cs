using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HeroParty : SingletonBasic<HeroParty>
{
    [SerializeField] private SerializedDictionary<HeroID, SoHero> availablesHeroes;
    private Dictionary<HeroID, SoHero> selectedHeroes;
    private Hero party;

    public void AssignStats() // calculadora de danio + buffs
    {
        int hp = 0;
        int atk = 0;
        int def = 0;
        foreach (var item in selectedHeroes)
        {
            hp += item.Value.hp;
            atk += item.Value.atk;
            def += item.Value.def;
        }
        // aplicar buffs y multiplicadores

        // despues
        party.SetStats(hp, atk, def);
    }
    public void AssingHero(HeroID heroID, SoHero hero) => selectedHeroes.Add(heroID, hero);
    public void PartyTurn(Enemy enemy, int damageAmount) => enemy.ReciveDamage(damageAmount);
    public void RecieveDamage(int amount) => party.ReciveDamage(amount);
    public bool IsPartyAlive() => party.IsAlive();
}
