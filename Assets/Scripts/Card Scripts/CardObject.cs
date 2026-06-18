using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardObject : MonoBehaviour
{
    // Set in inspector
    [SerializeField]
    protected Canvas canvas;
    [SerializeField]
    protected GameObject cardSelectionRing, cardToBePlayedRing;
    [SerializeField]
    private Image cardBaseImage, cardArtImage;
    [SerializeField]
    private TMP_Text cardNameText, cardSlotText, cardDescriptionText;

    // Set at Start
    [SerializeField]
    protected bool isSelected, isBeingDragged;

    // Set in script after card is Instantiated (in DeckManager.SpawnCard())
    protected CardData cardData;

    public CardData CardData { get { return cardData; } }

    protected virtual void Start()
    {
        if(cardSelectionRing != null)
        {
            cardSelectionRing.SetActive(false);
        }
        if(cardToBePlayedRing != null)
        {
            cardToBePlayedRing.SetActive(false);
        }
        isSelected = false;
        isBeingDragged = false;

        Deselect();
    }

    public void SetCardData(CardData cardData)
    {
        this.cardData = cardData;
        cardNameText.text = cardData.Name;
        cardSlotText.text = cardData.Slot switch
        {
            Slot.MainHand => "PHY",
            Slot.OffHand => "DEF",
            Slot.Ally => "ALY",
            Slot.Spell => "MAG",
            Slot.Spirit => "SPI",
            Slot.Drink => "DRK",
            _ => "???"
        };

        UpdateCardDescription();

        Sprite cardArtSprite = CardManager.instance.GetCardArtSprite(cardData.Name);
        if(cardArtSprite != null)
        {
            // Set card art image
            cardArtImage.gameObject.SetActive(true);
            cardArtImage.sprite = cardArtSprite;
        }
        else
        {
            cardArtImage.gameObject.SetActive(false);
        }

        Sprite cardBaseSprite = CardManager.instance.GetCardBaseSprite(cardData.Slot, cardData.Rarity);
        if(cardBaseSprite != null)
        {
            // Set card base image
            cardBaseImage.gameObject.SetActive(true);
            cardBaseImage.sprite = cardBaseSprite;
        }
    }

    public void UpdateCardDescription()
    {
        Unit unit = null;
        if(cardData.Slot == Slot.Ally)
        {
            if(CharacterManager.instance.Ally != null)
            {
                unit = CharacterManager.instance.Ally;
            }
        }
        else
        {
            unit = GameManager.instance.Player;
        }

        cardDescriptionText.text = cardData.GetCardDescription(unit);
    }

    protected virtual void Select()
    {
        if(cardSelectionRing != null)
        {
            // Show the selection ring of the card
            cardSelectionRing.SetActive(true);
        }
        // Prioritize the card in the sorting layer
        canvas.sortingOrder = 3;

        isSelected = true;
    }

    protected virtual void Deselect()
    {
        if(cardSelectionRing != null)
        {
            // Hide the selection ring of the card
            cardSelectionRing.SetActive(false);
        }
        // Deprioritize the card in the sorting layer
        canvas.sortingOrder = 2;

        isSelected = false;
    }
}
