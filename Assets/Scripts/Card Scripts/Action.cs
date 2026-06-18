using System.Collections.Generic;

public enum ActionType
{
    Attack,
    Defend,
    Heal,
    Burn,
    Poison,
    Spike,
    Draw,
    Cleanse,
    Summon
}

public enum TargetType
{
    None,
    Self,
    Player,
    Ally,
    AllAllies,
    Foe,
    RandomFoe,
    AllFoes,
}

public class Action
{
    protected ActionType actionType;
    protected int amount;
    protected int times;
    protected TargetType targetType;
    protected bool doesActionTarget;

    public ActionType ActionType { get { return actionType; } }
    public int Amount { get { return amount; } }
    public int Times { get { return times; } }
    public TargetType TargetType { get { return targetType; } }
    public bool DoesActionTarget { get { return doesActionTarget; } }

    public Action(ActionType actionType, int amount, TargetType targetType, int times)
    {
        this.actionType = actionType;
        this.amount = amount;
        this.times = times;
        this.targetType = targetType;
        doesActionTarget = targetType == TargetType.Ally || targetType == TargetType.Foe;
    }

    public Action(ActionType actionType, int amount, TargetType targetType) : this(actionType, amount, targetType, 1)
    {
        
    }

    public virtual string GetActionDescription(Unit unit)
    {
        if(unit == null)
        {
            return "";
        }

        // ===== Description Format =====
        // Normal Attack: "Attack for X"
        // Heal: "Heal for X"
        // Gain defense: "Defend for X"
        // Burn: "Burn for X"
        // Poison: "Poison for X"
        // Gain Spikes: "Spike for X"
        // Draw Cards: "Draw X cards"
        // Cleanse Debuffs: "Cleanse"
        string description = "";
        int descriptionAmount = amount;

        switch(actionType)
        {
            case ActionType.Attack:
                if(unit is Player && CharacterManager.instance.ChosenCharacter == Character.Badger)
                {
                    descriptionAmount++;
                }
                descriptionAmount += unit.UnitEffects.GetEffectAmount("Buff Attack");
                description += string.Format("Attack for {0}", descriptionAmount);
                break;
            case ActionType.Defend:
                descriptionAmount += unit.UnitEffects.GetEffectAmount("Buff Defense");
                description += string.Format("Defend for {0}", descriptionAmount);
                break;
            case ActionType.Heal:
                if(unit is Player && CharacterManager.instance.ChosenCharacter == Character.Fox)
                {
                    descriptionAmount++;
                }
                descriptionAmount += unit.UnitEffects.GetEffectAmount("Buff Healing");
                description += string.Format("Heal for {0}", descriptionAmount);
                break;
            case ActionType.Burn:
                if(unit is Player && CharacterManager.instance.ChosenCharacter == Character.Skunk)
                {
                    descriptionAmount++;
                }
                descriptionAmount += unit.UnitEffects.GetEffectAmount("Buff Burn");
                description += string.Format("Burn for {0}", descriptionAmount);
                break;
            case ActionType.Poison:
                if(unit is Player && CharacterManager.instance.ChosenCharacter == Character.Skunk)
                {
                    descriptionAmount++;
                }
                descriptionAmount += unit.UnitEffects.GetEffectAmount("Buff Poison");
                description += string.Format("Poison for {0}", descriptionAmount);
                break;
            case ActionType.Spike:
                descriptionAmount += unit.UnitEffects.GetEffectAmount("Buff Spike");
                description += string.Format("Gain {0} spikes", descriptionAmount);
                break;
            case ActionType.Draw:
                description += string.Format("Draw {0} cards", amount);
                break;
            case ActionType.Cleanse:
                description += "Cleanse";
                break;
            default:
                break;
        }

        switch(targetType)
        {
            case TargetType.AllAllies:
                description += ", to all";
                break;
            case TargetType.AllFoes:
                description += ", to all";
                break;
            case TargetType.RandomFoe:
                description += ", randomly";
                break;

        }

        if(times > 1)
        {
            description += string.Format(", {0} times", times);
        }

        return description;
    }
}

public class Buff : Action
{
    public Buff(ActionType buffType, int amount) : base(buffType, amount, TargetType.Self)
    {

    }

    public override string GetActionDescription(Unit unit)
    {
        // ===== Description Format =====
        // Buff: "Buff X by Y"
        return string.Format("Buff {0} by {1}", actionType, amount);
    }
}

public class Summon : Action
{
    protected string summonName;
    protected List<Action> summonActions;

    public string SummonName { get { return summonName; } }
    public List<Action> SummonActions { get { return summonActions; } }

    public Summon(ActionType actionType, int amount, string summonName, List<Action> summonActions)
        : base(actionType, amount, TargetType.None)
    {
        this.summonName = summonName;
        this.summonActions = summonActions;
    }

    public override string GetActionDescription(Unit unit)
    {
        // ===== Description Format =====
        // Summon: "Summon a [NAME] with X health. ..."
        int descriptionAmount = amount;
        if(unit is Player && CharacterManager.instance.ChosenCharacter == Character.Opossum)
        {
            descriptionAmount++;
        }
        string description = string.Format("Summon a {0} for {1} health. \n\nOn its turn:", summonName, descriptionAmount);

        foreach(Action action in summonActions)
        {
            description += string.Format(" {0}", action.GetActionDescription(unit));
        }

        return description;
    }
}
