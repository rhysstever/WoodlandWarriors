using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    // Singleton
    public static CardManager instance = null;

    // Set in inspector
    [SerializeField]    // Basic card base sprites
    private Sprite cardBaseAttack, cardBaseDefend, cardBaseAlly, cardBaseSpell, cardBaseSpirit, cardBaseDrink;
    [SerializeField]    // Uncommon card base sprites
    private Sprite cardBaseUncommonAttack, cardBaseUncommonDefend, cardBaseUncommonAlly, cardBaseUncommonSpell, cardBaseUncommonSpirit, cardBaseUncommonDrink;
    [SerializeField]    // Rare card base sprites
    private Sprite cardBaseRareAttack, cardBaseRareDefend, cardBaseRareAlly, cardBaseRareSpell, cardBaseRareSpirit, cardBaseRareDrink;
    [SerializeField]    // Action icon sprites
    private Sprite actionIconSpriteAttack, actionIconSpriteDefend, actionIconSpriteHeal, actionIconSpriteFire, actionIconSpritePoison, actionIconSpriteSpike, actionIconSpriteSummon;
    [SerializeField]
    private GameObject effectUIPrefab, effectBuffUIPrefab;

    // Set in script
    private List<CardData> cardLibrary;
    private Dictionary<Rarity, float> rarityPercentages;
    private List<Sprite> cardArtList;

    // Slots
    private CardData mainHand, offHand, spirit, ally, spell, drink;

    public GameObject EffectUIPrefab { get { return effectUIPrefab; } }
    public GameObject EffectBuffUIPrefab { get { return effectBuffUIPrefab; } }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        cardArtList = LoadCardArtSprites();
        cardLibrary = CardCreation();

        Reset();
    }

    void Start()
    {
        rarityPercentages = new Dictionary<Rarity, float>();
        rarityPercentages.Add(Rarity.Basic, 0.75f);
        rarityPercentages.Add(Rarity.Uncommon, 0.25f);
    }

    #region Card Creation
    private List<CardData> CardCreation()
    {
        // Reminder: First card of each slot will be there starter card 

        List<CardData> cards = new() {
            // Main hand cards
            new CardData("Shortsword", Slot.MainHand, Rarity.Basic, new List<Action> { new Action(ActionType.Attack, 1, TargetType.Foe) }),
            new CardData("Wand", Slot.MainHand, Rarity.Basic, new List<Action> { new Action(ActionType.Burn, 1, TargetType.RandomFoe) }),
            new CardData("Staff", Slot.MainHand, Rarity.Basic, new List<Action> { new Action(ActionType.Burn, 2, TargetType.Foe) }),
            new CardData("Mace", Slot.MainHand, Rarity.Basic, new List<Action> { new Action(ActionType.Attack, 3, TargetType.AllFoes) }),
            new CardData("Flail", Slot.MainHand, Rarity.Basic, new List<Action> { new Action(ActionType.Attack, 2, TargetType.RandomFoe, 3) }),
            new CardData("Flaming Arrow", Slot.MainHand, Rarity.Basic, new List<Action> {
                new Action(ActionType.Attack, 2, TargetType.Foe),
                new Action(ActionType.Burn, 2, TargetType.Foe),
            }),
            new CardData("Spear", Slot.MainHand, Rarity.Uncommon, new List<Action> { new Action(ActionType.Attack, 6, TargetType.Foe) }),
            new CardData("Trident", Slot.MainHand, Rarity.Uncommon, new List<Action> {
                new Action(ActionType.Attack, 4, TargetType.Foe),
                new Action(ActionType.Heal, 4, TargetType.Player)
            }),
            new CardData("Scythe", Slot.MainHand, Rarity.Uncommon, new List<Action> {
                new Action(ActionType.Attack, 3, TargetType.Foe),
                new Action(ActionType.Poison, 3, TargetType.Foe)
            }),

            // Off hand cards
            new CardData("Wooden Shield", Slot.OffHand, Rarity.Basic, new List<Action> { new Action(ActionType.Defend, 1, TargetType.Player) }),
            new CardData("Buckler", Slot.OffHand, Rarity.Basic, new List<Action> { new Action(ActionType.Defend, 2, TargetType.Player) }),
            new CardData("Cloak and Dagger", Slot.OffHand, Rarity.Basic, new List<Action> {
                new Action(ActionType.Defend, 1, TargetType.Player),
                new Action(ActionType.Attack, 1, TargetType.Foe)
            }),
            new CardData("Quiver", Slot.OffHand, Rarity.Basic, new List<Action> { new Action(ActionType.Attack, 1, TargetType.RandomFoe) }),
            //new CardData("Scroll", Slot.OffHand, Rarity.Basic, new List<CardAction> {}),
            new CardData("Spike Shield", Slot.OffHand, Rarity.Uncommon, new List<Action> {
                new Action(ActionType.Defend, 3, TargetType.Player),
                new Action(ActionType.Spike, 1, TargetType.Player)
            }),
            //new CardData("Tome", Slot.OffHand, Rarity.Uncommon, new List<CardAction> {}),
            new CardData("Tower Shield", Slot.OffHand, Rarity.Uncommon, new List<Action> { new Action(ActionType.Defend, 5, TargetType.Player) }),
            //new CardData("Arcane Focus", Slot.OffHand, Rarity.Uncommon, new List<CardAction> {}),

            // Ally cards
            new CardData("Squirrel", Slot.Ally, Rarity.Basic, new List<Action> { 
                new Summon(1, "Squirrel", new List<Action> { new Action(ActionType.Attack, 1, TargetType.RandomFoe) }) 
            }),
            new CardData("Frog", Slot.Ally, Rarity.Basic, new List<Action> { 
                new Summon(1, "Frog", new List<Action> { new Action(ActionType.Heal, 1, TargetType.Player) }) 
            }),
            new CardData("Rat", Slot.Ally, Rarity.Basic, new List<Action> { 
                new Summon(1, "Rat", new List<Action> { new Action(ActionType.Poison, 1, TargetType.RandomFoe) }) 
            }),
            new CardData("Newt", Slot.Ally, Rarity.Basic, new List<Action> { 
                new Summon(1, "Newt", new List<Action> { new Action(ActionType.Burn, 1, TargetType.RandomFoe) }) 
            }),
            //new CardData("Bunny", Slot.Ally, Rarity.Uncommon, new List<Action> {
            //    new Summon(1, "Bunny", new List<Action> { new Action(ActionType.Heal, 1, TargetType.Self) })
            //}),
            new CardData("Toad", Slot.Ally, Rarity.Uncommon, new List<Action> { 
                new Summon(1, "Toad", new List<Action> {
                    new Action(ActionType.Heal, 1, TargetType.Player),
                    new Action(ActionType.Poison, 1, TargetType.RandomFoe)
                }) 
            }),
            new CardData("Porcupine", Slot.Ally, Rarity.Uncommon, new List<Action> { 
                new Summon(1, "Porcupine", new List<Action> { new Action(ActionType.Spike, 1, TargetType.Player) }) 
            }),
            new CardData("Hamster", Slot.Ally, Rarity.Uncommon, new List<Action> { 
                new Summon(1, "Hamster", new List<Action> { new Action(ActionType.Draw, 1, TargetType.None) }) 
            }),

            // Spirit cards
            new CardData("Air Spirit", Slot.Spirit, Rarity.Basic, new List<Action> { new Buff(ActionType.Attack, 1) }),
            new CardData("Earth Spirit", Slot.Spirit, Rarity.Basic, new List<Action> { new Buff(ActionType.Defend, 1) }),
            new CardData("Fire Spirit", Slot.Spirit, Rarity.Basic, new List<Action> { new Buff(ActionType.Burn, 1) }),
            new CardData("Water Spirit", Slot.Spirit, Rarity.Basic, new List<Action> { new Buff(ActionType.Poison, 1) }),
            new CardData("Dark Spirit", Slot.Spirit, Rarity.Uncommon, new List<Action> { new Buff(ActionType.Spike, 1) }),
            new CardData("Light Spirit", Slot.Spirit, Rarity.Uncommon, new List<Action> { new Buff(ActionType.Heal, 1) }),

            // Spell cards
            new CardData("Arcane Bolt", Slot.Spell, Rarity.Basic, new List<Action> { new Action(ActionType.Attack, 1, TargetType.RandomFoe) }),
            new CardData("Fireball", Slot.Spell, Rarity.Basic, new List<Action> { new Action(ActionType.Burn, 3, TargetType.Foe) }),
            new CardData("Life Drain", Slot.Spell, Rarity.Basic, new List<Action> {
                new Action(ActionType.Attack, 2, TargetType.Foe),
                new Action(ActionType.Heal, 1, TargetType.Player)
            }),
            new CardData("Lightning Strike", Slot.Spell, Rarity.Uncommon, new List<Action> { new Action(ActionType.Attack, 4, TargetType.RandomFoe) }),
            new CardData("Heal", Slot.Spell, Rarity.Uncommon, new List<Action> { new Action(ActionType.Heal, 5, TargetType.Ally) }),
            new CardData("Blizzard", Slot.Spell, Rarity.Uncommon, new List<Action> { new Action(ActionType.Attack, 3, TargetType.AllFoes) }),
            new CardData("Curse", Slot.Spell, Rarity.Uncommon, new List<Action> { new Action(ActionType.Poison, 5, TargetType.Foe) }),

            // Drink cards
            new CardData("Cup", Slot.Drink, Rarity.Basic, new List<Action> { new Action(ActionType.Heal, 1, TargetType.Player) }),
            new CardData("Pouch", Slot.Drink, Rarity.Basic, new List<Action> { new Action(ActionType.Draw, 1, TargetType.Player) }),
            new CardData("Tankard", Slot.Drink, Rarity.Basic, new List<Action> {
                new Action(ActionType.Heal, 1, TargetType.None),
                new Action(ActionType.Attack, 1, TargetType.RandomFoe)
            }),
            new CardData("Goblet", Slot.Drink, Rarity.Basic, new List<Action> { new Action(ActionType.Heal, 2, TargetType.Player) }),
            new CardData("Potion", Slot.Drink, Rarity.Uncommon, new List<Action> { new Action(ActionType.Heal, 4, TargetType.Player) }),
            new CardData("Flagon", Slot.Drink, Rarity.Uncommon, new List<Action> {
                new Action(ActionType.Heal, 1, TargetType.Player),
                new Action(ActionType.Poison, 2, TargetType.Foe)
            }),
            new CardData("Chalice", Slot.Drink, Rarity.Uncommon, new List<Action> {
                new Action(ActionType.Heal, 1, TargetType.Player),
                new Action(ActionType.Cleanse, 0, TargetType.Player)
            }),
        };

        return cards;
    }
    #endregion Card Creation

    public void Play(GameObject cardObject)
    {
        Play(cardObject, null);
    }

    public void Play(GameObject cardObject, Enemy targetEnemy)
    {
        CardData cardData = cardObject.GetComponent<CardObject>().CardData;
        ActionManager.instance.PerformActions(cardData.Actions, GameManager.instance.Player, targetEnemy);
        DeckManager.instance.RemoveCard(cardObject);
    }

    private List<Sprite> LoadCardArtSprites()
    {
        List<Sprite> spriteList = new List<Sprite>();

        string cardArtFilePath = "Assets/Resources/Images/CardArt/";
        string[] files = Directory.GetFiles(cardArtFilePath, "*.png", SearchOption.TopDirectoryOnly);

        foreach(var file in files)
        {
            var sprite = AssetDatabase.LoadAssetAtPath(file, typeof(Sprite));

            if(sprite != null)
            {
                spriteList.Add((Sprite)sprite);
            }
            else
            {
                Debug.Log("Error! Sprite not loaded");
            }
        }

        return spriteList;
    }

    public Sprite GetCardArtSprite(string cardName)
    {
        string formattedName = cardName.Replace(" ", "");

        for(int i = 0; i < cardArtList.Count; i++)
        {
            if(cardArtList[i].name == formattedName + "CardArt")
            {
                return cardArtList[i];
            }
        }

        Debug.LogFormat("Error! No art found for {0}", formattedName);
        return null;
    }

    private Rarity GetRandomRarity()
    {
        float currentRarityPercSum = 0f;
        // Get a random float
        float randomF = UnityEngine.Random.Range(0f, 1f);

        for(int i = 0; i < rarityPercentages.Count; i++)
        {
            // Find the next rarity
            KeyValuePair<Rarity, float> currentPair = rarityPercentages.ElementAt(i);
            // Add its rate to a running sum
            // Common -> 0.75 and lower
            // Rare -> 1.0 and lower
            currentRarityPercSum += currentPair.Value;
            // If the random float is less than the running sum, 
            // Return this rarity
            if(randomF <= currentRarityPercSum)
            {
                return currentPair.Key;
            }
        }

        // Display an error and return Common if no rarity was hit
        Debug.Log("Error! Rarity defaulted to Common for random num: $randomF");
        return Rarity.Basic;
    }

    public List<CardData> GetRandomCardDatas(int numOfCardData)
    {
        List<CardData> cardDatas = new List<CardData>();

        while(cardDatas.Count < numOfCardData)
        {
            CardData randomCardData = GetRandomCardData();
            // Ensure the random card has not already been added nor is it currently in the deck
            if(!cardDatas.Contains(randomCardData) && GetCurrentCardData(randomCardData.Slot) != randomCardData)
            {
                cardDatas.Add(randomCardData);
            }
        }

        return cardDatas;
    }

    public CardData GetRandomCardData()
    {
        Slot randomSlot = (Slot)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Slot)).Length);
        return GetRandomCardData(randomSlot);
    }

    public CardData GetRandomCardData(Slot slotType)
    {
        Rarity randomRarity = GetRandomRarity();

        List<CardData> filterList = cardLibrary
            .FindAll(card => card.Slot == slotType)
            .FindAll(card => card.Rarity == randomRarity);
        int randomIndex = UnityEngine.Random.Range(0, filterList.Count);
        return filterList[randomIndex];
    }

    /// <summary>
    /// Gets the CardData of the starting card for a given slot
    /// </summary>
    /// <param name="slotType">The slot type of the card</param>
    /// <returns>Returns the first card of that slot</returns>
    public CardData GetStarterCardData(Slot slotType)
    {
        return cardLibrary
            .FindAll(card => card.Slot == slotType)[0];
    }

    /// <summary>
    /// Gets the object of a Card of the given slot
    /// </summary>
    /// <param name="slotType">The card type</param>
    /// <returns>A Card object, if card exists, otherwise null</returns>
    public CardData GetCurrentCardData(Slot slotType)
    {
        return slotType switch
        {
            Slot.MainHand => mainHand,
            Slot.OffHand => offHand,
            Slot.Ally => ally,
            Slot.Spirit => spirit,
            Slot.Spell => spell,
            Slot.Drink => drink,
            _ => null,
        };
    }

    public Sprite GetCardBaseSprite(Slot slotType, Rarity rarity)
    {
        return (rarity, slotType) switch
        {
            (Rarity.Basic, Slot.MainHand) => cardBaseAttack,
            (Rarity.Basic, Slot.OffHand) => cardBaseDefend,
            (Rarity.Basic, Slot.Ally) => cardBaseAlly,
            (Rarity.Basic, Slot.Spirit) => cardBaseSpirit,
            (Rarity.Basic, Slot.Spell) => cardBaseSpell,
            (Rarity.Basic, Slot.Drink) => cardBaseDrink,
            (Rarity.Uncommon, Slot.MainHand) => cardBaseUncommonAttack,
            (Rarity.Uncommon, Slot.OffHand) => cardBaseUncommonDefend,
            (Rarity.Uncommon, Slot.Ally) => cardBaseUncommonAlly,
            (Rarity.Uncommon, Slot.Spirit) => cardBaseUncommonSpirit,
            (Rarity.Uncommon, Slot.Spell) => cardBaseUncommonSpell,
            (Rarity.Uncommon, Slot.Drink) => cardBaseUncommonDrink,
            (Rarity.Rare, Slot.MainHand) => cardBaseRareAttack,
            (Rarity.Rare, Slot.OffHand) => cardBaseRareDefend,
            (Rarity.Rare, Slot.Ally) => cardBaseRareAlly,
            (Rarity.Rare, Slot.Spirit) => cardBaseRareSpirit,
            (Rarity.Rare, Slot.Spell) => cardBaseRareSpell,
            (Rarity.Rare, Slot.Drink) => cardBaseRareDrink,
            _ => null
        };
    }

    public Sprite GetActionSprite(ActionType actionType)
    {
        return actionType switch
        {
            ActionType.Attack => actionIconSpriteAttack,
            ActionType.Defend => actionIconSpriteDefend,
            ActionType.Heal => actionIconSpriteHeal,
            ActionType.Burn => actionIconSpriteFire,
            ActionType.Poison => actionIconSpritePoison,
            ActionType.Spike => actionIconSpriteSpike,
            ActionType.Summon => actionIconSpriteSummon,
            _ => null
        };
    }

    public void UpdateSlot(CardData newCardData)
    {
        switch(newCardData.Slot)
        {
            case Slot.MainHand:
                mainHand = newCardData;
                break;
            case Slot.OffHand:
                offHand = newCardData;
                break;
            case Slot.Ally:
                ally = newCardData;
                break;
            case Slot.Spirit:
                spirit = newCardData;
                break;
            case Slot.Spell:
                spell = newCardData;
                break;
            case Slot.Drink:
                drink = newCardData;
                break;
        }
    }

    /// <summary>
    /// Reset card slots to starting cards
    /// </summary>
    public void Reset()
    {
        // Setup starter slots
        mainHand = GetStarterCardData(Slot.MainHand);
        offHand = GetStarterCardData(Slot.OffHand);
        ally = GetStarterCardData(Slot.Ally);
        spirit = GetStarterCardData(Slot.Spirit);
        spell = GetStarterCardData(Slot.Spell);
        drink = GetStarterCardData(Slot.Drink);
    }
}