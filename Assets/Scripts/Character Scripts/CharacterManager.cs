using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Character
{
    Badger,
    Beaver,
    Fox,
    Opossum,
    Otter,
    Skunk
}

public class CharacterManager : MonoBehaviour
{
    // Singleton
    public static CharacterManager instance = null;

    // Set in inspector
    [SerializeField]
    private Transform characterSelectIconParent;
    [SerializeField]
    private SpriteRenderer characterSelectSprite;
    [SerializeField]
    private Transform allySpawnTrans, spiritSpawnTrans;
    [SerializeField]    // Character Sprites
    private Sprite badgerSprite, beaverSprite, foxSprite, opossumSprite, otterSprite, skunkSprite;
    [SerializeField]    // Ally Prefabs
    private GameObject squirrelPrefab, frogPrefab, ratPrefab, newtPrefab, toadPrefab, porcupinePrefab, hamsterPrefab;
    [SerializeField]    // Spirit Prefabs
    private GameObject earthSpiritPrefab, airSpiritPrefab, fireSpiritPrefab, waterSpiritPrefab, darkSpiritPrefab, lightSpiritPrefab;  // TODO: add the rest

    // Set at Start
    private Character chosenCharacter;
    private Ally ally;
    private GameObject spiritObject;

    public Character ChosenCharacter { get { return chosenCharacter; } }
    public Ally Ally { get { return ally; } }
    public GameObject SpiritObject { get { return spiritObject; } }

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
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HideCharacterSelectIcons();
    }

    public void ShowCharacterSelectIcons()
    {
        characterSelectIconParent.gameObject.SetActive(true);
    }

    public void HideCharacterSelectIcons()
    {
        characterSelectIconParent.gameObject.SetActive(false);
    }

    public void ChooseCharacter(Character character)
    {
        chosenCharacter = character;
        ResetSummons();
        HideCharacterSelectIcons();
        ClearCharacterSelectInfo();
        GameManager.instance.StartGame();
    }

    public void ClearCharacterSelectInfo()
    {
        characterSelectSprite.gameObject.SetActive(false);
        UIManager.instance.UpdateCharacterSelectInfo();
    }

    public void SetCharacterSelectInfo(Character character)
    {
        // Update character sprite
        characterSelectSprite.gameObject.SetActive(true);
        characterSelectSprite.sprite = GetCharacterSprite(character);
        UIManager.instance.UpdateCharacterSelectInfo(character);
    }

    public Sprite GetCharacterSprite(Character character)
    {
        return character switch
        {
            Character.Badger => badgerSprite,
            Character.Beaver => beaverSprite,
            Character.Fox => foxSprite,
            Character.Opossum => opossumSprite,
            Character.Otter => otterSprite,
            Character.Skunk => skunkSprite,
            _ => null,
        };
    }

    public string GetCharacterDeckDescription(Character character)
    {
        return character switch
        {
            Character.Badger => "Physical attacks deal more damage",
            Character.Beaver => "Starts with higher max health",
            Character.Fox => "Spells deal more damage",
            Character.Opossum => "Summons are tougher",
            Character.Otter => "Cards draw with an equal chance",
            Character.Skunk => "Damaging effects linger on enemies longer",
            _ => string.Format("Error! No deck description for {0} character", character),
        };
    }

    private GameObject GetAllyPrefab(string allyType)
    {
        return allyType.ToLower() switch
        {
            "squirrel" => squirrelPrefab,
            "frog" => frogPrefab,
            "rat" => ratPrefab,
            "newt" => newtPrefab,
            "toad" => toadPrefab,
            "porcupine" => porcupinePrefab,
            "hamster" => hamsterPrefab,
            _ => squirrelPrefab,
        };
    }

    private GameObject GetSpiritPrefab(string spiritType)
    {
        return spiritType.ToLower() switch
        {
            "earth" => earthSpiritPrefab,
            "air" => airSpiritPrefab,
            "fire" => fireSpiritPrefab,
            "water" => waterSpiritPrefab,
            "dark" => darkSpiritPrefab,
            "light" => lightSpiritPrefab,
            _ => earthSpiritPrefab,
        };
    }

    public void SummonAlly(Summon summonAction)
    {
        int amount = summonAction.Amount;
        if(ChosenCharacter == Character.Opossum)
        {
            amount++;
        }

        if(ally != null)
        {
            ally.GetComponent<Ally>().Buff(amount);
        }
        else
        {
            Ally newAlly = Instantiate(
                GetAllyPrefab(summonAction.SummonName),
                allySpawnTrans.position,
                Quaternion.identity,
                GameManager.instance.Player.transform
            ).GetComponent<Ally>();

            newAlly.SetHealth(amount);
            newAlly.SetActions(summonAction.SummonActions);
            ally = newAlly;
        }
    }

    public void SummonSpirit(string spiritTypeToSummon)
    {
        if(spiritObject != null)
        {
            // TODO: buff the current spirit
        }
        else
        {
            GameObject newSpirit = Instantiate(
                GetSpiritPrefab(spiritTypeToSummon),
                spiritSpawnTrans.position,
                Quaternion.identity,
                GameManager.instance.Player.transform
            );

            spiritObject = newSpirit;
        }
    }

    public IEnumerator ProcessAllyTurn()
    {
        WaitForSeconds allyActionDelayWait = new WaitForSeconds(1);

        yield return allyActionDelayWait;
        CardData allyCardToPlay = DeckManager.instance.GetCardDataBySlot(Slot.Ally);

        if(allyCardToPlay != null)
        {
            // TODO: Check actor and target 
            ActionManager.instance.PerformActions(ally.GetComponent<Ally>().Actions, ally, null);
            yield return allyActionDelayWait;
            yield return allyActionDelayWait;
        }

        GameManager.instance.ChangeCombatState(CombatState.EnemyTurn);
    }

    public void ResetSummons()
    {
        if(ally != null)
        {
            Destroy(ally.gameObject);
            ally = null;
        }

        if(spiritObject != null)
        {
            Destroy(spiritObject);
            spiritObject = null;
        }
    }
}
