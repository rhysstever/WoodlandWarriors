public class Effect
{
    private string effectName;
    private int effectAmount;
    private bool isAffliction;
    private Effect buffingEffect;

    public string EffectName { get { return effectName; } }
    public int Amount { get { return effectAmount; } } 
    public bool IsAffliction { get { return isAffliction; } }

    public Effect(string effectName, int effectAmount, bool isAffliction, Effect buffingEffect)
    {
        this.effectName = effectName;
        this.effectAmount = effectAmount;
        this.isAffliction = isAffliction;
        this.buffingEffect = buffingEffect;
    }

    public Effect(string effectName, int effectAmount, bool isAffliction)
    {
        this.effectName = effectName;
        this.effectAmount = effectAmount;
        this.isAffliction = isAffliction;
        this.buffingEffect = null;
    }

    public void UpdateAmount(int amount)
    {
        effectAmount += amount;

        if(buffingEffect != null)
        {
            effectAmount += buffingEffect.Amount;
        }

        if(effectAmount < 0)
        {
            effectAmount = 0;
        }
    }

    public void Reset()
    {
        effectAmount = 0;
    }
}
