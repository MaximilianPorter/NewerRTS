using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float currentHealth = 100;
    [SerializeField] private float maxHealth = 100;

    private int lastHitByPlayer = -1;
    private Vector3 lastHitFromPos;

    public float GetCurrentHealth => currentHealth;
    public float GetMaxHealth => maxHealth;
    public bool GetIsDead => currentHealth <= 0;
    public int GetLastHitByPlayer => lastHitByPlayer;
    public Vector3 GetLastHitFromPos => lastHitFromPos;

    public void SetValues (float maxHealth)
    {
        this.maxHealth = maxHealth;
    }
    public void TakeDamage (float damageAmt, int hitByPlayerID, Vector3 hitFromPos)
    {
        currentHealth -= damageAmt;
        lastHitByPlayer = hitByPlayerID;
        lastHitFromPos = hitFromPos;   
    }

    /// <summary>
    /// doesn't overheal
    /// </summary>
    public void Heal (float amtToHeal)
    {
        currentHealth = Mathf.Clamp(currentHealth + amtToHeal, -1, maxHealth);
    }

}
