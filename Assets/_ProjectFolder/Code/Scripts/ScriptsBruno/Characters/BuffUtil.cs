using System.Collections.Generic;
using UnityEngine;

public class BuffUtil : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<BuffID, float> buffMultiplier;
    private readonly Dictionary<BuffID, bool> _passiveSkills = new();

    private void Awake()
    {
        _passiveSkills.Add(BuffID.Hp, false);
        _passiveSkills.Add(BuffID.Atk, false);
        _passiveSkills.Add(BuffID.Def, false);
        _passiveSkills.Add(BuffID.Heal, false);
        _passiveSkills.Add(BuffID.steps, false);
        _passiveSkills.Add(BuffID.cash, false);
    }
    public void ActivateBuff(BuffID buffID) => _passiveSkills[buffID] = true;
    public void DeActiveBuffs() 
    {
        _passiveSkills[BuffID.Hp] = false;
        _passiveSkills[BuffID.Atk] = false;
        _passiveSkills[BuffID.Def] = false;
        _passiveSkills[BuffID.Heal] = false;
        _passiveSkills[BuffID.steps] = false;
        _passiveSkills[BuffID.cash] = false;

    }
    public float GetBuff(BuffID buffID)
    {
        if (!_passiveSkills.TryGetValue(buffID, out var hasSkill))
            return 0;
        
        return hasSkill ? buffMultiplier.GetValueOrDefault(buffID, 0) : 0;
    }
}
