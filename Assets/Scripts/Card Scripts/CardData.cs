public enum Slot
{
    Physical,
    Defense,
    Ally,
    Magical,
    Spirit,
    Drink
}

public enum Rarity
{
    Basic,
    Uncommon,
    Rare
}

public enum TargetType
{
    None,   // Targets nothing
    Self,   // Targets the player
    AOE,    // Targets all enemies
    Unit    // Targets a specific enemy
}

public class CardData
{
    protected string name;
    protected Slot slot;
    protected Rarity rarity;
    protected TargetType targetType;
    protected string description;

    public string Name { get { return name; } }
    public Slot Slot { get { return slot; } }
    public Rarity Rarity { get { return rarity; } }
    public TargetType TargetType { get { return targetType; } }
    public string Description { get { return description; } }

    public CardData(string name, Slot slot, Rarity rarity, TargetType targetType, string description)
    {
        this.name = name;
        this.slot = slot;
        this.rarity = rarity;
        this.targetType = targetType;
        this.description = description;
    }

    public CardData(string name, Slot slot, Rarity rarity, TargetType targetType)
        : this(name, slot, rarity, targetType, "")
    {
        
    }
}
