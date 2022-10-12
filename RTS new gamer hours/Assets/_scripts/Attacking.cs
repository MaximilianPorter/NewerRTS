using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Attacking : MonoBehaviour
{

    private Identifier identifier;

    [Header("Stats")]
    [SerializeField] private UnitStats stats;
    //[SerializeField] private bool isRanged = true;
    //[SerializeField] private float lookRange = 5f;
    //[SerializeField] private float attackRange = 5f;
    //[SerializeField] private float timeBetweenAttacks = 0.5f;
    //[SerializeField] private float damage = 1f;
    //[SerializeField] private float projectileForce;
    //[SerializeField] private GameObject projectile;
    //[SerializeField] private float slowMultiplierBlocking = 0.2f;
    //[SerializeField] private LayerMask hitMask;
    //[SerializeField] private float hitDistance = 0.2f;
    //[SerializeField] private float hitRadius = 2f;
    //[SerializeField] private float hitForce = 0.1f;

    [Header("Effects")]
    [SerializeField] private GameObject defaultHitEffect;
    [SerializeField] private GameObject woodHitEffect;
    [SerializeField] private GameObject wheatHitEffect;

    [Space(10)]


    [SerializeField] private bool debugShoot = false;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask enemyMask;


    private Movement movement;
    private Transform nearestEnemy;
    private bool canAttack = true;
    private float attackCounter = 10000f;

    private float checkForEnemyCounter = 0;
    private float checkForEnemyTime = .2f;
    private float attackAnimWaitTimeCounter = 1000f;
    private bool hasAttacked = false;

    public bool GetCanAttack => canAttack;
    public void SetNearestEnemy (Transform newEnemy) => nearestEnemy = newEnemy;
    public Transform GetNearestEnemy => nearestEnemy;
    public UnitStats GetStats => stats;
    public float GetLookRange => stats.lookRange;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        movement = GetComponent<Movement>();
    }

    private void Update()
    {
        if (debugShoot)
        {
            Shoot();
            debugShoot = false;
        }

        if (!identifier.GetIsPlayer)
        {
            //NonPlayerBehaviour();
        }

        HandleAttackingTime();
    }

    private void FixedUpdate()
    {
        if (!identifier.GetIsPlayer)
        {
            checkForEnemyCounter += Time.fixedDeltaTime;
            if (checkForEnemyCounter > checkForEnemyTime)// || nearestEnemy == null)
            {
                FindNearestEnemy(movement.GetMoveTarget);
                checkForEnemyCounter = 0f;
                checkForEnemyTime = Random.Range(0.1f, 0.3f);
            }
        }
    }

    //private void NonPlayerBehaviour()
    //{
    //    if (nearestEnemy != null)
    //    {
    //        //// if the place we're going to is already within (range) of the target, stop moving and look at the target
    //        //if ((movement.GetMoveTarget - nearestEnemy.position).sqrMagnitude < (stats.lookRange * stats.lookRange))
    //        //{
    //        //    movement.SetLookAt(nearestEnemy);
    //        //    movement.SetMoveTarget(transform.position);
    //        //    return;
    //        //}
    //        //else
    //        //    movement.SetLookAt(null);

    //        // if the point we're moving to is far enough away (lookrange / 2) don't pay attention to any enemies nearby
    //        if ((movement.GetMoveTarget - transform.position).sqrMagnitude > (stats.lookRange * stats.lookRange) / 2f)
    //        {
    //            movement.SetLookAt(null);
    //            return;
    //        }
    //        else if (movement.GetLookTarget != nearestEnemy)
    //        {
    //            // if the point we're moving to is close enough
    //            movement.SetLookAt(nearestEnemy);
    //            movement.SetMoveTarget (transform.position);
    //        }

    //        if (movement.GetHasReachedMovePos && nearestEnemy.TryGetComponent(out Building nearestBuilding))
    //        {
    //            Vector3 buildingDir = (nearestBuilding.transform.position - transform.position);
    //            bool isWithinDist = buildingDir.sqrMagnitude <= nearestBuilding.GetStats.interactionRadius * nearestBuilding.GetStats.interactionRadius;
    //            if (!isWithinDist)
    //            {
    //                Vector3 moveTarget = nearestBuilding.transform.position - buildingDir.normalized * nearestBuilding.GetStats.interactionRadius;
    //                moveTarget.y = transform.position.y;
    //                movement.SetMoveTarget(moveTarget);
    //            }
    //        }
    //        else if (movement.GetHasReachedMovePos && 
    //            (transform.position - nearestEnemy.position).sqrMagnitude > stats.attackRange * stats.attackRange) // don't worry about pos if you're already in attacking range
    //        {
    //            // chase after the enemy until they can attack, only if they've reached their original desired position

    //            // pos = enemy + attackrange / 4
    //            // not exactly on the enemy position, but close enough to attack
    //            movement.SetMoveTarget(nearestEnemy.position + (transform.position - nearestEnemy.position).normalized * (stats.attackRange * 0.75f));


    //            // if canAttack and there's an enemy IN RANGE
    //            if (canAttack && (transform.position - nearestEnemy.position).sqrMagnitude < stats.attackRange * stats.attackRange)
    //            {
    //                //Attack();
    //                hasLoadedAttack = true;
    //                attackCounter = 0f;
    //            }
    //        }

    //    }
    //    else
    //    {
    //        movement.SetLookAt(null);

    //    }
    //}

    private void HandleAttackingTime ()
    {
        attackCounter += Time.deltaTime;
        attackAnimWaitTimeCounter -= Time.deltaTime;

        // can attack if your timer is ready and you're not moving
        canAttack = attackCounter > stats.timeBetweenAttacks && (identifier.GetIsPlayer || movement.GetMoveSpeed01 < 0.01f); 

        if (attackAnimWaitTimeCounter < 0 && !hasAttacked)
            SendAttack();
    }
    public void SetAttackWaitTime (float waitTime)
    {
        attackCounter = 0f;
        hasAttacked = false;

        attackAnimWaitTimeCounter = waitTime;
    }

    private void SendAttack ()
    {
        hasAttacked = true;
        if (stats.isRanged)
        {
            Shoot();
        }
        else
        {
            MeleeHit();
            //swordAnimator.SetTrigger("Attack");
        }
        FindNearestEnemy(transform.position);
    }

    private void FindNearestEnemy (Vector3 fromPoint)
    {
        //Collider[] nearbyUnits = Physics.OverlapSphere(fromPoint, stats.lookRange, enemyMask).Where(unit => unit.GetComponent<Identifier>().GetTeamID != identifier.GetTeamID).ToArray();

        //if (nearbyUnits.Length <= 0)
        //{
        //    nearestEnemy = null;
        //    return;
        //}

        //foreach (Collider enemyCol in nearbyUnits)
        //{
        //    if (nearestEnemy == null || (enemyCol.transform.position - fromPoint).sqrMagnitude <
        //        (nearestEnemy.position - fromPoint).sqrMagnitude)
        //    {
        //        nearestEnemy = enemyCol.transform;
        //    }
        //}
    }

    private void Shoot ()
    {
        // sometimes it's null. i don't think it matters too much if we don't fire every once in a while
        if (movement.GetLookTarget == null)
            return;

        Projectile projInstance = Instantiate(stats.projectile, firePoint.position, Quaternion.identity).GetComponent<Projectile>();
        projInstance.SetInfo(stats.damage, identifier.GetTeamID);

        Projectile.SetTrajectory(projInstance.GetRigidbody, movement.GetLookTarget.position, stats.projectileForce, stats.accuracy, stats.projectileArch,
            stats.leadsTarget ? movement.GetLookTarget.GetComponent<Rigidbody>() : null);
    }

    public void MeleeHit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * stats.hitDistance, stats.hitRadius, stats.hitMask);

        Collider firstTree = hits.FirstOrDefault(tree => tree.CompareTag("Tree"));
        if (firstTree != null)
        {
            Vector3 dir = Vector3.Cross(firstTree.transform.position - transform.position, Vector3.up).normalized;
            firstTree.GetComponent<TreeShake>().ShakeOnce(-dir, stats.hitForce);
            GameObject woodHitInstance = Instantiate(woodHitEffect, firstTree.transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity);
            Destroy(woodHitInstance, 5f);
        }

        Collider wheat = hits.FirstOrDefault(hit => hit.CompareTag("Field"));
        if (wheat != null)
        {
            GameObject wheatHitInstance = Instantiate(wheatHitEffect, transform.position + transform.forward * stats.hitDistance, Quaternion.identity);
            Destroy(wheatHitInstance, 5f);
        }


        // hits unit with different team ID
        Collider enemy = hits.FirstOrDefault(hit => hit.TryGetComponent(out Identifier otherId) && otherId.GetTeamID != identifier.GetTeamID);
        if (enemy != null)
        {
            // damage enemy unit
            if (enemy.TryGetComponent(out Health enemyHealth))
                enemyHealth.TakeDamage(stats.damage);

            // don't spawn effects if we have too many units on the field
            if (PlayerHolder.GetUnits(identifier.GetPlayerID).Count > 100)
                if (Random.Range(0f, 1f) > 0.5f)
                    return;

            // hit effect
            GameObject hitEffectInstance = Instantiate(defaultHitEffect, transform.position + transform.forward * stats.hitDistance, Quaternion.identity);
            Destroy(hitEffectInstance, 1f);

            //unit.attachedRigidbody.AddForce((unit.transform.position - transform.position).normalized * attackingKnockbackForce, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!stats)
        {
            Debug.Log(this + " needs a UnitStats assigned");
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stats.lookRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stats.attackRange);

        if (!stats.isRanged)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere (transform.position + transform.forward * stats.hitDistance, stats.hitRadius);
        }
    }
}
