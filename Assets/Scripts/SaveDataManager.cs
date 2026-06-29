using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    // Singleton
    public static SaveDataManager instance;

    private string saveDataDirPath, saveDataDirName, saveDataFileName, saveDataFullPath;
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
        saveDataDirPath = "Assets";
        saveDataDirName = "Saves";
        saveDataFileName = "saveData.txt";
        saveDataFullPath = Path.Combine(saveDataDirPath, saveDataDirName, saveDataFileName);
        hasSaveData = CheckForSaveData();
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

    private bool CheckForSaveData()
    {
        try
        {
            return Directory.Exists(Path.Combine(saveDataDirPath, saveDataDirName));
        } 
        catch(Exception e)
        {
            Debug.LogError(string.Format("Error! Failed to check for save data: {0}\n {1}", saveDataFullPath, e));
            return false;
        }
    }

    private void CreateSave(SaveDataObject saveData)
    {
        try
        {
            if(!CheckForSaveData())
            {
                Directory.CreateDirectory(saveDataDirPath);
            }

            string saveDataJSON = JsonUtility.ToJson(saveData);

            FileMode mode = FileMode.Create;
            if(hasSaveData)
            {
                mode = FileMode.Append;
            }
            using(var stream = new FileStream(saveDataFullPath, mode))
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
            Debug.LogError(string.Format("Error! Failed to save data to file: {0}\n {1}", saveDataFullPath, e));
        }
    }

    public List<SaveDataObject> LoadRunInfo()
    {
        List<SaveDataObject> saveData = new List<SaveDataObject>();
        try
        {
            string readLine = "";
            using(var stream = new FileStream(saveDataFullPath, FileMode.Open))
            {
                using(StreamReader reader = new StreamReader(stream))
                {
                    while((readLine = reader.ReadLine()) != null)
                    {
                        saveData.Add(JsonUtility.FromJson<SaveDataObject>(readLine));
                    }
                }
            }

        } 
        catch(Exception e)
        {
            Debug.LogError(string.Format("Error! Failed to load data from file: {0}\n {1}", saveDataFullPath, e));
        }

        return saveData;
    }
}
