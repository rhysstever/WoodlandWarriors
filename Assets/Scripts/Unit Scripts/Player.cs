using UnityEngine;

public class Player : Unit
{
    protected int currentGold;

    [SerializeField]
    private SpriteRenderer characterSpriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        Reset();
        characterSpriteRenderer.sprite = CharacterManager.instance.GetCharacterSprite(CharacterManager.instance.ChosenCharacter);
    }

    public override void Reset()
    {
        maxLife = 10;

        if(CharacterManager.instance.ChosenCharacter == Character.Beaver)
        {
            maxLife += 10;
        }

        currentGold = 0;
        base.Reset();
    }

    public override void TakeDamage(int amount, DamageType damageType)
    {
        base.TakeDamage(amount, null, damageType);
    }

    public override void TakeDamage(int amount, Unit attacker, DamageType damageType)
    {
        base.TakeDamage(amount, attacker, damageType);

        // Check if the player has been killed, if so, end the game
        if(currentLife <= 0)
        {
            currentLife = 0;
            UpdateLifeUIText();
            AudioManager.instance.PlayDeathAudio();
            GameManager.instance.ChangeMenuState(MenuState.GameEnd);
        }
    }

    public bool CanAfford(int amount)
    {
        return currentGold >= amount;
    }

    public void SpendGold(int amount)
    {
        if(amount < 0 || !CanAfford(amount))
        {
            return;
        }

        currentGold -= amount;
    }

    public void GiveGold(int amount)
    {
        if(amount < 0)
        {
            return;
        }

        currentGold += amount;
    }

    public void HealFull()
    {
        Heal(maxLife);
    }
}
