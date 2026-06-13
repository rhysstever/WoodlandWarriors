using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Singleton
    public static UIManager instance;

    // Set in inspector
    [SerializeField]
    private GameObject mainMenuUIParent, characterSelectUIParent, gameUIParent, gameEndUIParent;
    [SerializeField]
    private GameObject mainMenuButtonsParent, combatUIParent, nonCombatUIParent, cardSelectionUIParent, wellUIParent;
    [SerializeField]
    private GameObject gameInfoUIParent, viewDeckUIParent, viewDeckCardsUIParent;
    [SerializeField]
    private GameObject playerTurnBanner, enemyTurnBanner;
    [SerializeField]
    private Button mainMenuToCharacterSelectButton, quitButton, gameInfoButton, closeGameInfoButton, viewDeckButton, closeViewDeckButton, endTurnButton, selectCardButton, skipButton, drinkWellButton, gameEndToMainMenuButton;
    [SerializeField]
    private TMP_Text characterSelectInfoText, gameAreaStageText, gameEndHeaderText, gameEndDeckInfoText;

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
        turnBannerVisibleTime = 2f;
        TogglePlayerTurnBanner(false);
        ToggleEnemyTurnBanner(false);

        // Set up button listeners
        mainMenuToCharacterSelectButton.onClick.AddListener(() => {
            mainMenuButtonsParent.SetActive(false);
            Camera.main.GetComponent<CameraPan>().PanCameraDown();
        });
        quitButton.onClick.AddListener(() => Application.Quit());
        gameInfoButton.onClick.AddListener(() => ShowGameInfo());
        closeGameInfoButton.onClick.AddListener(() => HideGameInfo());
        viewDeckButton.onClick.AddListener(() => ShowDeckInfo());
        closeViewDeckButton.onClick.AddListener(() => HideDeckInfo());
        endTurnButton.onClick.AddListener(() => {
            endTurnButton.interactable = false;
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
        switch(menuState)
        {
            case MenuState.MainMenu:
                mainMenuUIParent.SetActive(true);
                mainMenuButtonsParent.SetActive(true);
                characterSelectUIParent.SetActive(false);
                gameUIParent.SetActive(false);
                gameEndUIParent.SetActive(false);
                break;
            case MenuState.CharacterSelect:
                mainMenuUIParent.SetActive(false);
                characterSelectUIParent.SetActive(true);
                UpdateCharacterSelectInfo();
                gameUIParent.SetActive(false);
                gameEndUIParent.SetActive(false);
                break;
            case MenuState.Game:
                mainMenuUIParent.SetActive(false);
                characterSelectUIParent.SetActive(false);
                gameUIParent.SetActive(true);
                gameEndUIParent.SetActive(false);
                HideGameInfo();
                HideDeckInfo();
                break;
            case MenuState.GameEnd:
                mainMenuUIParent.SetActive(false);
                characterSelectUIParent.SetActive(false);
                gameUIParent.SetActive(false);
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

                endTurnButton.gameObject.SetActive(true);
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

    public void EnableEndTurnButton()
    {
        endTurnButton.interactable = true;
    }

    public void TogglePlayerTurnBanner(bool isActive)
    {
        playerTurnBanner.SetActive(isActive);
    }

    public void ToggleEnemyTurnBanner(bool isActive)
    {
        enemyTurnBanner.SetActive(isActive);
    }

    public void UpdateCharacterSelectInfo()
    {
        characterSelectInfoText.text = "Choose Your Character";
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
            CardManager.instance.GetCurrentCardData(Slot.Physical).Name,
            CardManager.instance.GetCurrentCardData(Slot.Defense).Name,
            CardManager.instance.GetCurrentCardData(Slot.Ally).Name,
            spiritText,
            CardManager.instance.GetCurrentCardData(Slot.Magical).Name,
            CardManager.instance.GetCurrentCardData(Slot.Drink).Name);
    }
}
