public class Player : Unit
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        Reset();
        unitSpriteRenderer.sprite = CharacterManager.instance.GetCharacterSprite(CharacterManager.instance.ChosenCharacter);
    }

    public override void Reset()
    {
        maxLife = 10;

        if(CharacterManager.instance.ChosenCharacter == Character.Beaver)
        {
            maxLife += 10;
        }

        base.Reset();
    }

    public override void DealDamage(int baseAttack, Unit target, DamageType damageType)
    {
        int amount = baseAttack;
        if(CharacterManager.instance.ChosenCharacter == Character.Badger)
        {
            amount++;
        }
        base.DealDamage(amount, target, damageType);
    }
    
    public override void TakeDamage(int amount, DamageType damageType)
    {
        TakeDamage(amount, null, damageType);
    }

    public override void TakeDamage(int amount, Unit attacker, DamageType damageType)
    {
        if(damageType == DamageType.Attack && CharacterManager.instance.Ally != null)
        {
            // If the ally exists, only damage the player the amount remaining after the ally is attacked
            int playerDamageAmount = amount - CharacterManager.instance.Ally.GetComponent<Ally>().CurrentLife;

            CharacterManager.instance.Ally.GetComponent<Ally>().TakeDamage(amount);
            base.TakeDamage(playerDamageAmount, attacker, damageType);
        }
        else
        {
            base.TakeDamage(amount, attacker, damageType);
        }

        // Check if the player has been killed, if so, end the game
        if(currentLife <= 0)
        {
            currentLife = 0;
            UpdateLifeUIText();
            AudioManager.instance.PlayDeathAudio();
            GameManager.instance.ChangeMenuState(MenuState.GameEnd);
        }
    }

    public override void Heal(int baseHeal)
    {
        int amount = baseHeal;
        if(CharacterManager.instance.ChosenCharacter == Character.Fox)
        {
            amount++;
        }
        base.Heal(amount);
    }

    public void HealFull()
    {
        Heal(maxLife);
    }
}