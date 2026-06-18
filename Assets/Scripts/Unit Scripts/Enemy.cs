using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class Enemy : Unit
{
    [SerializeField]
    private TMP_Text nextActionText;
    [SerializeField]
    private Image nextActionIcon;
    [SerializeField]
    private GameObject nextActionBuffIcon;
    [SerializeField]
    private EnemyType enemyType;

    [SerializeField]
    private int round;
    private int positionIndex;
    private bool hasBeenProcessed;
    private List<Action> actions;

    public int Round { get { return round; } }
    public int PositionIndex { get { return positionIndex; } }
    public bool HasBeenProcessed { get { return hasBeenProcessed; } }
    public EnemyType EnemyType { get { return enemyType; } }

    protected override void Awake()
    {
        base.Awake();
        Reset();
        actions = EnemyManager.instance.GetEnemyActions(enemyType);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        UpdateNextActionUI();
    }

    public override void Reset()
    {
        base.Reset();
        HideNextActionUI();
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

    public void IncrementStartingRound()
    {
        round++;
        UpdateNextActionUI();
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

    private Action ParseRoundAction()
    {
        return actions[round % actions.Count];
    }

    public void PerformRoundAction()
    {
        Action action = ParseRoundAction();
        ActionManager.instance.PerformAction(action, this, null);

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
        nextActionBuffIcon.SetActive(false);
    }

    public void UpdateNextActionUI()
    {
        Action nextAction = ParseRoundAction();
        int nextActionTotalAmount = nextAction.Amount + unitEffects.GetEffectAmount(nextAction.ActionType, true);
        nextActionText.text = nextActionTotalAmount.ToString();
        nextActionIcon.sprite = CardManager.instance.GetActionSprite(nextAction.ActionType);

        nextActionText.gameObject.SetActive(true);
        nextActionIcon.gameObject.SetActive(true);
        nextActionBuffIcon.SetActive(nextAction is Buff);
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
