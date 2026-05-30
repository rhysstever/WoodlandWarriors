using UnityEngine;

public class Ally : MonoBehaviour
{
    [SerializeField]
    private int maxHealth, currentHealth;

    public int MaxHealth { get { return maxHealth; } }

    private void Start()
    {
        
    }

    public void SetHealth(int health)
    {
        maxHealth = health;
        currentHealth = health;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {

            Destroy(gameObject);
        }
    }

    public void Buff(int healthIncrease)
    {
        maxHealth += healthIncrease;
        currentHealth += healthIncrease;
    }
}
