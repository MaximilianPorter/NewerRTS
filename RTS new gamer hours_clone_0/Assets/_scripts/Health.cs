using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100;
    [SerializeField][Tooltip("Flat damage reduction")] private float armor = 0;
    [SerializeField] private bool invincible = false;

    private Identifier lastHitBy = null;
    private Vector3 lastHitFromPos = Vector3.zero;
    private float currentHealth = 100;

    public float GetCurrentHealth => currentHealth;
    public float GetMaxHealth => maxHealth;
    public bool GetIsDead => currentHealth <= 0;
    public Identifier GetLastHitByPlayer => lastHitBy;
    public Vector3 GetLastHitFromPos => lastHitFromPos;

    public void SetValues (float maxHealth, float armor)
    {
        this.currentHealth = maxHealth;
        this.maxHealth = maxHealth;
        this.armor = armor;
    }
    public void TakeDamage (float damageAmt, Identifier hitBy, Vector3 hitFromPos, bool negatesArmor = false)
    {
        lastHitBy = hitBy;
        lastHitFromPos = hitFromPos;   

        if (invincible)
            return;

        if (!negatesArmor)
        {
            if (armor > 0)
                damageAmt = Mathf.Max(1, damageAmt - armor);
        }

        currentHealth -= damageAmt;
    }

    public void Heal (float amtToHeal, bool overheal = false)
    {
        currentHealth = Mathf.Clamp(currentHealth + amtToHeal, 0f, overheal ? Mathf.Infinity : maxHealth);
    }

    public void ResetHealth ()
    {
        currentHealth = maxHealth;
        lastHitBy = null;
        lastHitFromPos = Vector3.zero;
    }

}