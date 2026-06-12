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
            Slot.Physical => "PHY",
            Slot.Defense => "DEF",
            Slot.Ally => "ALY",
            Slot.Magical => "MAG",
            Slot.Spirit => "SPI",
            Slot.Drink => "DRK",
            _ => "???"
        };

        // Updates to the card description based on any character buffs
        cardDescriptionText.text = UpdateCardDescription(cardData.Description);

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

    private string UpdateCardDescription(string description)
    {
        switch(CharacterManager.instance.ChosenCharacter)
        {
            case Character.Badger:
                if(cardData.Slot == Slot.Physical)
                {
                    return IncrementTextNumber(description, "Attack for ");
                }
                return description;
            case Character.Fox:
                if(cardData.Slot == Slot.Magical)
                {
                    return IncrementTextNumber(description, "Attack for ");
                }
                break;
            case Character.Opossum:
                if(cardData.Slot == Slot.Ally)
                {
                    return IncrementTextNumber(description, "with ");
                }
                break;
            case Character.Skunk:
                return IncrementTextNumber(IncrementTextNumber(description, "Poison for"), "Burn for");
            default:
                return description;
        }

        return description;
    }

    private string IncrementTextNumber(string text, string prefix)
    {
        int numIndex = text.IndexOf(prefix);
        if(numIndex != -1)
        {
            int startIndex = numIndex + prefix.Length;
            string numStr = text.Substring(startIndex, 1);
            if(int.TryParse(numStr, out int num))
            {
                num++;
                return text[..startIndex] + num.ToString() + text[(startIndex + 1)..];
            }
            else
            {
                Debug.LogWarning($"Failed to parse number from text: {text}");
                return text;
            }
        }
        else
        {
            Debug.LogWarning($"Failed to find prefix '{prefix}' in text: {text}");
            return text;
        }
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
