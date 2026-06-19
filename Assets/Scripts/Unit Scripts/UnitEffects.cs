using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitEffects
{
    private List<Effect> effects;

    public UnitEffects()
    {
        Effect burnBuffEffect = new Effect(new Buff(ActionType.Burn, 0), false);
        Effect poisonBuffEffect = new Effect(new Buff(ActionType.Poison, 0), false);
        Effect spikeBuffEffect = new Effect(new Buff(ActionType.Spike, 0), false);

        effects = new List<Effect>
        {
            new Effect(new Buff(ActionType.Attack, 0), false),
            new Effect(new Buff(ActionType.Defend, 0), false),
            new Effect(new Buff(ActionType.Heal, 0), false),
            burnBuffEffect,
            poisonBuffEffect,
            spikeBuffEffect,
            new Effect(new Action(ActionType.Burn, 0, TargetType.Self), true, burnBuffEffect),
            new Effect(new Action(ActionType.Poison, 0, TargetType.Self), true, poisonBuffEffect),
            new Effect(new Action(ActionType.Spike, 0, TargetType.Self), false, spikeBuffEffect),
        };
    }

    public bool UpdateEffectAmount(ActionType effectType, int amount, bool isBuff = false)
    {
        foreach(Effect effect in effects.Where(e => e.Action is Buff == isBuff))
        {
            if(effect.Action.ActionType == effectType)
            {
                effect.UpdateAmount(amount);
                return true;
            }
        }

        Debug.Log(string.Format("Warning! No effect of type {0} found. Is it a buff? {1}", effectType, isBuff));
        return false;
    }

    public int GetEffectAmount(ActionType effectType, bool isBuff = false)
    {
        foreach(Effect effect in effects.Where(e => e.Action is Buff == isBuff))
        {
            if(effect.Action.ActionType == effectType)
            {
                return effect.Action.Amount;
            }
        }

        Debug.Log(string.Format("Warning! No effect of type {0} found. Is it a buff? {1}", effectType, isBuff));
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
        return effects.Where(effect => effect.IsAffliction).Where(effect => effect.Action.Amount > 0).Any();
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
