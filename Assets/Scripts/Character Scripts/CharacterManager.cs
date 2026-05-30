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
    private Transform allySpawnTrans;
    [SerializeField]    // Character Sprites
    private Sprite badgerSprite, beaverSprite, foxSprite, opossumSprite, otterSprite, skunkSprite;
    [SerializeField]    // Ally Prefabs
    private GameObject squirrelPrefab;  // TODO: add the rest
    [SerializeField]    // Spirit Prefabs
    private GameObject earthSpiritPrefab;  // TODO: add the rest

    // Set at Start
    private Character chosenCharacter;
    private GameObject allyObject, spiritObject;

    public Character ChosenCharacter { get { return chosenCharacter; } }
    public GameObject AllyObject { get { return allyObject; } }
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
        allyObject = null;
        spiritObject = null;
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

    public int GetSlotCardCountOfChosenCharacter(Slot slotType)
    {
        return slotType switch
        {
            Slot.MainHand => GetCharacterDeckStructure(chosenCharacter)[0],
            Slot.OffHand => GetCharacterDeckStructure(chosenCharacter)[1],
            Slot.Ally => GetCharacterDeckStructure(chosenCharacter)[2],
            Slot.Spirit => GetCharacterDeckStructure(chosenCharacter)[3],
            Slot.Spell => GetCharacterDeckStructure(chosenCharacter)[4],
            Slot.Drink => GetCharacterDeckStructure(chosenCharacter)[5],
            _ => 0,
        };
    }

    public int[] GetCharacterDeckStructure(Character character)
    {
        return character switch
        {
            // Slot order:
            // Main Hand, Off Hand, Ally, Spirit, Spell, Drink
            Character.Badger => new int[] { 
                4, 4, 3, 3, 2, 2 
            },
            Character.Beaver => new int[] { 
                3, 4, 3, 2, 2, 4 
            },
            Character.Fox => new int[] { 
                3, 2, 2, 4, 4, 3 
            },
            Character.Opossum => new int[] { 
                2, 3, 4, 3, 4, 2
            },
            Character.Otter => new int[] { 
                4, 2, 2, 4, 3, 3 
            },
            Character.Skunk => new int[] { 
                2, 3, 4, 2, 3, 4 
            },
            _ => null,
        };
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
            Character.Badger => "Balanced deck with both attacking and defending",
            Character.Beaver => "Defensive deck that outlasts enemies with blocking and healing.",
            Character.Fox => "Technical deck with a focus on spells and buffs.",
            Character.Opossum => "Mixed deck using allies and spells.",
            Character.Otter => "Aggressive deck with many buffs and attacks.",
            Character.Skunk => "Intricate deck with allies and healing.",
            _ => "",
        };
    }

    private GameObject GetAllyPrefab(string allyType)
    {
        return allyType switch
        {
            "Squirrel" => squirrelPrefab,
            _ => squirrelPrefab,
        };
    }

    public void SummonAlly(string allyTypeToSummon, int initialHealth)
    {
        if(allyObject != null)
        {
            allyObject.GetComponent<Ally>().Buff(initialHealth);
        }
        else
        {
            Ally newAlly = Instantiate(
                GetAllyPrefab(allyTypeToSummon),
                allySpawnTrans.position,
                Quaternion.identity,
                GameManager.instance.Player.transform
            ).GetComponent<Ally>();

            newAlly.SetHealth(initialHealth);
            allyObject = newAlly.gameObject;
        }
    }
}
