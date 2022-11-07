using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Attacking : MonoBehaviour
{

    private Identifier identifier;

    [Header("Stats")]
    [SerializeField] private UnitStats stats;
    [SerializeField] private bool flamingAttacks = false;
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
    [SerializeField] private ParticleSystem startHitEffect;
    [SerializeField] private GameObject defaultHitEffect;
    [SerializeField] private GameObject wheatHitEffect;
    [SerializeField] private GameObject rockHitEffect;

    [Space(10)]


    [SerializeField] private bool debugShoot = false;
    [SerializeField] private Transform firePoint;


    private Transform nearestEnemy;
    private bool canAttack = true;
    private bool canAttackAddition = true;
    private float attackCounter = 10000f;

    //private float checkForEnemyCounter = 0;
    //private float checkForEnemyTime = .2f;
    private float attackAnimWaitTimeCounter = 1000f;
    private bool hasAttacked = false;

    //private List<Projectile> firedProjectiles = new List<Projectile>(4);

    public bool GetCanAttack => canAttack;
    public void SetCanAttack(bool addition) => canAttackAddition = addition;
    public void SetNearestEnemy (Transform newEnemy) => nearestEnemy = newEnemy;
    public Transform GetNearestEnemy => nearestEnemy;
    public UnitStats GetStats => stats;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Start()
    {
        if (startHitEffect)
            startHitEffect.Stop();
    }

    private void Update()
    {
        if (debugShoot)
        {
            Shoot();
            debugShoot = false;
        }

        HandleAttackingTime();
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
        canAttack = canAttackAddition && attackCounter > stats.timeBetweenAttacks;

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
        //FindNearestEnemy(transform.position);
    }

    private void Shoot ()
    {
        Transform target = nearestEnemy;

        // sometimes it's null. i don't think it matters too much if we don't fire every once in a while
        if (target == null)
            return;

        Projectile projInstance = Instantiate(stats.projectile, firePoint.position, Quaternion.identity).GetComponent<Projectile>();
        projInstance.SetInfo(stats.damage, identifier.GetPlayerID, identifier.GetTeamID, flamingAttacks);

        // attempt to find it's velocity (nav mesh agent first, then rigidbody)
        Vector3? targetVelocity = null;

        if (target.TryGetComponent(out NavMeshAgent enemyAgent))
            targetVelocity = enemyAgent.velocity;
        else if (target.TryGetComponent(out Rigidbody enemyBody))
            targetVelocity = enemyBody.velocity;

        Projectile.SetTrajectory(projInstance.GetRigidbody, target.position, stats.projectileForce, stats.accuracy, stats.projectileArch,
            stats.leadsTarget ? targetVelocity.GetValueOrDefault() : null);
    }

    public void MeleeHit()
    {
        if (startHitEffect)
            startHitEffect.Play();

        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * stats.hitDistance, stats.hitRadius, stats.hitMask);

        Collider resourceNode = hits.FirstOrDefault(hit => hit.TryGetComponent(out ResourceNode node));
        if (resourceNode != null)
        {
            ResourceNode node = resourceNode.GetComponent<ResourceNode>();
            if (node.TryCollectResources(out ResourceAmount returnedAmount))
            {
                int storageYardCount = PlayerHolder.GetBuildings(identifier.GetPlayerID).Count(building => building.GetStats.buildingType == BuyIcons.Building_StorageYard);
                ResourceAmount amtToAdd = new ResourceAmount(
                    returnedAmount.GetFood + (returnedAmount.GetFood > 0 ? storageYardCount : 0),
                    returnedAmount.GetWood + (returnedAmount.GetWood > 0 ? storageYardCount : 0),
                    returnedAmount.GetStone + (returnedAmount.GetStone > 0 ? storageYardCount : 0)
                    );
                PlayerResourceManager.instance.AddResourcesWithUI(identifier.GetPlayerID, amtToAdd, transform.position + new Vector3(0f, 1f, 0f));
                //PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].AddResources(
                //    returnedAmount.GetFood + (returnedAmount.GetFood > 0 ? storageYardCount : 0),
                //    returnedAmount.GetWood + (returnedAmount.GetWood > 0 ? storageYardCount : 0),
                //    returnedAmount.GetStone + (returnedAmount.GetStone > 0 ? storageYardCount : 0)
                //    );
            }
            //node.CollectResources(identifier.GetPlayerID);
        }

        Collider shakingObject = hits.FirstOrDefault(tree => tree.GetComponent <TreeShake>());
        if (shakingObject != null)
        {
            Vector3 dir = Vector3.Cross(shakingObject.transform.position - transform.position, Vector3.up).normalized;
            if (shakingObject.TryGetComponent (out TreeShake treeShake))
            {
                treeShake.ShakeOnce(-dir, stats.hitForce);
                GameObject hitEffect = Instantiate(treeShake.GetHitEffect, shakingObject.transform.position + treeShake.GetHitEffectOffset, Quaternion.identity);
                Destroy(hitEffect, 5f);
            }
        }

        Collider rock = hits.FirstOrDefault(hit => hit.CompareTag("Rock"));
        if (rock != null)
        {
            GameObject rockHitInstance = Instantiate(rockHitEffect, transform.position + transform.forward * stats.hitDistance,
                Quaternion.LookRotation(-transform.forward));
            Destroy(rockHitInstance, 2f);

            GameObject hitEffectInstance = Instantiate(defaultHitEffect, transform.position + transform.forward * stats.hitDistance, Quaternion.identity);
            Destroy(hitEffectInstance, 1f);

        }

        Collider wheat = hits.FirstOrDefault(hit => hit.CompareTag("Field"));
        if (wheat != null)
        {
            GameObject wheatHitInstance = Instantiate(wheatHitEffect, transform.position + transform.forward * stats.hitDistance, Quaternion.identity);
            Destroy(wheatHitInstance, 5f);
        }


        // hits unit with different team ID
        Collider[] hitEnemies = hits.Where(hit => hit.TryGetComponent(out Identifier otherId) && otherId.GetTeamID != identifier.GetTeamID).ToArray();
        if (hitEnemies.Length <= 0)
            return;

        // hit all enemies in radius
        if (stats.hitAllInRadius)
        {
            for (int i = 0; i < hitEnemies.Length; i++)
            {
                if (hitEnemies[i] != null)
                {
                    DamageEnemy(hitEnemies[i]);
                }
            }
        }
        else // hit first enemy in radius
        {
            Collider firstEnemy = hitEnemies.FirstOrDefault();
            DamageEnemy(firstEnemy);
        }


    }

    private void DamageEnemy (Collider enemy)
    {
        if (enemy != null)
        {
            // damage enemy unit
            if (enemy.TryGetComponent(out Health enemyHealth))
                enemyHealth.TakeDamage(stats.damage, identifier, transform.position);

            // don't spawn effects if we have too many units on the field
            if (PlayerHolder.GetUnits(identifier.GetPlayerID).Count > 100)
                if (Random.Range(0f, 1f) > 0.5f)
                    return;

            // hit effect
            if (defaultHitEffect)
            {
                GameObject hitEffectInstance = Instantiate(defaultHitEffect, transform.position + transform.forward * stats.hitDistance, Quaternion.identity);
                Destroy(hitEffectInstance, 1f);
            }

            if (flamingAttacks && enemy.TryGetComponent(out BurningObject burningHit))
            {
                burningHit.ResetBurn();
            }

            //unit.attachedRigidbody.AddForce((unit.transform.position - transform.position).normalized * attackingKnockbackForce, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!stats)
        {
            Debug.LogError(this + " needs a UnitStats assigned");
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
