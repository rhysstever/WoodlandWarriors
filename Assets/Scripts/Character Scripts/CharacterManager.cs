using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
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

    // Set at Start
    private Dictionary<string, GameObject> allyPrefabs;
    private Dictionary<string, GameObject> spiritPrefabs;
    private Character chosenCharacter;
    private Ally ally;
    private List<GameObject> summonedSpirits;

    public Character ChosenCharacter { get { return chosenCharacter; } }
    public Ally Ally { get { return ally; } }

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

        allyPrefabs = LoadAllyPrefabs();
        spiritPrefabs = LoadSpiritPrefabs();
    }

    void Start()
    {
        summonedSpirits = new List<GameObject>();
        HideCharacterSelectIcons();
    }

    #region Prefab File Loading
    private Dictionary<string, GameObject> LoadAllyPrefabs()
    {
        // Ally Sprites
        Dictionary<string, GameObject> newAllyPrefabs = new Dictionary<string, GameObject>();

        string allyPrefabFilePath = "Assets/Prefabs/Units/Allies/";
        string[] allyPrefabFiles = Directory.GetFiles(allyPrefabFilePath, "*.prefab", SearchOption.TopDirectoryOnly);

        foreach(var allyPrefabFile in allyPrefabFiles)
        {
            var allyPrefab = AssetDatabase.LoadAssetAtPath(allyPrefabFile, typeof(GameObject));

            if(allyPrefab != null)
            {
                newAllyPrefabs.Add(allyPrefab.name, (GameObject)allyPrefab);
            }
            else
            {
                Debug.Log("Error! Ally Prefab not loaded");
            }
        }

        return newAllyPrefabs;
    }

    private Dictionary<string, GameObject> LoadSpiritPrefabs()
    {
        Dictionary<string, GameObject> newSpiritPrefabs = new Dictionary<string, GameObject>();

        string spiritPrefabFilePath = "Assets/Prefabs/Units/Spirits/";
        string[] spiritPrefabFiles = Directory.GetFiles(spiritPrefabFilePath, "*.prefab", SearchOption.TopDirectoryOnly);

        foreach(var spiritPrefabFile in spiritPrefabFiles)
        {
            var spiritPrefab = AssetDatabase.LoadAssetAtPath(spiritPrefabFile, typeof(GameObject));

            if(spiritPrefab != null)
            {
                newSpiritPrefabs.Add(spiritPrefab.name, (GameObject)spiritPrefab);
            }
            else
            {
                Debug.Log("Error! Ally Prefab not loaded");
            }
        }

        return newSpiritPrefabs;
    }
    #endregion Prefab File Loading

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

    public void SummonAlly(Summon summonAction)
    {
        int amount = summonAction.Amount + GameManager.instance.Player.UnitEffects.GetEffectAmount(ActionType.Summon, true);

        AudioManager.instance.PlayAllyAudio(summonAction.SummonName);
        if(ally != null)
        {
            ally.GetComponent<Ally>().Buff(amount);
        }
        else
        {
            Ally newAlly = Instantiate(
                allyPrefabs[summonAction.SummonName.Replace(" ", "")],
                allySpawnTrans.position,
                Quaternion.identity,
                transform
            ).GetComponent<Ally>();

            newAlly.SetHealth(amount);
            newAlly.SetActions(summonAction.SummonActions);
            ally = newAlly;
        }
    }

    public void SummonSpirit(string spiritTypeToSummon)
    {
        GameObject newSpirit = Instantiate(
            spiritPrefabs[spiritTypeToSummon.Replace(" ", "")],
            spiritSpawnTrans.position,
            Quaternion.identity,
            transform
        );
        summonedSpirits.Add(newSpirit);
    }

    public IEnumerator ProcessAllyTurn()
    {
        WaitForSeconds allyActionDelayWait = new WaitForSeconds(0.5f);

        yield return allyActionDelayWait;
        CardData allyCardToPlay = DeckManager.instance.GetCardDataBySlot(Slot.Ally);

        if(allyCardToPlay != null)
        {
            AudioManager.instance.PlayAllyAudio(allyCardToPlay.Name);
            yield return allyActionDelayWait;
            ActionManager.instance.PerformActions(ally.GetComponent<Ally>().Actions, ally, null);
            yield return allyActionDelayWait;
            yield return allyActionDelayWait;
        }

        if(!EnemyManager.instance.IsWaveOver())
        {
            GameManager.instance.ChangeCombatState(CombatState.EnemyTurn);
        }
    }

    public void ResetSummons()
    {
        if(ally != null)
        {
            Destroy(ally.gameObject);
            ally = null;
        }

        for(int i = summonedSpirits.Count - 1; i >= 0; i--)
        {
            Destroy(summonedSpirits[i]);
        }
        summonedSpirits.Clear();
    }
}
