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
    private bool doesCardTarget;

    public string Name { get { return name; } }
    public Slot Slot { get { return slot; } }
    public Rarity Rarity { get { return rarity; } }
    public bool DoesCardTarget { get { return doesCardTarget; } }

    public CardData(string name, Slot slot, Rarity rarity, List<Action> actions)
    {
        this.name = name;
        this.slot = slot;
        this.rarity = rarity;
        this.actions = actions;
        doesCardTarget = actions.Where(action => action.DoesActionTarget).Any();
    }

    public string GetCardDescription()
    {
        string description = "";
        for(int i = 0; i < actions.Count; i++)
        {
            description += actions[i].GetActionDescription();

            if(i < actions.Count - 1)
            {
                description += ". ";
            }
        }

        return description;
    }
}