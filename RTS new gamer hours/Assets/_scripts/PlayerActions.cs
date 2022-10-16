using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof (Identifier))]
[RequireComponent(typeof (Attacking))]
[RequireComponent(typeof (Movement))]
public class PlayerActions : MonoBehaviour
{
    [SerializeField] private UnitStats stats;
    [SerializeField] private AnimatorOverrideController overrideController;
    [SerializeField] private Animator animator;
    [SerializeField] private float walkAnimSpeed = 1f;
    [SerializeField] private float attackAnimSpeed = 1f;
    [SerializeField] private float attackFireWaitTime = 0.4f;

    private Identifier identifier;
    private Movement movement;
    private Attacking attacking;


    private Vector3 moveInput;
    private bool isAttacking = false;
    private bool isBlocking = false;


    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        movement = GetComponent<Movement>();
        attacking = GetComponent<Attacking>();

        SetAnimations(overrideController);
    }

    private void Update()
    {
        moveInput = new Vector3(PlayerInput.GetPlayers[identifier.GetPlayerID].GetAxis(PlayerInput.GetInputMoveHorizontal),
                0f,
                PlayerInput.GetPlayers[identifier.GetPlayerID].GetAxis(PlayerInput.GetInputMoveVertical));
        movement.SetMoveTarget(transform.position + moveInput);

        movement.SetCanMove(!PlayerInput.GetPlayerIsInMenu(identifier.GetPlayerID) && !isAttacking && !isBlocking);

        movement.SetInputJumpDown(PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputJump));


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

                animator.SetBool("isMoving", movement.GetMoveSpeed01 > 0.01f);
                if (animator.GetBool("isMoving"))
                    animator.speed = walkAnimSpeed * movement.GetMaxMoveSpeed;

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
