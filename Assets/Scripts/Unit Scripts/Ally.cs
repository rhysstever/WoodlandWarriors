using System.Collections.Generic;

public class Ally : Unit
{
    private List<Action> actions;

    public List<Action> Actions { get { return actions; } }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    public void SetHealth(int health)
    {
        maxLife = health;
        Reset();
    }

    public void SetActions(List<Action> actions)
    {
        this.actions = actions;
    }

    public void TakeDamage(int damage)
    {
        currentLife -= damage;
        UpdateLifeUIText();

        if (currentLife <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void Buff(int healthIncrease)
    {
        maxLife += healthIncrease;
        currentLife += healthIncrease;
        UpdateLifeUIText();
    }

    protected override void UpdateLifeUIText()
    {
        lifeText.text = currentLife.ToString();
    }
}
