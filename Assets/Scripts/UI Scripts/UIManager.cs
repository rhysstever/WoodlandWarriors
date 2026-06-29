using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Singleton
    public static UIManager instance;

    // Set in inspector
    [SerializeField]    // MenuState UI Parents
    private GameObject mainMenuUIParent, statsParent, characterSelectUIParent, gameUIParent, gameEndUIParent;
    [SerializeField]    // Sub-menu UI Parents
    private GameObject mainMenuButtonsParent, statsTextParent, combatUIParent, nonCombatUIParent, cardSelectionUIParent, wellUIParent;
    [SerializeField]    // Game Sub-menu UI Parents
    private GameObject gameInfoUIParent, viewDeckUIParent, viewDeckCardsUIParent;
    [SerializeField]    // Turn Banners
    private GameObject playerTurnBanner, enemyTurnBanner;
    [SerializeField]    // Buttons
    private Button mainMenuToCharacterSelectButton, mainMenuToStatsButton, quitButton,  // Main Menu Buttons
        statsToMainMenuButton,                                                          // Stats Buttons
        gameInfoButton, closeGameInfoButton, viewDeckButton, closeViewDeckButton,       // Game Top Buttons
        endTurnButton, selectCardButton, skipButton, drinkWellButton,                   // Game Buttons
        gameEndToMainMenuButton;                                                        // GameEnd Buttons
    [SerializeField]    // Texts
    private TMP_Text characterSelectInfoText, gameAreaStageText, gameEndHeaderText, gameEndDeckInfoText;
    [SerializeField]
    private GameObject statsTextPrefab;

    private bool isSubMenuShowing;
    private float turnBannerVisibleTime;

    public bool IsSubMenuShowing { get { return isSubMenuShowing; } }
    public float TurnBannerVisibleTime { get { return turnBannerVisibleTime; } }

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

    void Start()
    {
        turnBannerVisibleTime = 1.5f;
        Reset();

        // Set up button listeners
        mainMenuToCharacterSelectButton.onClick.AddListener(() => {
            mainMenuButtonsParent.SetActive(false);
            Camera.main.GetComponent<CameraPan>().PanCameraDown();
        });
        mainMenuToStatsButton.onClick.AddListener(() => GameManager.instance.ChangeMenuState(MenuState.Stats));
        quitButton.onClick.AddListener(() => Application.Quit());
        statsToMainMenuButton.onClick.AddListener(() => GameManager.instance.ChangeMenuState(MenuState.MainMenu));
        gameInfoButton.onClick.AddListener(() => ShowGameInfo());
        closeGameInfoButton.onClick.AddListener(() => HideGameInfo());
        viewDeckButton.onClick.AddListener(() => ShowDeckInfo());
        closeViewDeckButton.onClick.AddListener(() => HideDeckInfo());
        endTurnButton.onClick.AddListener(() => {
            endTurnButton.gameObject.SetActive(false);
            GameManager.instance.ChangeCombatState(CombatState.AllyTurn);
        });
        selectCardButton.onClick.AddListener(() => {
            DeckManager.instance.AddSelectedCardToDeck();
            GameManager.instance.GoToNextStage();
        });
        skipButton.onClick.AddListener(() => {
            DeckManager.instance.ClearCardSelectionDisplayCards();
            GameManager.instance.GoToNextStage();
        });
        drinkWellButton.onClick.AddListener(() => {
            GameManager.instance.Player.HealFull();
            GameManager.instance.GoToNextStage();
        });
        gameEndToMainMenuButton.onClick.AddListener(() => GameManager.instance.ChangeMenuState(MenuState.MainMenu));
    }

    public void UpdateMenuUI(MenuState menuState)
    {
        // Hide all parents, then show the ui parent based on the MenuState
        mainMenuUIParent.SetActive(false);
        statsParent.SetActive(false);
        characterSelectUIParent.SetActive(false);
        gameUIParent.SetActive(false);
        gameEndUIParent.SetActive(false);

        switch(menuState)
        {
            case MenuState.MainMenu:
                mainMenuUIParent.SetActive(true);
                mainMenuButtonsParent.SetActive(true);
                mainMenuToStatsButton.interactable = SaveDataManager.instance.HasSaveData;
                break;
            case MenuState.Stats:
                statsParent.SetActive(true);
                UpdateRunHistory();
                break;
            case MenuState.CharacterSelect:
                characterSelectUIParent.SetActive(true);
                UpdateCharacterSelectInfo();
                break;
            case MenuState.Game:
                gameUIParent.SetActive(true);
                HideGameInfo();
                HideDeckInfo();
                break;
            case MenuState.GameEnd:
                gameEndUIParent.SetActive(true);
                UpdateGameEndText();
                break;
        }
    }

    public void UpdateGameUI(GameState gameState)
    {
        switch(gameState)
        {
            case GameState.Combat:
                combatUIParent.SetActive(true);
                nonCombatUIParent.SetActive(false);

                endTurnButton.gameObject.SetActive(false);
                break;
            case GameState.CardSelection:
                nonCombatUIParent.SetActive(true);
                cardSelectionUIParent.SetActive(true);
                wellUIParent.SetActive(false);

                endTurnButton.gameObject.SetActive(false);
                break;
            case GameState.Well:
                nonCombatUIParent.SetActive(true);
                cardSelectionUIParent.SetActive(false);
                wellUIParent.SetActive(true);
                break;
            case GameState.None:
                combatUIParent.SetActive(false);
                nonCombatUIParent.SetActive(false);
                break;
        }
    }

    private void Reset()
    {
        TogglePlayerTurnBanner(false);
        ToggleEnemyTurnBanner(false);
        endTurnButton.interactable = false;
    }

    public void ShowEndTurnButton()
    {
        endTurnButton.gameObject.SetActive(true);
    }

    public void TogglePlayerTurnBanner(bool isActive)
    {
        playerTurnBanner.SetActive(isActive);
    }

    public void ToggleEnemyTurnBanner(bool isActive)
    {
        enemyTurnBanner.SetActive(isActive);
    }

    public void UpdateRunHistory()
    {
        // Destroy all current text children
        for(int i = statsTextParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(statsTextParent.transform.GetChild(i).gameObject);
        }

        // Load run history data
        List<SaveDataObject> runHistoryList = SaveDataManager.instance.LoadRunInfo();

        // Loop thr each data object 
        runHistoryList.ForEach(runData => {
            // Construct a string using the run data 
            string displayString = string.Format(
                "{0} {1}: {2}\n    {3}, {4}, {5}, {6}, {7}, {8}\n",
                runData.date, runData.character, runData.progress,
                runData.mainHand, runData.offHand, runData.ally, runData.spell, runData.spirit, runData.drink);

            // Spawn a text prefab
            Instantiate(statsTextPrefab, statsTextParent.transform).GetComponent<TMP_Text>().text = displayString;
        });
    }

    public void UpdateCharacterSelectInfo()
    {
        characterSelectInfoText.text = "Choose Your Warrior";
        characterSelectInfoText.verticalAlignment = VerticalAlignmentOptions.Middle;
    }

    public void UpdateCharacterSelectInfo(Character character)
    {
        characterSelectInfoText.text = string.Format(
            "{0}\n\n{1}",
            character.ToString(),
            CharacterManager.instance.GetCharacterDeckDescription(character));
            
        characterSelectInfoText.verticalAlignment = VerticalAlignmentOptions.Top;
    }

    public void UpdateStageText()
    {
        gameAreaStageText.text = GameManager.instance.GetCurrentStageText();
    }

    private void ShowGameInfo()
    {
        isSubMenuShowing = true;
        gameInfoUIParent.SetActive(true);
        gameInfoButton.gameObject.SetActive(false);
        UpdateButtonInteractability(false);
    }

    private void HideGameInfo()
    {
        isSubMenuShowing = false;
        gameInfoUIParent.SetActive(false);
        gameInfoButton.gameObject.SetActive(true);
        UpdateButtonInteractability(true);
    }

    private void ShowDeckInfo()
    {
        isSubMenuShowing = true;
        viewDeckUIParent.SetActive(true);
        viewDeckButton.gameObject.SetActive(false);
        UpdateButtonInteractability(false);

        DeckManager.instance.DisplayDeckCards(viewDeckCardsUIParent.transform);
    }

    private void HideDeckInfo()
    {
        isSubMenuShowing = false;
        viewDeckUIParent.SetActive(false);
        viewDeckButton.gameObject.SetActive(true);
        UpdateButtonInteractability(true);

        // Destroy displayed cards
        foreach(Transform child in viewDeckCardsUIParent.transform)
        {
            if(child.childCount > 0)
            {
                Destroy(child.GetChild(0).gameObject);
            }
        }
    }

    /// <summary>
    /// Update active buttons' interactability
    /// </summary>
    /// <param name="interactable">Whether buttons should be interactable</param>
    private void UpdateButtonInteractability(bool interactable)
    {
        if(endTurnButton.gameObject.activeSelf)
        {
            endTurnButton.interactable = interactable;
        }

        if(selectCardButton.gameObject.activeSelf)
        {
            selectCardButton.interactable = interactable;
        }

        if(skipButton.gameObject.activeSelf)
        {
            skipButton.interactable = interactable;
        }

        if(drinkWellButton.gameObject.activeSelf)
        {
            drinkWellButton.interactable = interactable;
        }

        if(viewDeckButton.gameObject.activeSelf)
        {
            viewDeckButton.interactable = interactable;
        }

        if(gameInfoButton.gameObject.activeSelf)
        {
            gameInfoButton.interactable = interactable;
        }
    }

    public void SetCardSelectionButton(bool isACardSelected)
    {
        selectCardButton.interactable = isACardSelected;
    }

    public void UpdateGameEndText()
    {
        bool victory = GameManager.instance.Player.CurrentLife > 0;
        if(victory)
        {
            gameEndHeaderText.text = "VICTORY";
        }
        else
        {
            gameEndHeaderText.text = string.Format(
                "SLAIN ON {0}",
                GameManager.instance.GetCurrentStageText());
        }

        // Check for the current spirit card, which has no starter
        string spiritText = "None";
        if(CardManager.instance.GetCurrentCardData(Slot.Spirit) != null)
        {
            spiritText = CardManager.instance.GetCurrentCardData(Slot.Spirit).Name;
        }

        gameEndDeckInfoText.text = string.Format(
            "Character: {0}" +
            "\n\nDeck" +
            "\nMain Hand: {1}" +
            "\nOff Hand: {2}" +
            "\nAlly: {3}" +
            "\nSpirit: {4}" +
            "\nSpell: {5}" +
            "\nDrink: {6}",
            CharacterManager.instance.ChosenCharacter,
            CardManager.instance.GetCurrentCardData(Slot.MainHand).Name,
            CardManager.instance.GetCurrentCardData(Slot.OffHand).Name,
            CardManager.instance.GetCurrentCardData(Slot.Ally).Name,
            spiritText,
            CardManager.instance.GetCurrentCardData(Slot.Spell).Name,
            CardManager.instance.GetCurrentCardData(Slot.Drink).Name);
    }
}
