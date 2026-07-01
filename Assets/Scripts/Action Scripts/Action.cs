using System.Collections.Generic;

public enum ActionType
{
    WeaponAttack,
    SpellAttack,
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
            case ActionType.WeaponAttack:
                if(unit != null)
                {
                    descriptionAmount += unit.UnitEffects.GetEffectAmount(ActionType.WeaponAttack, true);
                }
                description += string.Format("Physcially attack for {0}", descriptionAmount);
                break;
            case ActionType.SpellAttack:
                if(unit != null)
                {
                    descriptionAmount += unit.UnitEffects.GetEffectAmount(ActionType.SpellAttack, true);
                }
                description += string.Format("Magically attack for {0}", descriptionAmount);
                break;
            case ActionType.Defend:
                if(unit != null)
                {
                    descriptionAmount += unit.UnitEffects.GetEffectAmount(ActionType.Defend, true);
                }
                description += string.Format("Defend for {0}", descriptionAmount);
                break;
            case ActionType.Heal:
                if(unit != null)
                {
                    descriptionAmount += unit.UnitEffects.GetEffectAmount(ActionType.Heal, true);
                }
                description += string.Format("Heal for {0}", descriptionAmount);
                break;
            case ActionType.Burn:
                if(unit != null)
                {
                    descriptionAmount += unit.UnitEffects.GetEffectAmount(ActionType.Burn, true);
                }
                description += string.Format("Burn for {0}", descriptionAmount);
                break;
            case ActionType.Poison:
                if(unit != null)
                {
                    descriptionAmount += unit.UnitEffects.GetEffectAmount(ActionType.Poison, true);
                }
                description += string.Format("Poison for {0}", descriptionAmount);
                break;
            case ActionType.Spike:
                if(unit != null)
                {
                    descriptionAmount += unit.UnitEffects.GetEffectAmount(ActionType.Spike, true);
                }
                description += string.Format("Gain {0} spike", descriptionAmount);
                if(descriptionAmount > 1)
                {
                    description += "s";
                }
                break;
            case ActionType.Draw:
                description += string.Format("Draw {0} card", amount);
                if(amount > 1)
                {
                    description += "s";
                }
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
        string actionTypeString = actionType switch
        {
            ActionType.WeaponAttack => "physcial attacks",
            ActionType.SpellAttack => "magical attacks",
            _ => actionType.ToString(),
        };
        return string.Format("Buff {0} by {1}", actionTypeString, amount);
    }
}

public class Summon : Action
{
    protected string summonName;
    protected List<Action> summonActions;

    public string SummonName { get { return summonName; } }
    public List<Action> SummonActions { get { return summonActions; } }

    public Summon(int amount, string summonName, List<Action> summonActions)
        : base(ActionType.Summon, amount, TargetType.None)
    {
        this.summonName = summonName;
        this.summonActions = summonActions;
    }

    public override string GetActionDescription(Unit unit)
    {
        // ===== Description Format =====
        // Summon: "Summon a [NAME] with X health. ..."
        int buffedAmount = amount + GameManager.instance.Player.UnitEffects.GetEffectAmount(ActionType.Summon, true);
        string description = string.Format("Summon a {0} for {1} health. \n\nOn its turn:", summonName, buffedAmount);

        foreach(Action action in summonActions)
        {
            description += string.Format(" {0}", action.GetActionDescription(unit));
        }

        return description;
    }
}

public class EnemySummon : Action
{
    protected EnemyType enemyType;

    public EnemyType EnemyType { get { return enemyType; } }

    public EnemySummon(int amount, EnemyType enemyType) : base(ActionType.Summon, amount, TargetType.None)
    {
        this.enemyType = enemyType;
    }
}
