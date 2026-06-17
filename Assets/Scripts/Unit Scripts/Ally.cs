using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ally : MonoBehaviour
{
    [SerializeField]
    private TMP_Text lifeText;

    private int maxHealth, currentHealth;
    private List<Action> actions;

    public int MaxHealth { get { return maxHealth; } }
    public int CurrentHealth { get { return currentHealth; } }
    public List<Action> Actions { get { return actions; } }

    private void Start()
    {
        
    }

    public void SetHealth(int health)
    {
        maxHealth = health;
        currentHealth = health;
        UpdateLifeUIText();
    }

    public void SetActions(List<Action> actions)
    {
        this.actions = actions;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateLifeUIText();

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void Buff(int healthIncrease)
    {
        maxHealth += healthIncrease;
        currentHealth += healthIncrease;
        UpdateLifeUIText();
    }

    protected void UpdateLifeUIText()
    {
        lifeText.text = currentHealth.ToString();
    }
}
