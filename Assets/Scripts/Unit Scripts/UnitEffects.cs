using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitEffects
{
    private List<Effect> effects;

    public UnitEffects()
    {
        Effect burnBuffEffect = new Effect("Buff Burn", 0, false);
        Effect poisonBuffEffect = new Effect("Buff Poison", 0, false);
        Effect spikeBuffEffect = new Effect("Buff Spike", 0, false);

        effects = new List<Effect>
        {
            new Effect("Buff Attack", 0, false),
            new Effect("Buff Defense", 0, false),
            new Effect("Buff Healing", 0, false),
            burnBuffEffect,
            poisonBuffEffect,
            spikeBuffEffect,
            new Effect("Burn", 0, true, burnBuffEffect),
            new Effect("Poison", 0, true, poisonBuffEffect),
            new Effect("Spike", 0, false, spikeBuffEffect),
        };
    }

    public bool UpdateEffectAmount(string effectName, int amount)
    {
        foreach(Effect effect in effects)
        {
            if(effect.EffectName == effectName)
            {
                effect.UpdateAmount(amount);
                return true;
            }
        }

        Debug.Log(string.Format("Warning! Effect {0} not found", effectName));
        return false;
    }

    public int GetEffectAmount(string effectName)
    {
        foreach(Effect effect in effects)
        {
            if(effect.EffectName == effectName)
            {
                return effect.Amount;
            }
        }

        Debug.Log(string.Format("Warning! Effect {0} not found", effectName));
        return 0;
    }

    public void Cleanse()
    {
        foreach(Effect effect in effects)
        {
            if(effect.IsAffliction)
            {
                effect.Reset();
            }
        }
    }

    public bool hasAfflictiveEffects()
    {
        return effects.Where(effect => effect.IsAffliction).Where(effect => effect.Amount > 0).Any();
    }

    public List<Effect> GetAllEffects()
    {
        return effects;
    }

    public void ResetAllEffects()
    {
        for(int i = 0; i < effects.Count; i++)
        {
            effects[i].Reset();
        }
    }
}
