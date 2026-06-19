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
    protected SpriteRenderer unitSpriteRenderer;
    [SerializeField]
    protected TMP_Text lifeText;
    [SerializeField]
    private TMP_Text defenseText;
    [SerializeField]
    private GameObject defenseParent, effectsParent;
    [SerializeField]
    private Transform effectTrans1, effectTrans2;
    [SerializeField]
    protected int maxLife;
    [SerializeField]
    private CustomParticleSystem healingParticleSystem, burnParticleSystem, poisonParticleSystem;

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

    public virtual void DealDamage(int baseAttack, Unit target, DamageType damageType)
    {
        int amount = baseAttack + unitEffects.GetEffectAmount(ActionType.Attack, true);
        if(amount < 0)
        {
            return;
        }
        target.TakeDamage(amount, damageType);
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

        // Check for Spike Reflection: 
        // 1) Unit must have spikes
        // 2) Damage type must be an Attack or Spell
        // 3) There must be an attacker
        if(unitEffects.GetEffectAmount(ActionType.Spike) > 0
            && (damageType == DamageType.Attack || damageType == DamageType.Spell)
            && attacker != null)
        {
            AudioManager.instance.PlaySpikesAudio();
            attacker.TakeDamage(
                unitEffects.GetEffectAmount(ActionType.Spike) + unitEffects.GetEffectAmount(ActionType.Spike, true), 
                DamageType.Spike);
        }
        // Update life UI text
        UpdateLifeUIText();
    }

    public void GiveDefense(int baseDefense)
    {
        int amount = baseDefense + unitEffects.GetEffectAmount(ActionType.Defend, true);
        if(amount < 0)
        {
            return;
        }

        AudioManager.instance.PlayGiveDefenseAudio();
        currentDefense += amount + unitEffects.GetEffectAmount(ActionType.Defend, true);
        UpdateDefenseUIText();
    }

    public virtual void Heal(int baseHeal)
    {
        int amount = baseHeal + unitEffects.GetEffectAmount(ActionType.Heal, true);
        if(amount < 0)
        {
            return;
        }

        if(healingParticleSystem != null)
        {
            healingParticleSystem.EnableParticles();
        }
        AudioManager.instance.PlayHealAudio();
        currentLife += amount + unitEffects.GetEffectAmount(ActionType.Heal, true);
        if(currentLife > maxLife)
        {
            currentLife = maxLife;
        }
        // Cure poison when healing
        unitEffects.UpdateEffectAmount(ActionType.Poison, -amount);
        UpdateEffectsUI();
        // Update life UI text
        UpdateLifeUIText();
        //healingSystem.DisableParticles();
    }

    public void ClearDefense()
    {
        currentDefense = 0;
        UpdateDefenseUIText();
    }

    public void GiveBurn(int baseBurn)
    {
        int amount = baseBurn + unitEffects.GetEffectAmount(ActionType.Burn, true);
        if(amount < 0)
        {
            return;
        }

        AudioManager.instance.PlayBurnAudio();
        unitEffects.UpdateEffectAmount(ActionType.Burn, amount);
        UpdateEffectsUI();
    }

    public void GivePoison(int basePoison)
    {
        int amount = basePoison + unitEffects.GetEffectAmount(ActionType.Poison, true);
        if(amount < 0)
        {
            return;
        }

        Debug.Log("Buffing poison");
        AudioManager.instance.PlayPoisonAudio();
        unitEffects.UpdateEffectAmount(ActionType.Poison, amount);
        UpdateEffectsUI();
    }

    public void GiveSpike(int baseSpikes)
    {
        int amount = baseSpikes + unitEffects.GetEffectAmount(ActionType.Spike, true);
        if(amount < 0)
        {
            return;
        }

        AudioManager.instance.PlaySpikesAudio();
        unitEffects.UpdateEffectAmount(ActionType.Spike, amount);
        UpdateEffectsUI();
    }

    public void Cleanse()
    {
        unitEffects.Cleanse();
        UpdateEffectsUI();
    }

    public void BuffAttack(int amount)
    {
        unitEffects.UpdateEffectAmount(ActionType.Attack, amount, true);
        UpdateEffectsUI();
    }

    public void BuffDefense(int amount)
    {
        unitEffects.UpdateEffectAmount(ActionType.Defend, amount, true);
        UpdateEffectsUI();
    }

    public void BuffHealing(int amount)
    {
        unitEffects.UpdateEffectAmount(ActionType.Heal, amount, true);
        UpdateEffectsUI();
    }

    public void BuffBurn(int amount)
    {
        unitEffects.UpdateEffectAmount(ActionType.Burn, amount, true);
        UpdateEffectsUI();
    }

    public void BuffPoison(int amount)
    {
        unitEffects.UpdateEffectAmount(ActionType.Poison, amount, true);
        UpdateEffectsUI();
    }

    public void BuffSpike(int amount)
    {
        unitEffects.UpdateEffectAmount(ActionType.Spike, amount, true);
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

        if(unitEffects.GetEffectAmount(ActionType.Burn) > 0)
        {
            AudioManager.instance.PlayBurnAudio();
            unitSpriteRenderer.color = ParticlesManager.instance.BurnColor;
            if(burnParticleSystem != null)
            {
                burnParticleSystem.EnableParticles();
            }
            yield return effectTriggerToDamageDelayWait;
            unitSpriteRenderer.color = ParticlesManager.instance.ResetColor;
            TakeDamage(unitEffects.GetEffectAmount(ActionType.Burn), null, DamageType.Burn);
            unitEffects.UpdateEffectAmount(ActionType.Burn, -1);
            UpdateEffectsUI();
        }

        if(unitEffects.GetEffectAmount(ActionType.Poison) > 0)
        {
            yield return betweenEffectsDelayWait;
            AudioManager.instance.PlayPoisonAudio();
            unitSpriteRenderer.color = ParticlesManager.instance.PoisonColor;
            if(poisonParticleSystem != null)
            {
                poisonParticleSystem.EnableParticles();
            }
            yield return effectTriggerToDamageDelayWait;
            unitSpriteRenderer.color = ParticlesManager.instance.ResetColor;
            TakeDamage(unitEffects.GetEffectAmount(ActionType.Poison), null, DamageType.Poison);
            unitEffects.UpdateEffectAmount(ActionType.Poison, -1);
            UpdateEffectsUI();
        }

        // If it is the player's turn, set up for the beginning of it
        if(GameManager.instance.CurrentCombatState == CombatState.PlayerTurn)
        {
            UIManager.instance.TogglePlayerTurnBanner(true);
            yield return turnBannerDelayWait;
            UIManager.instance.TogglePlayerTurnBanner(false);
            yield return effectTriggerToDamageDelayWait;
            DeckManager.instance.DealHand();
            EnemyManager.instance.GetCurrentEnemies().ForEach(e => e.UpdateNextActionUI());
        }

        // If this Unit is an enemy, mark it processed
        Enemy enemyComp = gameObject.GetComponent<Enemy>();
        if(enemyComp != null)
        {
            enemyComp.MarkProcessed();
        }
    }

    protected virtual void UpdateLifeUIText()
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
            if(effect.Action.Amount > 0)
            {
                bool isBuff = effect.Action is Buff;
                CreateNewEffectUIObject(effect.Action.Amount, effect.Action.ActionType, effectsCount, isBuff);
                effectsCount++;
            }
        }
    }

    private void CreateNewEffectUIObject(int amount, ActionType actionType, int currentEffectsCount, bool isBuffEffect)
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
        effectObject.GetComponent<EffectUIObject>().UpdateEffectUIObject(amount, CardManager.instance.GetActionSprite(actionType));
    }
}
