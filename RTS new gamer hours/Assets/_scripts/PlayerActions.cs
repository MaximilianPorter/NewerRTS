using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof (Identifier))]
[RequireComponent(typeof (Attacking))]
[RequireComponent(typeof (NavMeshMovement))]
public class PlayerActions : MonoBehaviour
{
    [SerializeField] private UnitStats stats;
    [SerializeField] private AnimatorOverrideController overrideController;
    [SerializeField] private Animator animator;
    [SerializeField] private float walkAnimSpeed = 1f;
    [SerializeField] private float attackAnimSpeed = 1f;
    [SerializeField] private float attackFireWaitTime = 0.4f;

    [Space(10)]
    [SerializeField] private Renderer[] bodyPartsNeedMaterial;

    private Identifier identifier;
    private NavMeshMovement navMovement;
    private Attacking attacking;


    private Vector3 moveInput;
    private bool isAttacking = false;
    private bool isBlocking = false;


    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        navMovement = GetComponent<NavMeshMovement>();
        attacking = GetComponent<Attacking>();

        SetAnimations(overrideController);
    }

    private void Start()
    {
        // turn on correct body parts
        for (int i = 0; i < bodyPartsNeedMaterial.Length; i++)
        {
            bodyPartsNeedMaterial[i].material = PlayerColorManager.GetUnitMaterial(identifier.GetTeamID);
        }
    }

    private void Update()
    {
        moveInput = new Vector3(PlayerInput.GetPlayers[identifier.GetPlayerID].GetAxis(PlayerInput.GetInputMoveHorizontal),
                0f,
                PlayerInput.GetPlayers[identifier.GetPlayerID].GetAxis(PlayerInput.GetInputMoveVertical));

        navMovement.SetCanMove(!PlayerInput.GetPlayerIsInMenu(identifier.GetPlayerID) && !isAttacking && !isBlocking);

        if (moveInput.magnitude > 0.05f)
        {
            navMovement.MoveTowards(transform.position + moveInput);
            navMovement.SetLookAt(transform.position + moveInput);
        }
        else
            navMovement.ResetDestination();


        if (animator)
        {
            isAttacking = animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
            animator.ResetTrigger("Attack");

            if (!isAttacking)
            {
                if (attacking.GetCanAttack && PlayerInput.GetPlayers[identifier.GetPlayerID].GetButton(PlayerInput.GetInputAttack))
                {
                    animator.speed = attackAnimSpeed / (stats.timeBetweenAttacks < 1f ? stats.timeBetweenAttacks : 1f);
                    animator.SetTrigger("Attack");
                    attacking.SetAttackWaitTime(attackFireWaitTime / animator.speed * (stats.timeBetweenAttacks < 1f ? stats.timeBetweenAttacks : 1f));
                    return;
                }

                isBlocking = PlayerInput.GetPlayers[identifier.GetPlayerID].GetButton(PlayerInput.GetInputBlock);
                animator.SetBool("isBlocking", isBlocking);
                if (isBlocking)
                    return;

                animator.SetBool("isMoving", navMovement.GetMoveSpeed01 > 0.01f);
                if (animator.GetBool("isMoving"))
                    animator.speed = walkAnimSpeed * navMovement.GetCurrentMoveSpeed;

                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    animator.speed = 1f;

            }
        }

        //if (attacking.GetCanAttack && PlayerInput.players[identifier.GetPlayerID].GetButton(PlayerInput.GetInputAttack) && !shieldAnimator.GetBool("isBlocking"))
        //{
        //    swordAnimator.SetTrigger("Attack");
        //    PlayerInput.VibrateController(identifier.GetPlayerID, .5f, .2f);
        //    StartCoroutine(attacking.Attack(0f));
        //}

        //bool isBlocking = PlayerInput.players[identifier.GetPlayerID].GetButton(PlayerInput.GetInputBlock);

        //swordAnimator.SetBool("isBlocking", isBlocking);
        //shieldAnimator.SetBool("isBlocking", isBlocking);

        //if (isBlocking)
        //    movement.SetSlowMultiplier(0, stats.slowMultiplierBlocking);
        //else
        //    movement.SetSlowMultiplier(0, 1f);


        //rightHandTarget.position = swordHolder.position;
        //rightHandTarget.rotation = swordHolder.rotation;

        //leftHandTarget.position = shieldHolder.position;
        //leftHandTarget.rotation = shieldHolder.rotation;
    }

    private void SetAnimations(AnimatorOverrideController newController)
    {
        animator.runtimeAnimatorController = newController;
    }

}
