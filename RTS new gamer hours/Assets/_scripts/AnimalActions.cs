using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof (NavMeshAgent))]
public class AnimalActions : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private Vector2 timeBetweenDecisions = new Vector2(0.5f, 10f);
    [SerializeField] private float roamRange = 5f;
    [SerializeField] private float maxMoveSpeed = 1f;
    [SerializeField] private int resourceGiveAmt = 5;

    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject spawnEffect;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private AnimatorOverrideController animalOverrideController;
    [SerializeField] private float walkAnimSpeed = 1f;

    private Health health;
    private bool isScared = false;
    private NavMeshAgent agent;
    private float decisionCounter = 0f;
    private Vector3 startPos;
    private float debugMoveSpeedDisplay;
    private bool justSpawned = false;

    public void SetIsScared(bool isScared) => this.isScared = isScared;

    private void Awake()
    {
        SetAnimations(animalOverrideController);
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
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

        debugMoveSpeedDisplay = agent.velocity.magnitude / (maxMoveSpeed * 2f);

        decisionCounter -= Time.deltaTime;

        agent.speed = isScared ? maxMoveSpeed * 2f : maxMoveSpeed;

        animator.SetFloat("moveSpeed", agent.velocity.magnitude / (maxMoveSpeed * 2f));
        if (animator.GetFloat("moveSpeed") > 0f)
            animator.speed = walkAnimSpeed * agent.velocity.magnitude / agent.speed;
        else
            animator.speed = 1f;

        if (decisionCounter < 0f && agent.velocity.magnitude <= 0f)
        {
            MakeDecision();
        }

        if (health && health.GetCurrentHealth <= 0)
        {
            Die();
        }
    }

    private void MakeDecision ()
    {
        decisionCounter = Random.Range(timeBetweenDecisions.x, timeBetweenDecisions.y);

        // if we're still moving, just return
        if (agent.velocity.magnitude > 0.01f)
            return;

        // make decision
        float decision = Random.Range(0f, 1f);

        // reset bools
        animator.SetBool("isSitting", false);
        animator.SetBool("isEating", false);

        if (decision > 0.7f)
        {
            // walk around
            agent.SetDestination(startPos + new Vector3(Random.Range(0f, roamRange), 0f, Random.Range(0f, roamRange)));
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

    public void Die()
    {

        // spawn effects
        GameObject deathEffectInstance = Instantiate(deathEffect, transform.position, Quaternion.identity);
        Vector3 dir = transform.position - health.GetLastHitFromPos;
        dir.y = 0f;
        deathEffectInstance.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        Destroy(deathEffectInstance, 5f);


        // if they have a health script
        if (health)
        {
            if (health.GetLastHitByPlayer != -1)
                PlayerResourceManager.PlayerResourceAmounts[health.GetLastHitByPlayer].AddResources(new ResourceAmount(resourceGiveAmt, 0, 0));

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

        if (!agent)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(agent.destination, 0.2f);
    }
}
