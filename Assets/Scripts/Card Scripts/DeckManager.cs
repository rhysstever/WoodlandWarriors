using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    // Singleton
    public static DeckManager instance;

    // Set in inspector
    [SerializeField]
    private Transform cardParentTrans, cardHandBoundsTrans, cardSelectionCardParentTrans, cardSelectionCard1Pos, cardSelectionCard2Pos, cardSelectionCard3Pos;
    [SerializeField]
    private Collider2D fieldCollider;
    [SerializeField]    // Prefabs
    private GameObject playableCardPrefab, selectableCardPrefab, displayCardPrefab;

    // Set in script
    private List<CardData> deck;
    private int numCardsDrawnAtPlayerTurnStart;
    private CardData currentCardSelection;
    private float playableCardPosXMin, playableCardPosXMax;

    // Properties
    public Collider2D FieldCollider { get { return fieldCollider; } }
    public CardData CurrentCardSelection { get { return currentCardSelection; } }

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

        deck = new List<CardData>();
    }

    void Start()
    {
        currentCardSelection = null;
        numCardsDrawnAtPlayerTurnStart = 4;

        float playableCardWidthHalf = playableCardPrefab.GetComponent<BoxCollider2D>().bounds.size.x / 2;
        playableCardPosXMin = cardHandBoundsTrans.position.x - cardHandBoundsTrans.localScale.x / 2 + playableCardWidthHalf;
        playableCardPosXMax = cardHandBoundsTrans.position.x + cardHandBoundsTrans.localScale.x / 2 - playableCardWidthHalf;
    }

    public void SetupForNewCombat()
    {
        deck = GenerateDeck();
        ClearHand();
        fieldCollider.gameObject.SetActive(true);
        GameManager.instance.ChangeCombatState(CombatState.PlayerTurn);
    }

    public List<CardData> GenerateDeck()
    {
        List<CardData> cards = new();

        foreach(Slot slot in Enum.GetValues(typeof(Slot)).Cast<Slot>().ToList())
        {
            CardData cardData = CardManager.instance.GetCurrentCardData(slot);

            if(cardData != null)
            {
                cards.Add(cardData);
            }
            else
            {
                Debug.LogFormat("Warning! No current {0} card, not generating", slot);
            }
        }

        return cards;
    }

    public void ClearHand()
    {
        RemoveAllCardsFromScene();
    }

    public void DealHand()
    {
        DrawCards(numCardsDrawnAtPlayerTurnStart);
        UIManager.instance.ShowEndTurnButton();
    }

    public void DrawCards(int numberOfCardsToDraw)
    {
        // Add the given number of cards from the deck into the hand
        for(int i = 0; i < numberOfCardsToDraw; i++)
        {
            // Get the card from the deck
            CardData cardData = GetRandomCardFromDeck();

            // Spawn the card in the scene
            SpawnCard(playableCardPrefab, cardData, cardParentTrans);
        }

        DisplayHand();
    }

    private CardData GetRandomCardFromDeck()
    {
        if(CharacterManager.instance.ChosenCharacter == Character.Otter)
        {
            // If the chosen character is Otter, all cards have an equal chance of being drawn, so return a random card from the deck
            int randomDeckIndex = UnityEngine.Random.Range(0, deck.Count);
            return deck[randomDeckIndex];
        }

        // Chances: Slot -> # / 18 (4 + 4 + 3 + 3 + 2 + 2)
        // Main Hand -> 4 / 18
        // Off Hand -> 4 / 18
        // Ally -> 3 / 18
        // Spell -> 3 / 18
        // Spirit -> 2 / 18
        // Drink -> 2 / 18

        int randomNum = UnityEngine.Random.Range(0, 18);
        return randomNum switch
        {
            < 4 => CardManager.instance.GetCurrentCardData(Slot.MainHand),
            < 8 => CardManager.instance.GetCurrentCardData(Slot.OffHand),
            < 11 => CardManager.instance.GetCurrentCardData(Slot.Ally),
            < 14 => CardManager.instance.GetCurrentCardData(Slot.Spell),
            < 16 => CardManager.instance.GetCurrentCardData(Slot.Spirit),
            _ => CardManager.instance.GetCurrentCardData(Slot.Drink),
        };
    }

    private GameObject SpawnCard(GameObject cardPrefab, CardData cardData, Transform parent)
    {
        return SpawnCard(cardPrefab, cardData, Vector2.zero, parent);
    }

    private GameObject SpawnCard(GameObject cardPrefab, CardData cardData, Vector2 position, Transform parent)
    {
        GameObject newCard = Instantiate(cardPrefab, position, Quaternion.identity, parent);
        newCard.GetComponent<CardObject>().SetCardData(cardData);
        newCard.name = cardData.Name;

        return newCard;
    }

    public int GetCardIndex(GameObject cardObject)
    {
        for(int i = 0; i < cardParentTrans.childCount; i++)
        {
            if(cardParentTrans.GetChild(i).gameObject == cardObject)
            {
                return i;
            }
        }

        return -1;
    }

    public CardData GetCardDataBySlot(Slot slot)
    {
        foreach(CardData cardData in deck)
        { 
            if(cardData.Slot == slot)
            {
                return cardData;
            }
        }

        return null;
    }

    public void RemoveCard(GameObject cardObject)
    {
        // Hide and Destroy the card game object
        cardObject.SetActive(false);
        Destroy(cardObject);

        // Recenter the remaining cards in hand
        DisplayHand();
    }

    private void DisplayHand()
    {
        // Get a list of active cards (the list of undestroyed cards cannot be used, as destroying cards takes some time)
        List<InteractableCardObject> cardsToBeCentered = new List<InteractableCardObject>();
        for(int i = 0; i < cardParentTrans.childCount; i++)
        {
            if(cardParentTrans.GetChild(i).gameObject.activeSelf)
            {
                cardsToBeCentered.Add(cardParentTrans.GetChild(i).gameObject.GetComponent<InteractableCardObject>());
            }
        }

        if(cardsToBeCentered.Count > 1)
        {
            // If there is more than 1 card, calculate an offset based on how many total cards there are
            float minPos = playableCardPosXMin;
            float cardPosDiff = playableCardPosXMax - playableCardPosXMin;

            //if(cardsToBeCentered.Count < 4)
            //{
            //    // If there are few cards in hand, place them closer togther by
            //    // increasing the minimum position by a percentage of the position difference
            //    // and decreasing the position difference
            //    float percentage = 0.2f;
            //    minPos = playableCardPosXMin + cardPosDiff * percentage;
            //    cardPosDiff *= 1 - (2f * percentage);
            //}

            float percentage = 0.125f;
            if(cardsToBeCentered.Count < 3)
            {
                percentage = 0.3f;
            }

            minPos = playableCardPosXMin + cardPosDiff * percentage;
            cardPosDiff *= 1 - (2f * percentage);

            float offset = cardPosDiff / (cardsToBeCentered.Count - 1);

            // Place each active card
            for(int i = 0; i < cardsToBeCentered.Count; i++)
            {
                cardsToBeCentered[i].Move(new Vector2(minPos + offset * i, 0f));
            }
        }
        else if(cardsToBeCentered.Count == 1)
        {
            // If there is only 1 card, place it in the center
            cardsToBeCentered[0].Move(Vector2.zero);
        }
    }

    private void RemoveAllCardsFromScene()
    {
        for(int i = cardParentTrans.childCount - 1; i >= 0; i--)
        {
            Destroy(cardParentTrans.GetChild(i).gameObject);
        }
    }

    public void UpdateCurrentHandDescriptions()
    {
        for(int i = 0; i < cardParentTrans.childCount; i++)
        {
            cardParentTrans.GetChild(i).GetComponent<CardObject>().UpdateCardDescription();
        }
    }

    public void SetupCardSelection()
    {
        // Hide the field collider
        fieldCollider.gameObject.SetActive(false);

        // Create the card selection cards to display
        List<CardData> cardDatasToDisplay = CardManager.instance.GetRandomCardDatas(3);
        for(int i = 0; i < cardDatasToDisplay.Count; i++)
        {
            Vector2 position = Vector2.zero;
            switch(i)
            {
                case 0:
                    position = cardSelectionCard1Pos.position;
                    break;
                case 1:
                    position = cardSelectionCard2Pos.position;
                    break;
                case 2:
                    position = cardSelectionCard3Pos.position;
                    break;
            }

            SpawnCard(selectableCardPrefab, cardDatasToDisplay[i], position, cardSelectionCardParentTrans);
        }

        // Reset the current selection
        SetCurrentCardSelection(null);
    }

    public void SetCurrentCardSelection(CardData cardSelected)
    {
        currentCardSelection = cardSelected;
        // Update UI
        UIManager.instance.SetCardSelectionButton(cardSelected != null);
    }

    public void AddSelectedCardToDeck()
    {
        if(cardSelectionCardParentTrans != null)
        {
            fieldCollider.gameObject.SetActive(true);
            CardManager.instance.UpdateSlot(currentCardSelection);
            ClearCardSelectionDisplayCards();
        }
        else
        {
            Debug.Log("Error! No card selected");
        }
    }

    public void ClearCardSelectionDisplayCards()
    {
        for(int i = cardSelectionCardParentTrans.childCount - 1; i >= 0; i--)
        {
            Destroy(cardSelectionCardParentTrans.GetChild(i).gameObject);
        }
    }

    public void DisplayDeckCards(Transform viewDeckCardParent)
    {
        for(int i = 0; i < viewDeckCardParent.childCount; i++)
        {
            Slot slot = (Slot)i;
            // Spawn display card for each slot
            Transform child = viewDeckCardParent.GetChild(i);
            CardData cardDataToDisplay = CardManager.instance.GetCurrentCardData(slot);

            SpawnCard(displayCardPrefab, cardDataToDisplay, child.position, child);
        }
    }
}
