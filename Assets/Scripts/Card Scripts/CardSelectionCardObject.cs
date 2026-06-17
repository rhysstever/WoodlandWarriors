using UnityEngine;

public class CardSelectionCardObject : CardObject
{
    // Set in inspector
    [SerializeField]
    private BoxCollider2D cardCollider;

    protected override void Start()
    {
        base.Start();
    }

    protected void Update()
    {
        cardToBePlayedRing.SetActive(cardData == DeckManager.instance.CurrentCardSelection);
        cardCollider.enabled = !UIManager.instance.IsSubMenuShowing;
    }

    private void OnMouseOver()
    {
        // When hovered over, select this card the player is not already targetting
        if(TargettingManager.instance.CardTargetting == null)
        {
            Select();
        }
    }

    private void OnMouseExit()
    {
        // When this card is first no longer hovered,
        // If this card targets, but the player is not currently targetting,
        // OR this card does not target and is not being dragged,
        // Deselect it
        if((cardData.DoesCardTarget && TargettingManager.instance.CardTargetting == null)
            || (!cardData.DoesCardTarget && !isBeingDragged))
        {
            Deselect();
        }
    }

    private void OnMouseUpAsButton()
    {
        if(cardData == DeckManager.instance.CurrentCardSelection)
        {
            DeckManager.instance.SetCurrentCardSelection(null);
        }
        else
        {
            DeckManager.instance.SetCurrentCardSelection(cardData);
        }
    }
}
