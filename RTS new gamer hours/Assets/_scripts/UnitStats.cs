using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Unit Name", menuName = "Create Unit Stats")]
public class UnitStats : ScriptableObject
{
    public string unitName = "developer man forgot to name this guy";
    public string unitDescription = "really cool unit type that the developer totally remembered to describe";
    public float health;
    public float damage;
    public float timeBetweenAttacks;
    public float slowMultiplierBlocking = 0.2f;

    [Header("Ranged")]
    public bool isRanged = false;
    public float range = 5f;
    public float projectileForce;
    public GameObject projectile;

    [Header("Movement")]
    public float moveForce = 1000f;
    public float maxMoveSpeed = 1f;
    public float maxStepDistance = 0.2f;
    public float legSwitchSpeed = 3.8f;
    public float stopMovingDist = 0.02f;
}
