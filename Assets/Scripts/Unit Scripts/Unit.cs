using System.Collections;
using TMPro;
using UnityEngine;

public enum DamageType
{
    Attack,
    Spell,
    Burn,
    Poison,
    Spike
}

public class Unit : MonoBehaviour
{
    // Instantiated in inspector
    [SerializeField]
    private SpriteRenderer unitSpriteRenderer;
    [SerializeField]
    private TMP_Text lifeText, defenseText;
    [SerializeField]
    private GameObject defenseParent, effectsParent;
    [SerializeField]
    private Transform effectTrans1, effectTrans2;
    [SerializeField]
    protected int maxLife;

    // Instantiated in code
    protected int currentLife, currentDefense;
    protected UnitEffects unitEffects;
    private float effectOffset;

    public int CurrentLife { get { return currentLife; } }
    public UnitEffects UnitEffects { get { return unitEffects; } }

    protected virtual void Awake()
    {
        unitEffects = new UnitEffects();
    }

    protected virtual void Start()
    {
        effectOffset = effectTrans2.localPosition.x - effectTrans1.localPosition.x;
    }

    public virtual void Reset()
    {
        currentLife = maxLife;
        PostCombatReset();
        UpdateLifeUIText();
    }

    public void PostCombatReset()
    {
        currentDefense = 0;
        unitEffects.ResetAllEffects();

        CharacterManager.instance.ResetSummons();
        RemoveEffectsUI();
        UpdateDefenseUIText();
    }

    public virtual void TakeDamage(int amount, DamageType damageType)
    {
        TakeDamage(amount, null, damageType);
    }

    public virtual void TakeDamage(int amount, Unit attacker, DamageType damageType)
    {
        if(amount <= 0)
        {
            return;
        }

        // Check whether the damage is blockable
        if(damageType != DamageType.Poison)
        {
            // Account for damaging the defense first, then health
            if(amount > currentDefense)
            {
                // If the damage dealt is more than the unit's defense,
                // subtract the difference from the unit's health
                currentLife -= (amount - currentDefense);
                currentDefense = 0;
            }
            else
            {
                // If the damage dealth is less than the unit's defense,
                // subtract it from the current defense
                currentDefense -= amount;
            }

            // Update defense UI text
            UpdateDefenseUIText();
        }
        else
        {
            currentLife -= amount;
        }

        // If the damage type is an attack or spell, there is an attacker, and the unit has spikes,
        // reflect spike damage to the attacker
        if((damageType == DamageType.Attack || damageType == DamageType.Spell)
            && attacker != null && unitEffects.GetEffectAmount("Spike") > 0)
        {
            AudioManager.instance.PlaySpikesAudio();
            attacker.TakeDamage(unitEffects.GetEffectAmount("Spike") + unitEffects.GetEffectAmount("Buff Spike"), DamageType.Spike);
        }
        // Update life UI text
        UpdateLifeUIText();
    }

    public void Heal(int amount)
    {
        if(amount < 0)
        {
            return;
        }

        AudioManager.instance.PlayHealAudio();
        currentLife += amount + unitEffects.GetEffectAmount("Buff Healing");
        if(currentLife > maxLife)
        {
            currentLife = maxLife;
        }
        // Cure poison when healing
        unitEffects.UpdateEffectAmount("Poison", -amount);
        UpdateEffectsUI();
        // Update life UI text
        UpdateLifeUIText();
    }

    public void GiveDefense(int amount)
    {
        if(amount < 0)
        {
            return;
        }

        AudioManager.instance.PlayGiveDefenseAudio();
        currentDefense += amount + unitEffects.GetEffectAmount("Buff Defense");
        UpdateDefenseUIText();
    }

    public void ClearDefense()
    {
        currentDefense = 0;
        UpdateDefenseUIText();
    }

    public void GiveBurn(int amount)
    {
        if(amount < 0)
        {
            return;
        }

        AudioManager.instance.PlayBurnAudio();
        unitEffects.UpdateEffectAmount("Burn", amount);
        UpdateEffectsUI();
    }

    public void GivePoison(int amount)
    {
        if(amount < 0)
        {
            return;
        }

        AudioManager.instance.PlayPoisonAudio();
        unitEffects.UpdateEffectAmount("Poison", amount);
        UpdateEffectsUI();
    }

    public void GiveSpike(int amount)
    {
        if(amount < 0)
        {
            return;
        }

        AudioManager.instance.PlaySpikesAudio();
        unitEffects.UpdateEffectAmount("Spike", amount);
        UpdateEffectsUI();
    }

    public void Cleanse()
    {
        unitEffects.Cleanse();
        UpdateEffectsUI();
    }

    public void BuffAttack(int amount)
    {
        unitEffects.UpdateEffectAmount("Buff Attack", amount);
        UpdateEffectsUI();
    }

    public void BuffDefense(int amount)
    {
        unitEffects.UpdateEffectAmount("Buff Defense", amount);
        UpdateEffectsUI();
    }

    public void BuffHealing(int amount)
    {
        unitEffects.UpdateEffectAmount("Buff Healing", amount);
        UpdateEffectsUI();
    }

    public void BuffBurn(int amount)
    {
        unitEffects.UpdateEffectAmount("Buff Burn", amount);
        UpdateEffectsUI();
    }

    public void BuffPoison(int amount)
    {
        unitEffects.UpdateEffectAmount("Buff Poison", amount);
        UpdateEffectsUI();
    }

    public void BuffSpike(int amount)
    {
        unitEffects.UpdateEffectAmount("Buff Spike", amount);
        UpdateEffectsUI();
    }

    public bool HasEffectsToProcess()
    {
        return unitEffects.hasAfflictiveEffects();
    }

    public IEnumerator ProcessEffects()
    {
        WaitForSeconds betweenEffectsDelayWait = new WaitForSeconds(1);
        WaitForSeconds effectTriggerToDamageDelayWait = new WaitForSeconds(0.5f);
        WaitForSeconds turnBannerDelayWait = new WaitForSeconds(UIManager.instance.TurnBannerVisibleTime);

        if(unitEffects.GetEffectAmount("Burn") > 0)
        {
            AudioManager.instance.PlayBurnAudio();
            unitSpriteRenderer.color = ParticlesManager.instance.BurnColor;
            // TODO: Activate burn visual effect
            yield return effectTriggerToDamageDelayWait;
            unitSpriteRenderer.color = ParticlesManager.instance.ResetColor;
            TakeDamage(unitEffects.GetEffectAmount("Burn"), null, DamageType.Burn);
            unitEffects.UpdateEffectAmount("Burn", -1);
            UpdateEffectsUI();
        }

        if(unitEffects.GetEffectAmount("Poison") > 0)
        {
            yield return betweenEffectsDelayWait;
            AudioManager.instance.PlayPoisonAudio();
            unitSpriteRenderer.color = ParticlesManager.instance.PoisonColor;
            // TODO: Activate poison visual effect
            yield return effectTriggerToDamageDelayWait;
            unitSpriteRenderer.color = ParticlesManager.instance.ResetColor;
            TakeDamage(unitEffects.GetEffectAmount("Poison"), null, DamageType.Poison);
            unitEffects.UpdateEffectAmount("Poison", -1);
            UpdateEffectsUI();
        }

        // If it is the player's turn, set up for the beginning of it
        if(GameManager.instance.CurrentCombatState == CombatState.PlayerTurn)
        {
            DeckManager.instance.DealHand();
            UIManager.instance.TogglePlayerTurnBanner(true);
            yield return turnBannerDelayWait;
            UIManager.instance.TogglePlayerTurnBanner(false);
            UIManager.instance.EnableEndTurnButton();
            yield return effectTriggerToDamageDelayWait;
            EnemyManager.instance.GetCurrentEnemies().ForEach(e => e.UpdateNextActionUI());
        }

        // If this Unit is an enemy, mark it processed
        Enemy enemyComp = gameObject.GetComponent<Enemy>();
        if(enemyComp != null)
        {
            enemyComp.MarkProcessed();
        }
    }

    protected void UpdateLifeUIText()
    {
        lifeText.text = string.Format("{0}/{1}", currentLife, maxLife);
    }

    protected void UpdateDefenseUIText()
    {
        if(currentDefense > 0)
        {
            defenseParent.SetActive(true);
            defenseText.text = currentDefense.ToString();
        }
        else
        {
            defenseParent.SetActive(false);
        }
    }

    protected void RemoveEffectsUI()
    {
        for(int i = effectsParent.transform.childCount - 1; i >= 2; i--)
        {
            Destroy(effectsParent.transform.GetChild(i).gameObject);
        }
    }

    protected void UpdateEffectsUI()
    {
        RemoveEffectsUI();
        int effectsCount = 0;

        foreach(Effect effect in unitEffects.GetAllEffects())
        {
            if(effect.Amount > 0)
            {
                if(effect.EffectName.Contains("Buff"))
                {
                    CreateNewEffectUIObject(effect.Amount, effect.EffectName["Buff ".Length..], effectsCount, true);
                }
                else
                {
                    CreateNewEffectUIObject(effect.Amount, effect.EffectName, effectsCount, false);
                }
                effectsCount++;
            }
        }
    }

    private void CreateNewEffectUIObject(int amount, string effectName, int currentEffectsCount, bool isBuffEffect)
    {
        Vector2 position = effectTrans1.localPosition;
        position.x += effectOffset * currentEffectsCount;
        GameObject prefab = CardManager.instance.EffectUIPrefab;
        if(isBuffEffect)
        {
            prefab = CardManager.instance.EffectBuffUIPrefab;
        }
        GameObject effectObject = Instantiate(prefab, effectsParent.transform);
        effectObject.transform.localPosition = position;
        effectObject.GetComponent<EffectUIObject>().UpdateEffectUIObject(amount, CardManager.instance.GetActionSprite(effectName));
    }
}
