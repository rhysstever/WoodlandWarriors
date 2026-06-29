using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    // Singleton
    public static ActionManager instance = null;

    private IEnumerator actionCoroutine;

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

    public void PerformAction(Action action, Unit actor, Unit target)
    {
        PerformActions(new List<Action> { action }, actor, target);
    }

    public void PerformActions(List<Action> actions, Unit actor, Unit target)
    {
        actionCoroutine = ProcessActions(actions, actor, target);
        StartCoroutine(actionCoroutine);
    }

    private IEnumerator ProcessActions(List<Action> actions, Unit actor, Unit target)
    {
        WaitForSeconds actionDelayWait = new WaitForSeconds(0.1f);
        WaitForSeconds timesDelayWait = new WaitForSeconds(0.25f);
        foreach(Action action in actions)
        {
            bool isBuff = action.GetActionDescription(null).Split(" ")[0] == "Buff";
            yield return actionDelayWait;

            // Repeat action if it triggers multiple times
            for(int i = 0; i < action.Times; i++)
            {
                if(EnemyManager.instance.IsWaveOver())
                {
                    break;
                }

                yield return timesDelayWait;
                // Check if the action should hit all enemies
                if((action.TargetType == TargetType.AllFoes && actor is Player)
                    || (action.TargetType == TargetType.AllAllies && actor is Enemy))
                {
                    EnemyManager.instance.GetCurrentEnemies().ForEach(enemy => {
                        if(isBuff)
                        {
                            ProcessBuff(action, enemy);
                        }
                        else
                        {
                            ProcessAction(action, actor, enemy);
                        }
                    });
                }
                else
                {
                    target = UpdateTarget(action.TargetType, actor, target);
                    if(isBuff)
                    {
                        ProcessBuff(action, target);
                    }
                    else
                    {
                        ProcessAction(action, actor, target);
                    }
                }
            }
        }

        // Reset targetting
        TargettingManager.instance.Reset();

        // Check if combat is over
        if(EnemyManager.instance.IsWaveOver())
        {
            GameManager.instance.ChangeCombatState(CombatState.End);
        }
    }

    private Unit UpdateTarget(TargetType actionTargetType, Unit actor, Unit target)
    {
        switch(actionTargetType)
        {
            case TargetType.None:
                return null;
            case TargetType.Self:
                return actor;
            case TargetType.Player:
                return GameManager.instance.Player;
            case TargetType.RandomFoe:
                if(actor is Enemy)
                {
                    // TODO - Get random between player and ally
                    return GameManager.instance.Player;
                }
                else
                {
                    return EnemyManager.instance.GetRandomEnemy();
                }
            case TargetType.AllAllies:
            case TargetType.AllFoes:
            case TargetType.Ally:
            case TargetType.Foe:
                if((actor is Player && target is Player)
                    || (actor is Enemy && target is Enemy))
                {
                    Debug.Log("Warning! Both the actor and player are the same type!");
                }
                break;
        }

        return target;
    }

    private void ProcessAction(Action action, Unit actor, Unit target) 
    {
        int amount = action.Amount;
        switch(action.ActionType)
        {
            case ActionType.Attack:
                actor.DealDamage(amount, target, DamageType.Attack);
                break;
            case ActionType.MagicalAttack:
                actor.DealDamage(amount, target, DamageType.Spell);
                break;
            case ActionType.Defend:
                target.GiveDefense(amount);
                break;
            case ActionType.Heal:
                target.Heal(amount);
                break;
            case ActionType.Burn:
                if(actor is Player && CharacterManager.instance.ChosenCharacter == Character.Skunk)
                {
                    amount++;
                }
                target.GiveBurn(amount);
                break;
            case ActionType.Poison:
                if(actor is Player && CharacterManager.instance.ChosenCharacter == Character.Skunk)
                {
                    amount++;
                }
                target.GivePoison(amount);
                break;
            case ActionType.Spike:
                target.GiveSpike(amount);
                break;
            case ActionType.Draw:
                if(action.TargetType == TargetType.Player)
                {
                    DeckManager.instance.DrawCards(amount);
                }
                break;
            case ActionType.Cleanse:
                target.Cleanse();
                break;
            case ActionType.Summon:
                if(action.TargetType == TargetType.None)
                {
                    for(int i = 0; i < action.Amount; i++)
                    {
                        if(actor is Enemy)
                        {
                            EnemySummon summon = action as EnemySummon;
                            GameObject enemySummonPrefab = EnemyManager.instance.GetEnemyPrefabByType(summon.EnemyType);
                            EnemyManager.instance.SpawnSummon(enemySummonPrefab);
                        }
                        else
                        {
                            Summon summon = action as Summon;
                            CharacterManager.instance.SummonAlly(summon);
                        }
                    }
                }
                break;
        }
    }

    private void ProcessBuff(Action action, Unit target)
    {
        switch(action.ActionType)
        {
            case ActionType.Attack:
                target.BuffAttack(action.Amount);
                break;
            case ActionType.Defend:
                target.BuffDefense(action.Amount);
                break;
            case ActionType.Heal:
                target.BuffHealing(action.Amount);
                break;
            case ActionType.Burn:
                target.BuffBurn(action.Amount);
                break;
            case ActionType.Poison:
                target.BuffPoison(action.Amount);
                break;
            case ActionType.Spike:
                target.BuffSpike(action.Amount);
                break;
            default:
                Debug.Log(string.Format("Error! No buff for type: {0}", action.ActionType));
                break;
        }

        // Update hand
        DeckManager.instance.UpdateCurrentHandDescriptions();
    }
}
