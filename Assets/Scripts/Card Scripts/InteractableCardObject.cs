using UnityEngine;

public class InteractableCardObject : CardObject
{
    // Set in inspector
    [SerializeField]
    private BoxCollider2D cardCollider;
    [SerializeField]
    private Transform hoverOffset;

    // Set in script at Start
    private Vector2 savedPos, dragOffset;
    private Collider2D cardFieldCollider;
    private bool isInField;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();

        cardToBePlayedRing.SetActive(false);

        savedPos = transform.position;
        dragOffset = Vector2.zero;
        cardFieldCollider = DeckManager.instance.FieldCollider;
        isInField = false;
    }

    // Update is called once per frame
    void Update()
    {
        // If this card targets, enable it if this is the current targetting card and there is no target
        if(cardData.DoesCardTarget)
        {
            cardToBePlayedRing.SetActive(
                TargettingManager.instance.CardTargetting == gameObject &&
                TargettingManager.instance.Target != null);
        }

        cardCollider.enabled = !UIManager.instance.IsSubMenuShowing;
    }

    public void Move(Vector2 newPos)
    {
        transform.localPosition = newPos;
        savedPos = transform.position;
    }

    private void OnMouseEnter()
    {
        // When the card is first hovered over, if the player is not already targetting,
        if(TargettingManager.instance.CardTargetting == null)
        {
            if(cardData.DoesCardTarget)
            {
                // If this card targets, move it up
                transform.position += (Vector3)hoverOffset.localPosition;
            }
            else
            {
                // If this card does not target, if it is not being dragged, move it up
                if(!isBeingDragged)
                {
                    transform.position += (Vector3)hoverOffset.localPosition;
                }
            }
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

    private void OnMouseDown()
    {
        // When clicked over, select this card the player is not already targetting
        if(TargettingManager.instance.CardTargetting == null)
        {
            Select();
        }

        // Also, if this card targets, start targetting it
        if(cardData.DoesCardTarget)
        {
            TargettingManager.instance.StartTargetting(gameObject);
            cardToBePlayedRing.SetActive(true);
        }
        else
        {
            // If it does not target, it is about to be dragged
            // Calculate the offset the mouse is from the center of the card
            dragOffset = savedPos - TargettingManager.instance.GetMousePosition() 
                + new Vector2(hoverOffset.localPosition.x, hoverOffset.localPosition.y);
            isBeingDragged = true;
        }
    }

    private void OnMouseDrag()
    {
        // If the card does not target, drag it
        if(!cardData.DoesCardTarget)
        {
            // When dragging, move the card to the mouse's position, and account for an offset
            transform.position = TargettingManager.instance.GetMousePosition() + dragOffset;
        }
    }

    private void OnMouseUp()
    {
        // When Card is being dropped
        if(cardData.DoesCardTarget)
        {
            // If the card targets and has one, play it with the current target
            if(TargettingManager.instance.CardTargetting != null && TargettingManager.instance.Target != null)
            {
                PlayCard(TargettingManager.instance.Target);
            }
            else
            {
                // Otherwise deselect it
                Deselect();
                cardToBePlayedRing.SetActive(false);
            }
            TargettingManager.instance.StopTargetting();
        }
        else
        {
            // If the card does not target, check if it is in the playing field
            // If it is, play it
            if(isInField)
            {
                PlayCard(null);
            }
            else
            {
                // If not in the playing field, it should no longer be dragged
                isBeingDragged = false;
            }
        }

        // If the card is not in the playing field OR targets but didnt have one,
        // move the card back to its original position
        transform.position = savedPos;
    }

    private void PlayCard(GameObject target)
    {
        if(target == null)
        {
            CardManager.instance.Play(gameObject);
        } 
        else
        {
            CardManager.instance.Play(gameObject, target.GetComponent<Enemy>());
        }
    }

    protected override void Deselect()
    {
        base.Deselect();

        // If the card targets, stop targetting
        if(cardData.DoesCardTarget)
        {
            TargettingManager.instance.StopTargetting();
        }

        // If the card is not already in its original, saved position,
        // remove the hover offset to return it to its original position
        if((Vector2)transform.position != savedPos)
        {
            transform.position -= (Vector3)hoverOffset.localPosition;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If the card does not target AND it has collided with the
        // field's collider, it is in the field (ready to be played)
        if(!cardData.DoesCardTarget && collision.collider == cardFieldCollider)
        {
            isInField = true;
            cardToBePlayedRing.SetActive(true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // If the card does not target AND it has left the field's
        // collider, it is no longer in the field (cannot be played)
        if(!cardData.DoesCardTarget && collision.collider == cardFieldCollider)
        {
            isInField = false;
            cardToBePlayedRing.SetActive(false);
        }
    }
}
