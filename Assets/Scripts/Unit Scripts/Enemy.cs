using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Unit
{
    [SerializeField]
    private TMP_Text nextActionText;
    [SerializeField]
    private Image nextActionIcon;
    [SerializeField]
    private List<string> actions;
    [SerializeField]
    private GameObject enemySummonPrefab;

    private int round;
    private int positionIndex;
    private bool hasBeenProcessed;

    public int Round { get { return round; } }
    public int PositionIndex { get { return positionIndex; } }
    public bool HasBeenProcessed { get { return hasBeenProcessed; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        Reset();

        // For certain enemies, if there are multiple enemies, offset their attacks
        List<Enemy> currentEnemies = EnemyManager.instance.GetCurrentEnemies();
        if(currentEnemies.Count > 1
            && gameObject.name.Contains("Boar")
            && currentEnemies.IndexOf(this) % 2 == 1)
        {
            round = 1;
        }

        UpdateNextActionUI();
    }

    public override void Reset()
    {
        base.Reset();
        round = 0;
        hasBeenProcessed = false;
    }

    public void SetPositionIndex(int index)
    {
        if(index < 0)
        {
            Debug.Log("Error! Cannot set enemy position index to less than 0!");
            return;
        }

        positionIndex = index;
    }

    public void IncrementRound()
    {
        round++;
        hasBeenProcessed = false;
    }

    public void MarkProcessed()
    {
        hasBeenProcessed = true;
    }

    private string[] ParseRoundAction()
    {
        int actionRound = round % actions.Count;
        return actions[actionRound].Split(" ");
    }

    public void PerformRoundAction()
    {
        string[] parsedAction = ParseRoundAction();
        string actionString = parsedAction[0];
        if(parsedAction[1].Contains("x"))
        {
            string[] damageParts = parsedAction[1].Split("x");
            for(int i = 0; i < int.Parse(damageParts[1]); i++)
            {
                PerformAction(actionString, int.Parse(damageParts[0]));
            }
        }
        else
        {
            PerformAction(actionString, int.Parse(parsedAction[1]));
        }
    }

    private void PerformAction(string actionString, int actionAmount)
    {
        switch(actionString.Split(" ")[0])
        {
            case "Attack":
                AudioManager.instance.PlayAttackAudio();
                GameManager.instance.Player.TakeDamage(actionAmount + currentAttackBuff, this, DamageType.Attack);
                break;
            case "Heal":
                Heal(actionAmount);
                break;
            case "Defend":
                AudioManager.instance.PlayGiveDefenseAudio();
                GiveDefense(actionAmount);
                break;
            case "Burn":
                GameManager.instance.Player.GiveBurn(actionAmount);
                break;
            case "Poison":
                GameManager.instance.Player.GivePoison(actionAmount);
                break;
            case "Summon":
                if(enemySummonPrefab == null)
                {
                    Debug.Log(string.Format("Error! No enemy summon prefab found!"));
                    break;
                }
                else
                {
                    for(int i = 0; i < actionAmount; i++)
                    {
                        EnemyManager.instance.SpawnSummon(enemySummonPrefab);
                    }
                }
                break;
            default:
                Debug.Log(string.Format("Error! No action type of {0} for enemy {1}", actionString, gameObject.name));
                break;
        }

        // Post-action updates
        IncrementRound();
        HideNextActionUI();
    }

    public override void TakeDamage(int amount, DamageType damageType)
    {
        TakeDamage(amount, null, damageType);
    }

    public override void TakeDamage(int amount, Unit attacker, DamageType damageType)
    {
        base.TakeDamage(amount, attacker, damageType);

        if(currentLife <= 0)
        {
            AudioManager.instance.PlayDeathAudio();
            Destroy(gameObject);
        }
    }

    public void HideNextActionUI()
    {
        nextActionText.gameObject.SetActive(false);
        nextActionIcon.gameObject.SetActive(false);
    }

    public void UpdateNextActionUI()
    {
        string[] nextAction = ParseRoundAction();
        nextActionText.text = nextAction[1];
        nextActionIcon.sprite = CardManager.instance.GetActionSprite(nextAction[0]);

        nextActionText.gameObject.SetActive(true);
        nextActionIcon.gameObject.SetActive(true);
    }

    private void OnMouseEnter()
    {
        // If the player is currently targetting, set this enemy as the target
        if(TargettingManager.instance.CardTargetting != null)
        {
            TargettingManager.instance.SetTarget(gameObject);
        }
    }

    private void OnMouseExit()
    {
        // If the player is currently targetting, remove this enemy as the target
        if(TargettingManager.instance.CardTargetting != null)
        {
            TargettingManager.instance.SetTarget(null);
        }
    }
}
