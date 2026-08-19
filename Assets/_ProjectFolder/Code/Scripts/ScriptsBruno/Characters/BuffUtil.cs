using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BuffUtil : SingletonBasic<BuffUtil>
{
    [SerializeField] private SerializedDictionary<BuffID, float> buffMultiplier;
    private Dictionary<BuffID, bool> pasiveSkills;

    protected override void Awake()
    {
        base.Awake();

        pasiveSkills.Add(BuffID.Hp, false);
        pasiveSkills.Add(BuffID.Atk, false);
        pasiveSkills.Add(BuffID.Def, false);
        pasiveSkills.Add(BuffID.Heal, false);
        pasiveSkills.Add(BuffID.steps, false);
        pasiveSkills.Add(BuffID.cash, false);
    }
    public void ActivateBuff(BuffID buffID) => pasiveSkills[buffID] = true;
    public void DeActiveBuffs() 
    {
        pasiveSkills[BuffID.Hp] = false;
        pasiveSkills[BuffID.Atk] = false;
        pasiveSkills[BuffID.Def] = false;
        pasiveSkills[BuffID.Heal] = false;
        pasiveSkills[BuffID.steps] = false;
        pasiveSkills[BuffID.cash] = false;

    }
    public float GetBuff(BuffID buffID) 
    {
        if (pasiveSkills[buffID])
            return buffMultiplier[buffID];
        else return 0;        
    }
}
