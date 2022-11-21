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
    [SerializeField] private GameObject projectile;
    [SerializeField] private bool flamingAttacks = false;

    [Header("Physics")]
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask obstacleVisionMask;

    [Header("Effects")]
    [SerializeField] private ParticleSystem startHitEffect;
    [SerializeField] private GameObject defaultHitEffect;
    [SerializeField] private GameObject wheatHitEffect;
    [SerializeField] private GameObject rockHitEffect;

    private Vector3 startHitEffectStartScale;

    [Space(10)]


    [SerializeField] private bool debugShoot = false;
    [SerializeField] private Transform firePoint;


    private NavMeshMovement navMovement;
    private CellIdentifier nearestCellEnemy;
    private CellIdentifier cellIdentifier;
    private bool canAttack = true;
    private bool canAttackAddition = true;
    private float attackCounter = 10000f;
    private float lookRangeWithHeight = 0f;
    private float attackRangeWithHeight = 0f;

    //private float checkForEnemyCounter = 0;
    //private float checkForEnemyTime = .2f;
    private float attackAnimWaitTimeCounter = 1000f;
    private float findNearestEnemyCounter = 0f;
    private bool hasAttacked = false;
    private bool canLookForEnemies = true;

    //private List<Projectile> firedProjectiles = new List<Projectile>(4);

    public void SetCanLookForEnemies(bool canLookForEnemies) => this.canLookForEnemies = canLookForEnemies;
    public bool GetCanAttack => canAttack;
    public void SetCanAttack(bool addition) => canAttackAddition = addition;
    public void SetNearestEnemy (CellIdentifier newEnemy) => nearestCellEnemy = newEnemy;
    public CellIdentifier GetNearestEnemy => nearestCellEnemy;
    public UnitStats GetStats => stats;
    public float AttackRangeWithHeight => attackRangeWithHeight;
    public float LookRangeWithHeight => lookRangeWithHeight;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        navMovement = GetComponent<NavMeshMovement>();
        cellIdentifier = GetComponent<CellIdentifier>();
    }

    private void Start()
    {
        if (startHitEffect)
        {
            startHitEffect.Stop();
            startHitEffectStartScale = startHitEffect.transform.localScale;
        }

    }

    private void Update()
    {
        // RESEARCH : MAGE LARGER ATTACKS
        float attackRange = stats.unitType == BuyIcons.Unit_Mage && identifier.GetPlayerID > -1 && PlayerHolder.GetCompletedResearch(identifier.GetPlayerID).Contains(BuyIcons.Research_LargerMageAttacks) ?
            stats.attackRange * 1.3f :
            stats.attackRange;

        // increase look distace and attack distance with height up to 3x
        if (stats.isRanged)
        {
            float heightMultiplier = 1.8f;
            lookRangeWithHeight = stats.lookRange + Mathf.Clamp((-navMovement.GetBaseOffset + transform.position.y) * heightMultiplier, 0f, stats.lookRange * 2f);
            attackRangeWithHeight = attackRange + Mathf.Clamp((-navMovement.GetBaseOffset + transform.position.y) * heightMultiplier, 0f, attackRange * 2f);
        }
        else
        {
            lookRangeWithHeight = stats.lookRange;
            attackRangeWithHeight = attackRange;
        }

        if (startHitEffect)
        {
            // RESEARCH : MAGE LARGER ATTACKS
            if (stats.unitType == BuyIcons.Unit_Mage && identifier.GetPlayerID > -1 && PlayerHolder.GetCompletedResearch(identifier.GetPlayerID).Contains(BuyIcons.Research_LargerMageAttacks))
            {
                startHitEffect.transform.localScale = startHitEffectStartScale * 1.3f;
            }
            else
            {
                startHitEffect.transform.localScale = startHitEffectStartScale;
            }
        }


        if (debugShoot)
        {
            Shoot();
            debugShoot = false;
        }

        HandleAttackingTime();


        findNearestEnemyCounter -= Time.deltaTime;
        if (findNearestEnemyCounter <= 0 && canLookForEnemies)
        {
            CellFindNearestEnemy();
            findNearestEnemyCounter = Random.Range(0.1f, 0.3f);
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

    private void CellFindNearestEnemy()
    {
        int cellsOutToCheck = Mathf.CeilToInt(lookRangeWithHeight / UnitCellManager.cellWidth);
        Cell activeCell = UnitCellManager.GetCell(transform.position);

        Vector2Int bottomLeft = new Vector2Int(activeCell.pos.x - cellsOutToCheck, activeCell.pos.y - cellsOutToCheck);
        Vector2Int topRight = new Vector2Int(activeCell.pos.x + cellsOutToCheck, activeCell.pos.y + cellsOutToCheck);
        Cell[] cellsAroundMe = UnitCellManager.GetCells(bottomLeft, topRight);


        SetNearestEnemy(ReturnClosestEnemy(cellsAroundMe));
    }

    private CellIdentifier ReturnClosestEnemy(Cell[] cellsToCheck)
    {
        CellIdentifier closestEnemy = null;

        //int totalUnitsAroundMe = cellsToCheck.Sum(cell => cell.unitsInCell.Count);

        for (int i = 0; i < cellsToCheck.Length; i++)
        {
            if (cellsToCheck[i] == null)
                continue;


            foreach (CellIdentifier unit in cellsToCheck[i].unitsInCell)
            {
                if (unit == null)
                    continue;

                // if the enemy is not part of the enemy mask
                if (enemyMask != (enemyMask | (1 << unit.gameObject.layer)))
                    continue;

                if (unit == identifier) // don't look for self
                    continue;

                // if the unit isn't targetable
                if (unit.GetIdentifier && unit.GetIdentifier.GetIsTargetable == false)
                    continue;

                // if the unit is on the same team as you
                if (unit.GetIdentifier.GetTeamID == identifier.GetTeamID)
                    continue;

                // if you're not a battering ram
                if (stats.unitType != BuyIcons.Unit_BatteringRam && unit.GetBuilding && unit.GetBuilding.GetIsWall)
                    continue;

                // if there's something in the way
                if (Physics.Raycast(transform.position, unit.transform.position - transform.position, stats.lookRange, obstacleVisionMask))
                    continue;

                Vector3 unitDir = (unit.transform.position - transform.position);
                unitDir.y = 0; // height doesn't matter

                if (unitDir.sqrMagnitude > lookRangeWithHeight * lookRangeWithHeight)
                {
                    continue;
                }


                if (closestEnemy == null)
                    closestEnemy = unit;
                else if (unitDir.sqrMagnitude < (closestEnemy.transform.position - transform.position).sqrMagnitude)
                    closestEnemy = unit;
            }




            // if we already found an enmy in the first 12 squares, there's no point to keep checking
            if (i >= 12 && closestEnemy != null)
                break;
        }

        return closestEnemy;
    }

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
        Transform target = nearestCellEnemy ? nearestCellEnemy.transform : null;

        // sometimes it's null. i don't think it matters too much if we don't fire every once in a while
        if (target == null)
            return;

        Projectile projInstance = Instantiate(projectile, firePoint.position, Quaternion.identity).GetComponent<Projectile>();
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

        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * stats.GetHitCenterDist, stats.GetHitRadius, stats.hitMask);

        foreach (Collider hit in hits)
        {
            if (hit.transform == transform)
                continue;

            if (hit.TryGetComponent (out ResourceNode node))
            {
                if (identifier.GetPlayerID < 0)
                    continue;
                if (node.GetIsReward) // don't collect if this resource node is being used as a reward (ie. bear)
                    continue;

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

            }

            if (hit.TryGetComponent (out TreeShake treeShake))
            {
                Vector3 dir = Vector3.Cross(treeShake.transform.position - transform.position, Vector3.up).normalized;
                treeShake.ShakeOnce(-dir, 0.1f);
                GameObject hitEffect = Instantiate(treeShake.GetHitEffect, treeShake.transform.position + treeShake.GetHitEffectOffset, Quaternion.identity);
                Destroy(hitEffect, 5f);

            }


            if (hit.CompareTag("Rock"))
            {
                GameObject rockHitInstance = Instantiate(rockHitEffect, transform.position + transform.forward * stats.GetHitCenterDist,
                    Quaternion.LookRotation(-transform.forward));
                Destroy(rockHitInstance, 2f);

                GameObject hitEffectInstance = Instantiate(defaultHitEffect, transform.position + transform.forward * stats.GetHitCenterDist, Quaternion.identity);
                Destroy(hitEffectInstance, 1f);

            }

            if (hit.CompareTag("Field"))
            {
                GameObject wheatHitInstance = Instantiate(wheatHitEffect, transform.position + transform.forward * stats.GetHitCenterDist, Quaternion.identity);
                Destroy(wheatHitInstance, 5f);

            }
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
            if (firstEnemy != null)
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

            if (identifier.GetPlayerID > -1)
            {
                // don't spawn effects if we have too many units on the field
                if (PlayerHolder.GetUnits(identifier.GetPlayerID).Count > 100)
                    if (Random.Range(0f, 1f) > 0.5f)
                        return;
            }

            // hit effect
            if (defaultHitEffect)
            {
                GameObject hitEffectInstance = Instantiate(defaultHitEffect, transform.position + transform.forward * stats.GetHitCenterDist, Quaternion.identity);
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
            Gizmos.DrawWireSphere (transform.position + transform.forward * stats.GetHitCenterDist, stats.GetHitRadius);
        }
    }
}
