using System;
using System.Collections;
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
    private GameObject effectUIPrefab;

    // Set in script
    private List<CardData> cardLibrary;
    private Dictionary<Rarity, float> rarityPercentages;
    private List<Sprite> cardArtList;

    private IEnumerator cardPlayCoroutine, cardAttackCoroutine;

    // Slots
    private CardData mainHand, offHand, spirit, ally, spell, drink;

    public GameObject EffectUIPrefab { get { return effectUIPrefab; } }

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
        List<CardData> cards = new() {
            // ===== Description Format =====
            // Normal Attack: "Attack for X"
            // Random Attack: "Attack for X, randomly"
            // AOE Attack: "Attack for X, to all"
            // Multi Attack: "Attack for X, Y times"
            // Heal: "Heal for X"
            // Gain defense: "Defend for X"
            // Burn: "Burn for X"
            // Poison: "Poison for X"
            // Gain Spikes: "Spike for X"
            // Draw Cards: "Draw X cards"   // TODO: implement drawing
            // Cleanse Debuffs: "Cleanse"
            // Buff: "Buff X by Y"
            // Summon: "Summon a [NAME] with X health. ..."

            // Main hand cards
            new CardData("Shortsword", Slot.Physical, Rarity.Basic, TargetType.Unit, "Attack for 1"),
            new CardData("Wand", Slot.Physical, Rarity.Basic, TargetType.None, "Burn 1"),
            new CardData("Staff", Slot.Physical, Rarity.Basic, TargetType.None, "Burn 2"),
            new CardData("Mace", Slot.Physical, Rarity.Basic, TargetType.AOE, "Attack for 3, to all"),
            new CardData("Flail", Slot.Physical, Rarity.Uncommon, TargetType.None, "Attack for 2, randomly, 3 times"),
            new CardData("Flaming Arrow", Slot.Physical, Rarity.Basic, TargetType.Unit, "Attack for 2. Burn for 2"),
            new CardData("Spear", Slot.Physical, Rarity.Uncommon, TargetType.Unit, "Attack for 6"),
            new CardData("Trident", Slot.Physical, Rarity.Uncommon, TargetType.Unit, "Attack for 4. Heal for 4"),
            new CardData("Scythe", Slot.Physical, Rarity.Uncommon, TargetType.Unit, "Attack for 3. Poison for 3"),

            // Defense cards
            new CardData("Wooden Shield", Slot.Defense, Rarity.Basic, TargetType.Self, "Defend for 1"),
            new CardData("Buckler", Slot.Defense, Rarity.Basic, TargetType.Self, "Defend for 2"),
            new CardData("Dagger", Slot.Defense, Rarity.Basic, TargetType.Unit, "Attack for 1"),
            new CardData("Quiver", Slot.Defense, Rarity.Basic, TargetType.Unit, "Attack for 1, randomly"),
            //new CardData("Scroll", Slot.OffHand, Rarity.Common, TargetType.Self, "Some magic... nothing yet"),
            new CardData("Spike Shield", Slot.Defense, Rarity.Basic, TargetType.Self, "Defend for 3. Spike for 2"),
            //new CardData("Tome", Slot.OffHand, Rarity.Common, TargetType.Self, "Some magic... nothing yet"),
            new CardData("Tower Shield", Slot.Defense, Rarity.Uncommon, TargetType.Self, "Defend for 5"),
            //new CardData("Arcane Focus", Slot.OffHand, Rarity.Uncommon, TargetType.Self, "Some magic... nothing yet"),

            // Ally cards
            new CardData("Squirrel", Slot.Ally, Rarity.Basic, TargetType.Self, "Summon a Squirrel with 1 health. Attacks for 1"),
            new CardData("Frog", Slot.Ally, Rarity.Basic, TargetType.Self, "Summon a Frog with 1 health. Heals for 1"),
            new CardData("Rat", Slot.Ally, Rarity.Basic, TargetType.Self, "Summon a Rat with 1 health. Poison for 1"),
            new CardData("Newt", Slot.Ally, Rarity.Basic, TargetType.Self, "Summon a Newt with 1 health. Burn for 1"),
            //new CardData("Bunny", Slot.Ally, Rarity.Rare, TargetType.Self, "Summon 1. Some magic... nothing yet"),
            new CardData("Toad", Slot.Ally, Rarity.Uncommon, TargetType.Self, "Summon a Toad with 1 health. Heal for 1. Poison for 1"),
            new CardData("Porcupine", Slot.Ally, Rarity.Uncommon, TargetType.Self, "Summon a Porcupine with 1 health. Spike for 1"),
            new CardData("Hamster", Slot.Ally, Rarity.Uncommon, TargetType.Self, "Summon a Hamster with 1 health. Draw 1 card"),

            // Spirit cards
            new CardData("Earth Spirit", Slot.Spirit, Rarity.Basic, TargetType.Self, "Buff Defense by 1"),
            new CardData("Air Spirit", Slot.Spirit, Rarity.Basic, TargetType.None, "Buff Attacks by 1"),
            new CardData("Fire Spirit", Slot.Spirit, Rarity.Basic, TargetType.Unit, "Buff Burn by 1"),
            new CardData("Water Spirit", Slot.Spirit, Rarity.Basic, TargetType.Self, "Buff Poison by 1"),
            new CardData("Dark Spirit", Slot.Spirit, Rarity.Uncommon, TargetType.Unit, "Buff Spike by 1"),
            new CardData("Light Spirit", Slot.Spirit, Rarity.Uncommon, TargetType.Unit, "Buff Healing by 1"),

            // Spell cards
            new CardData("Arcane Bolt", Slot.Magical, Rarity.Basic, TargetType.None, "Attack for 1, randomly"),
            new CardData("Fireball", Slot.Magical, Rarity.Basic, TargetType.Unit, "Burn for 3"),
            new CardData("Life Drain", Slot.Magical, Rarity.Basic, TargetType.Unit, "Attack for 2. Heal for 2"),
            new CardData("Lightning Strike", Slot.Magical, Rarity.Uncommon, TargetType.None, "Attack for 4, randomly"),
            new CardData("Heal", Slot.Magical, Rarity.Uncommon, TargetType.Self, "Heal for 5"),
            new CardData("Blizzard", Slot.Magical, Rarity.Uncommon, TargetType.AOE, "Attack for 3, to all"),
            new CardData("Curse", Slot.Magical, Rarity.Uncommon, TargetType.Unit, "Poison for 5"),

            // Drink cards
            new CardData("Cup", Slot.Drink, Rarity.Basic, TargetType.Self, "Heal for 1"),
            new CardData("Pouch", Slot.Drink, Rarity.Basic, TargetType.Self, "Draw 1 card"),
            new CardData("Tankard", Slot.Drink, Rarity.Basic, TargetType.None, "Heal for 1. Attack for 1, randomly"),
            new CardData("Goblet", Slot.Drink, Rarity.Basic, TargetType.Self, "Heal for 2"),
            new CardData("Potion", Slot.Drink, Rarity.Uncommon, TargetType.Self, "Heal for 4"),
            new CardData("Flagon", Slot.Drink, Rarity.Uncommon, TargetType.Unit, "Heal for 1. Poison for 2"),
            new CardData("Chalice", Slot.Drink, Rarity.Uncommon, TargetType.Self, "Heal for 1. Cleanse")
        };

        return cards;
    }
    #endregion Card Creation

    #region Card Actions
    public void Play(int cardIndex)
    {
        Play(cardIndex, null);
    }

    public void Play(int cardIndex, Enemy targetEnemy)
    {
        cardPlayCoroutine = ProcessCard(cardIndex, targetEnemy);
        StartCoroutine(cardPlayCoroutine);
    }

    private IEnumerator ProcessCard(int cardIndex, Enemy targetEnemy)
    {
        CardData cardData = DeckManager.instance.GetCardDataAtIndex(cardIndex);

        WaitForSeconds actionDelayWait = new WaitForSeconds(0.5f);
        List<string> actions = cardData.Description.Split(". ").ToList();
        int actionIndex = 0;
        // Apply a small delay before performing each action
        while(actionIndex < actions.Count)
        {
            yield return actionDelayWait;
            PerformCardAction(actions[actionIndex], targetEnemy, cardData.Slot);
            actionIndex++;

            if(cardData.Slot == Slot.Ally)
            {
                break;
            } 
        }

        // Remove card and reset targetting
        DeckManager.instance.RemoveCard(cardIndex);
        TargettingManager.instance.Reset();
    }

    private void PerformCardAction(string action, Enemy target, Slot slot)
    {
        string firstWord = action.Split(" ")[0];
        int amount;

        switch(firstWord)
        {
            case "Attack":
                ParseAttack(action, target, slot);
                break;
            case "Defend":
                amount = int.Parse(action.Split(" ")[2]);
                GameManager.instance.Player.GiveDefense(amount);
                break;
            case "Heal":
                amount = int.Parse(action.Split(" ")[2]);
                GameManager.instance.Player.Heal(amount);
                break;
            case "Burn":
                amount = int.Parse(action.Split(" ")[2]);
                if(CharacterManager.instance.ChosenCharacter == Character.Skunk)
                {
                    amount++;
                }
                target.GiveBurn(amount);
                break;
            case "Poison":
                amount = int.Parse(action.Split(" ")[2]);
                if(CharacterManager.instance.ChosenCharacter == Character.Skunk)
                {
                    amount++;
                }
                target.GivePoison(amount);
                break;
            case "Spike":
                amount = int.Parse(action.Split(" ")[2]);
                GameManager.instance.Player.GiveSpike(amount);
                break;
            case "Draw":
                amount = int.Parse(action.Split(" ")[1]);
                DeckManager.instance.DrawCards(amount);
                break;
            case "Cleanse":
                GameManager.instance.Player.Cleanse();
                break;
            case "Buff":
                string spiritName = GetCurrentCardData(Slot.Spirit).Name.Split(' ')[0];
                CharacterManager.instance.SummonSpirit(spiritName);
                amount = int.Parse(action.Split(" ")[3]);
                string type = action.Split(" ")[1];
                ParseBuff(amount, type);
                break;
            case "Summon":
                string[] trimmedAction = action.Split(".")[0].Split(" ");
                string allyName = trimmedAction[2];
                int allyHealth = int.Parse(trimmedAction[4]);
                if(CharacterManager.instance.ChosenCharacter == Character.Opossum)
                {
                    allyHealth++;
                }
                CharacterManager.instance.SummonAlly(allyName, allyHealth);
                break;
            default:
                Debug.Log(string.Format("Error! No action found for: {0}", action));
                break;
        }
    }

    private void ParseAttack(string action, Enemy target, Slot slot)
    {
        string[] attackParts = action.Split(", ");

        // Figure out if the attack is AOE, random, and/or multi
        bool isAttackAOE = false;
        bool isAttackRandom = false;
        int attackCount = 1;
        for(int i = 1; i < attackParts.Length; i++)
        {
            if(attackParts[i].Contains("to all"))
            {
                isAttackAOE = true;
            }
            else if(attackParts[i].Contains("randomly"))
            {
                isAttackRandom = true;
            }
            else if(attackParts[i].Contains("times"))
            {
                attackCount = int.Parse(attackParts[i].Split(" ")[0]);
            }
        }

        int amount = int.Parse(attackParts[0].Split(" ")[2]);

        if(CharacterManager.instance.ChosenCharacter == Character.Badger && slot == Slot.Physical)
        {
            amount++;
        }
        else if(CharacterManager.instance.ChosenCharacter == Character.Fox && slot == Slot.Magical)
        {
            amount++;
        }

        cardAttackCoroutine = ProcessCardAttack(amount, target, slot, attackCount, isAttackAOE, isAttackRandom);
        StartCoroutine(cardAttackCoroutine);
    }

    private void ParseBuff(int amount, string type)
    {
        switch(type)
        {
            case "Attacks":
                GameManager.instance.Player.BuffAttack(amount);
                break;
            case "Defense":
                GameManager.instance.Player.BuffDefense(amount);
                break;
            case "Healing":
                GameManager.instance.Player.BuffHealing(amount);
                break;
            case "Burn":
                GameManager.instance.Player.BuffBurn(amount);
                break;
            case "Poison":
                GameManager.instance.Player.BuffPoison(amount);
                break;
            case "Spike":
                GameManager.instance.Player.BuffSpike(amount);
                break;
            default:
                Debug.Log(string.Format("Error! No buff found for: {0}", type));
                break;
        }
    }

    private IEnumerator ProcessCardAttack(int amount, Enemy target, Slot slot, int attackCount, bool isAOE, bool isRandom)
    {
        WaitForSeconds attackDelayWait = new WaitForSeconds(0.75f);
        int attackIndex = 0;
        // Apply a small delay before performing each attack
        while(attackIndex < attackCount)
        {
            if(attackIndex > 0)
            {
                yield return attackDelayWait;
            }
            AudioManager.instance.PlaySlotAttackAudio(slot);
            if(isAOE)
            {
                AttackEveryEnemy(amount);
            }
            else if(isRandom)
            {
                AttackRandomEnemy(amount);
            }
            else
            {
                AttackSingleEnemy(amount, target);
            }
            attackIndex++;
        }
    }

    private void AttackEveryEnemy(int damage)
    {
        EnemyManager.instance.GetCurrentEnemies().ForEach(enemy => {
            AttackSingleEnemy(damage, enemy);
        });
    }

    private void AttackRandomEnemy(int damage)
    {
        List<Enemy> currentEnemies = EnemyManager.instance.GetCurrentEnemies();
        int randomEnemyIndex = UnityEngine.Random.Range(0, currentEnemies.Count);
        AttackSingleEnemy(damage, currentEnemies[randomEnemyIndex]);
    }

    private void AttackSingleEnemy(int amount, Enemy enemy)
    {
        if(enemy == null)
        {
            Debug.Log("Error: No target to attack!");
            return;
        }

        if(amount < 1)
        {
            Debug.Log(string.Format("Error: Not enough damage ({0})", amount));
            return;
        }

        enemy.TakeDamage(amount, GameManager.instance.Player, DamageType.Attack);
    }
    #endregion Card Actions

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
            Slot.Physical => mainHand,
            Slot.Defense => offHand,
            Slot.Ally => ally,
            Slot.Spirit => spirit,
            Slot.Magical => spell,
            Slot.Drink => drink,
            _ => null,
        };
    }

    public Sprite GetCardBaseSprite(Slot slotType, Rarity rarity)
    {
        return (rarity, slotType) switch
        {
            (Rarity.Basic, Slot.Physical) => cardBaseAttack,
            (Rarity.Basic, Slot.Defense) => cardBaseDefend,
            (Rarity.Basic, Slot.Ally) => cardBaseAlly,
            (Rarity.Basic, Slot.Spirit) => cardBaseSpirit,
            (Rarity.Basic, Slot.Magical) => cardBaseSpell,
            (Rarity.Basic, Slot.Drink) => cardBaseDrink,
            (Rarity.Uncommon, Slot.Physical) => cardBaseUncommonAttack,
            (Rarity.Uncommon, Slot.Defense) => cardBaseUncommonDefend,
            (Rarity.Uncommon, Slot.Ally) => cardBaseUncommonAlly,
            (Rarity.Uncommon, Slot.Spirit) => cardBaseUncommonSpirit,
            (Rarity.Uncommon, Slot.Magical) => cardBaseUncommonSpell,
            (Rarity.Uncommon, Slot.Drink) => cardBaseUncommonDrink,
            (Rarity.Rare, Slot.Physical) => cardBaseRareAttack,
            (Rarity.Rare, Slot.Defense) => cardBaseRareDefend,
            (Rarity.Rare, Slot.Ally) => cardBaseRareAlly,
            (Rarity.Rare, Slot.Spirit) => cardBaseRareSpirit,
            (Rarity.Rare, Slot.Magical) => cardBaseRareSpell,
            (Rarity.Rare, Slot.Drink) => cardBaseRareDrink,
            _ => null
        };
    }

    public Sprite GetActionSprite(string actionType)
    {
        return actionType.ToLower() switch
        {
            "attack" => actionIconSpriteAttack,
            "defend" => actionIconSpriteDefend,
            "heal" => actionIconSpriteHeal,
            "burn" => actionIconSpriteFire,
            "poison" => actionIconSpritePoison,
            "spike" => actionIconSpriteSpike,
            "summon" => actionIconSpriteSummon,
            _ => null
        };
    }

    public void UpdateSlot(CardData newCardData)
    {
        switch(newCardData.Slot)
        {
            case Slot.Physical:
                mainHand = newCardData;
                break;
            case Slot.Defense:
                offHand = newCardData;
                break;
            case Slot.Ally:
                ally = newCardData;
                break;
            case Slot.Spirit:
                spirit = newCardData;
                break;
            case Slot.Magical:
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
        mainHand = GetStarterCardData(Slot.Physical);
        offHand = GetStarterCardData(Slot.Defense);
        ally = GetStarterCardData(Slot.Ally);
        spirit = GetStarterCardData(Slot.Spirit);
        spell = GetStarterCardData(Slot.Magical);
        drink = GetStarterCardData(Slot.Drink);
    }
}