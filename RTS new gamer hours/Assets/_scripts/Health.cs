using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float currentHealth = 100;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private bool invincible = false;

    private Identifier lastHitBy = null;
    private Vector3 lastHitFromPos = Vector3.zero;

    public float GetCurrentHealth => currentHealth;
    public float GetMaxHealth => maxHealth;
    public bool GetIsDead => currentHealth <= 0;
    public Identifier GetLastHitByPlayer => lastHitBy;
    public Vector3 GetLastHitFromPos => lastHitFromPos;

    public void SetValues (float maxHealth)
    {
        this.maxHealth = maxHealth;
    }
    public void TakeDamage (float damageAmt, Identifier hitBy, Vector3 hitFromPos)
    {
        lastHitBy = hitBy;
        lastHitFromPos = hitFromPos;   

        if (invincible)
            return;
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
