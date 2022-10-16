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

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private AnimatorOverrideController animalOverrideController;
    [SerializeField] private float walkAnimSpeed = 1f;

    private bool isScared = false;
    private NavMeshAgent agent;
    private float decisionCounter = 0f;
    private Vector3 startPos;
    private float debugMoveSpeedDisplay;

    public void SetIsScared(bool isScared) => this.isScared = isScared;

    private void Awake()
    {
        SetAnimations(animalOverrideController);
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
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

    private void SetAnimations(AnimatorOverrideController newController)
    {
        animator.runtimeAnimatorController = newController;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, roamRange);

        if (!agent)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(agent.destination, 0.2f);
    }
}
