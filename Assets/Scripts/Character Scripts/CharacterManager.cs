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
    private GameObject squirrelPrefab, frogPrefab, ratPrefab, newtPrefab, toadPrefab, porcupinePrefab, hamsterPrefab;  // TODO: add the rest
    [SerializeField]    // Spirit Prefabs
    private GameObject earthSpiritPrefab, airSpiritPrefab, fireSpiritPrefab, waterSpiritPrefab, darkSpiritPrefab, lightSpiritPrefab;  // TODO: add the rest

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
}
