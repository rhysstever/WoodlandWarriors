using System.Collections.Generic;
using System.Linq;

public enum Slot
{
    MainHand,
    OffHand,
    Ally,
    Spell,
    Spirit,
    Drink
}

public enum Rarity
{
    Basic,
    Uncommon,
    Rare
}

public class CardData
{
    protected string name;
    protected Slot slot;
    protected Rarity rarity;
    protected List<Action> actions;
    protected List<Action> combatStartActions;
    private bool doesCardTarget;

    public string Name { get { return name; } }
    public Slot Slot { get { return slot; } }
    public Rarity Rarity { get { return rarity; } }
    public List<Action> Actions { get { return actions; } }
    public List<Action> CombatStartActions { get { return combatStartActions; } }
    public bool DoesCardTarget { get { return doesCardTarget; } }

    public CardData(string name, Slot slot, Rarity rarity, List<Action> actions, List<Action> combatStartActions)
    {
        this.name = name;
        this.slot = slot;
        this.rarity = rarity;
        this.actions = actions;
        this.combatStartActions = combatStartActions;
        doesCardTarget = actions.Where(action => action.DoesActionTarget).Any();
    }

    public CardData(string name, Slot slot, Rarity rarity, List<Action> actions) : this(name, slot, rarity, actions, new List<Action>())
    {
        
    }

    public string GetCardDescription(Unit unit)
    {
        string description = "";
        for(int i = 0; i < actions.Count; i++)
        {
            description += actions[i].GetActionDescription(unit);

            if(i < actions.Count - 1)
            {
                description += ". ";
            }
        }

        for(int i = 0; i < combatStartActions.Count; i++)
        {
            description += ". \n\nAt the start of combat: ";
            description += combatStartActions[i].GetActionDescription(unit);

            if(i < actions.Count - 1)
            {
                description += ". ";
            }
        }

        return description;
    }
}