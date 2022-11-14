using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Unit Name", menuName = "Create Unit Stats")]
[System.Serializable]
public class UnitStats : ScriptableObject
{
    public BuyIcons unitType;

    // change these variables in database https://console.firebase.google.com/u/3/project/rts-castles/database/rts-castles-default-rtdb/data
    [Header("Changed in Database")]
    public float health;
    public float armor;
    public float damage;
    public float timeBetweenAttacks;
    public float lookRange = 5f;
    public float attackRange = 1f;
    public ResourceAmount cost;

    [Space(10)]

    public bool regularAttackBuildings = false;

    [Header("Ranged")]
    public bool isRanged = false;
    public float projectileForce;
    [Tooltip("a percentage chance for if they will hit")]
    public float accuracy = 0f;
    public bool leadsTarget = false;
    [Range(0f, 1f)]public float projectileArch = 0.5f;
    public GameObject projectile;

    [Header("Melee")]
    public LayerMask hitMask;
    public bool hitAllInRadius = false;
    [Range (0f, 1f)] [SerializeField] private float hitDistance = 0.5f;
    public float GetHitCenterDist => hitDistance * attackRange;
    public float GetHitRadius => hitDistance > 0.5f ? Mathf.Lerp(0f, attackRange, hitDistance) : Mathf.Lerp(attackRange, 0f, hitDistance);//Mathf.Lerp(attackRange, 0f, Mathf.Clamp(hitDistance, 0f, 0.5f)) + Mathf.Lerp (0f, attackRange/2f, hitDistance > 0.5f ? hitDistance : 0f);

    [Header("Movement")]
    public float maxMoveSpeed = 1f;

    public UnitStats(UnitStats newStats)
    {
        this.unitType = newStats.unitType;
        this.health = newStats.health;
        this.armor = newStats.armor;
        this.damage = newStats.damage;
        this.timeBetweenAttacks = newStats.timeBetweenAttacks;
        this.lookRange = newStats.lookRange;
        this.attackRange = newStats.attackRange;
        this.cost = newStats.cost;
        this.regularAttackBuildings = newStats.regularAttackBuildings;
        this.isRanged = newStats.isRanged;
        this.projectileForce = newStats.projectileForce;
        this.accuracy = newStats.accuracy;
        this.leadsTarget = newStats.leadsTarget;
        this.projectileArch = newStats.projectileArch;
        this.projectile = newStats.projectile;
        this.hitMask = newStats.hitMask;
        this.hitAllInRadius = newStats.hitAllInRadius;
        this.hitDistance = newStats.hitDistance;
        this.maxMoveSpeed = newStats.maxMoveSpeed;
    }
}

