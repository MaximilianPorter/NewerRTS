using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof (NavMeshAgent))]
public class AnimalActions : MonoBehaviour
{
    private enum AnimalBehaviour
    {
        PASSIVE,
        AGGRESSIVE,
    }

    [Header("Stats")]
    [SerializeField] private UnitStats animalStats;
    [SerializeField] private AnimalBehaviour animalBehaviour = AnimalBehaviour.PASSIVE;
    [SerializeField] private Vector2 timeBetweenDecisions = new Vector2(0.5f, 10f);
    [SerializeField] private float roamRange = 5f;
    [SerializeField] private float maxLeaveRange = 10f;
    [SerializeField] private float runMultiplier = 2f;
    [SerializeField] private Attacking attacking;
    [SerializeField] private float attackWaitTime = 0.5f;
    //[SerializeField] private int resourceGiveAmt = 5;

    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject spawnEffect;
    [SerializeField] private bool destroyGameobjectOnDeath = false;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private AnimatorOverrideController animalOverrideController;
    [SerializeField] private float walkAnimSpeed = 1f;
    [SerializeField] private float runAnimSpeed = 1f;

    private float lastMaxHealth = 0f;
    private Health health;
    private CellIdentifier cellIdentifier;
    private CellIdentifier target;
    private NavMeshMovement navMovement;
    private float decisionCounter = 0f;
    private Vector3 startPos;
    private float debugMoveSpeedDisplay;
    private bool justSpawned = false;
    private bool goBackToSpawn = false;
    private bool isRunning = false;
    private bool isAttacking = false;
    private Cell lastCell;

    private void Awake()
    {
        SetAnimations(animalOverrideController);
        navMovement = GetComponent<NavMeshMovement>();
        health = GetComponent<Health>();
        cellIdentifier = GetComponent<CellIdentifier>();
    }

    private void Start()
    {
        if (animalStats)
            health.SetValues(animalStats.health, animalStats.armor);

        lastMaxHealth = health.GetMaxHealth;
        startPos = transform.position;
    }

    private void Update()
    {
        if (!justSpawned)
        {
            GameObject spawnEffectInstance = Instantiate(spawnEffect, transform.position, Quaternion.identity);
            Destroy(spawnEffectInstance, 5f);
            justSpawned = true;
        }

        debugMoveSpeedDisplay = navMovement.GetAgentVelocity.magnitude / (animalStats.maxMoveSpeed * runMultiplier);

        decisionCounter -= Time.deltaTime;

        HandleReturningToSpawn();
        HandleAnimationSpeed();

        if (!goBackToSpawn)
            HandleBehaviour();

        if (health && health.GetCurrentHealth <= 0)
        {
            Die();
        }

        UnitCellManager.UpdateActiveCell(cellIdentifier, transform.position, ref lastCell);
    }

    private void HandleReturningToSpawn ()
    {
        // too far away, go back to spawn point
        if (Vector3.Distance(transform.position, startPos) > maxLeaveRange)
        {
            goBackToSpawn = true;
            target = null;
            navMovement.SetDestination(startPos);
        }
        else if (goBackToSpawn)
        {
            if (Vector3.Distance(transform.position, startPos) < 1f || navMovement.GetMoveSpeed01 < 0.1f)
            {
                goBackToSpawn = false;

                // reset target
                if (animalBehaviour == AnimalBehaviour.AGGRESSIVE)
                {
                    attacking.SetCanLookForEnemies(false);
                    lastMaxHealth = health.GetCurrentHealth;
                }
            }
        }
    }
    private void HandleAnimationSpeed ()
    {
        animator.SetBool("isWalking", navMovement.GetMoveSpeed01 > 0.01f && !isRunning);
        animator.SetBool("isRunning", navMovement.GetMoveSpeed01 > 0.01f && isRunning);

        if (navMovement.GetMoveSpeed01 > 0.01f)
        {
            if (!isRunning)
            {
                animator.speed = walkAnimSpeed * navMovement.GetMoveSpeed01;
            }
            else
            {
                animator.speed = runAnimSpeed * navMovement.GetMoveSpeed01;
            }

        }
        else
            animator.speed = 1f;
    }

    private void HandleBehaviour ()
    {
        if (animalBehaviour == AnimalBehaviour.PASSIVE)
        {
            if (decisionCounter < 0f && navMovement.GetAgentVelocity.magnitude <= 0f)
            {
                MakePassiveDecision();
            }
        }
        else if (animalBehaviour == AnimalBehaviour.AGGRESSIVE)
        {
            isAttacking = animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
            navMovement.SetMoveSpeed(target == null ? animalStats.maxMoveSpeed : animalStats.maxMoveSpeed * runMultiplier);

            // look for enemies if we got attacked
            if (health.GetCurrentHealth < lastMaxHealth)
            {
                attacking.SetCanLookForEnemies(true);
                target = attacking.GetNearestEnemy;
            }

            isRunning = target != null;

            if (target == null)
            {
                if (decisionCounter < 0f && navMovement.GetAgentVelocity.magnitude <= 0f)
                {
                    MakePassiveDecision();
                }
            }
            else
            {
                // reset bools
                animator.SetBool("isSitting", false);
                animator.SetBool("isEating", false);


                if (Vector3.Distance(target.transform.position, transform.position) > animalStats.attackRange)
                    navMovement.SetDestination(target.transform.position);
                else
                {
                    navMovement.ResetDestination();
                    navMovement.SetLookAt(target.transform);

                    if (attacking.GetCanAttack)
                    {
                        Attack();
                    }
                }
            }
        }
    }

    private void MakePassiveDecision ()
    {
        decisionCounter = Random.Range(timeBetweenDecisions.x, timeBetweenDecisions.y);

        // if we're still moving, just return
        if (navMovement.GetAgentVelocity.magnitude > 0.01f)
            return;

        // make decision
        float decision = Random.Range(0f, 1f);

        // reset bools
        animator.SetBool("isSitting", false);
        animator.SetBool("isEating", false);

        if (decision > 0.7f)
        {
            // walk around
            navMovement.SetDestination(startPos + new Vector3(Random.Range(-roamRange, roamRange), 0f, Random.Range(-roamRange, roamRange)));
        }
        else if (decision > 0.4f)
        {
            // sit down
            animator.SetBool("isSitting", true);
            decision *= 2f;
        }
        else if (decision > 0.1f)
        {
            // eat
            animator.SetBool("isEating", true);
        }
        else
        {
            // idle
        }
    }

    private void Attack()
    {
        animator.SetTrigger("Attack");
        attacking.SetAttackWaitTime(attackWaitTime / animator.speed * (animalStats.timeBetweenAttacks < 1f ? animalStats.timeBetweenAttacks : 1f)); // dont use Ienumerators, they suck
    }

    public void Die()
    {
        if (lastCell != null && lastCell.unitsInCell.Contains(cellIdentifier))
            lastCell.unitsInCell.Remove(cellIdentifier);


        // spawn effects
        GameObject deathEffectInstance = Instantiate(deathEffect, transform.position, Quaternion.identity);
        Vector3 dir = transform.position - health.GetLastHitFromPos;
        dir.y = 0f;
        deathEffectInstance.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        Destroy(deathEffectInstance, 5f);


        PlayerResourceManager.instance.AddResourcesWithUI(health.GetLastHitByPlayer.GetPlayerID, animalStats.cost, transform.position + new Vector3(0f, 1f, 0f));

        if (destroyGameobjectOnDeath)
        {
            Destroy(gameObject);
        }



        // if they have a health script
        if (health)
        {
            //if (health.GetLastHitByPlayer != null)
            //    PlayerResourceManager.PlayerResourceAmounts[health.GetLastHitByPlayer.GetPlayerID].AddResources(new ResourceAmount(resourceGiveAmt, 0, 0));

            // reset health back to normal
            health.ResetHealth();
        }

        justSpawned = false;
        // set the gameobject inactive
        gameObject.SetActive(false);
    }

    private void SetAnimations(AnimatorOverrideController newController)
    {
        animator.runtimeAnimatorController = newController;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(Application.isPlaying ? startPos : transform.position, roamRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPos : transform.position, maxLeaveRange);

        if (!navMovement)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(navMovement.GetDestination, 0.2f);

    }
}
