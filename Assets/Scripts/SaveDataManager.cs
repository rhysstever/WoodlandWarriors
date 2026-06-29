using System;
using System.IO;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    // Singleton
    public static SaveDataManager instance;

    private string saveDataDirPath;
    private string saveDataFileName;
    private bool hasSaveData;

    public bool HasSaveData { get { return hasSaveData; } } 

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

    private void Start()
    {
        saveDataDirPath = "Assets/Saves";
        saveDataFileName = "saveData.game";
        hasSaveData = false;
    }

    public void SaveGame()
    {
        // JSON structure
        // list of runs
        // [
        //  {
        //      "date": {YYYY-MM-DD},
        //      "progress": #-# or WIN (if won run)
        //      "character": {NAME OF CHARACTER},
        //      "mainHand": {NAME OF ATTACK CARD},
        //      "offHand": {NAME OF DEFEND CARD},
        //      "ally": {NAME OF ALLY CARD},
        //      "spell": {NAME OF SPELL CARD},
        //      "spirit": {NAME OF SPIRIT CARD},
        //      "drink": {NAME OF DRINK CARD},
        //  } 
        // ]
        SaveDataObject saveData = new SaveDataObject();

        // Current date and time
        DateTime today = DateTime.Now;
        saveData.date = string.Format("{0}/{1}/{2} {3}:{4}", today.Year, today.Month, today.Day, today.Hour, today.Minute);
        // Current progress
        saveData.progress = GameManager.instance.GetCurrentStageText();
        // Deck info
        saveData.character = CharacterManager.instance.ChosenCharacter.ToString();
        saveData.mainHand = DeckManager.instance.GetCardDataBySlot(Slot.MainHand).Name;
        saveData.offHand = DeckManager.instance.GetCardDataBySlot(Slot.OffHand).Name;
        saveData.ally = DeckManager.instance.GetCardDataBySlot(Slot.Ally).Name;
        saveData.spell = DeckManager.instance.GetCardDataBySlot(Slot.Spell).Name;
        saveData.spirit = DeckManager.instance.GetCardDataBySlot(Slot.Spirit).Name;
        saveData.drink = DeckManager.instance.GetCardDataBySlot(Slot.Drink).Name;

        CreateSave(saveData);
    }

    private void CreateSave(SaveDataObject saveData)
    {
        string fullPath = Path.Combine(saveDataDirPath, saveDataFileName);
        try
        {
            if(!Directory.Exists(saveDataDirPath))
            {
                Directory.CreateDirectory(saveDataDirPath);
            }

            string saveDataJSON = JsonUtility.ToJson(saveData);

            FileMode mode = FileMode.Create;
            if(hasSaveData)
            {
                mode = FileMode.Append;
            }
            using(var stream = File.Open(fullPath, mode))
            {
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine(saveDataJSON);
                }
            }

            hasSaveData = true;
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("Error! Failed to save data to file: {0}\n {1}", fullPath, e));
        }
    }

    public SaveDataObject LoadGame()
    {
        string fullPath = Path.Combine(saveDataDirPath, saveDataFileName);
        SaveDataObject saveData = null;
        try
        {
            string readData = "";
            using(FileStream stream = new FileStream(fullPath, FileMode.Open))
            {
                using(StreamReader reader = new StreamReader(stream))
                {
                    readData = reader.ReadToEnd();
                }
            }
            saveData = JsonUtility.FromJson<SaveDataObject>(readData);
        } 
        catch(Exception e)
        {
            Debug.LogError(string.Format("Error! Failed to load data from file: {0}\n {1}", fullPath, e));
        }

        return saveData;
    }
}
