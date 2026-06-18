public class Effect
{
    private Action action;
    private bool isAffliction;
    private Effect buffingEffect;

    public bool IsAffliction { get { return isAffliction; } }
    public Action Action { get { return action; } }

    public Effect(Action action, bool isAffliction, Effect buffingEffect)
    {
        this.isAffliction = isAffliction;
        this.action = action;
        this.buffingEffect = buffingEffect;
    }

    public Effect(Action action, bool isAffliction)
    {
        this.isAffliction = isAffliction;
        this.action = action;
        this.buffingEffect = null;
    }

    public void UpdateAmount(int amount)
    {
        int totalAmount = Action.Amount + amount;
        if(buffingEffect != null)
        {
            totalAmount += buffingEffect.Action.Amount;
        }

        if(totalAmount < 0)
        {
            totalAmount = 0;
        }

        if(action is Buff)
        {
            action = new Buff(action.ActionType, totalAmount);
        }
        else
        {
            action = new Action(action.ActionType, totalAmount, action.TargetType);
        }
    }

    public void Reset()
    {
        if(action is Buff)
        {
            action = new Buff(action.ActionType, 0);
        }
        else
        {
            action = new Action(action.ActionType, 0, action.TargetType);
        }
    }
}
