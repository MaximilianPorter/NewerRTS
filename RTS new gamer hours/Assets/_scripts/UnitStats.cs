using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Unit Name", menuName = "Create Unit Stats")]
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

    public float slowMultiplierBlocking = 0.2f;
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
    public float hitForce = 0.1f;

    [Header("Movement")]
    //public float moveForce = 1000f;
    public float maxMoveSpeed = 1f;
    //public float maxStepDistance = 0.2f;
    //public float legSwitchSpeed = 3.8f;
    //public float stopMovingDist = 0.02f;

}

