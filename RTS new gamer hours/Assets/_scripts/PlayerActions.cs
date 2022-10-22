using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof (Identifier))]
[RequireComponent(typeof (Attacking))]
[RequireComponent(typeof (NavMeshMovement))]
public class PlayerActions : MonoBehaviour
{
    [SerializeField] private UnitStats stats;

    [Header("Sprint")]
    [SerializeField] private Image sprintFill;
    [SerializeField] private float sprintTimeMulti = 2f;
    [SerializeField] private float timeToRegenSprint = 3f;
    [SerializeField] private float refreshSprintWaitTime = 1f;

    private float sprintCounter = 0f;
    private float refreshSprintCounter = 0f;

    [Header("Animated")]
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
    private UnitSelection unitSelection;

    private Image[] sprintRenderers;
    private Color[] startSprintRendColors;
    private float sprintFullColorCounter = 0f;


    private Vector3 moveInput;
    private bool isAttacking = false;
    private bool isBlocking = false;
    private bool isSprinting = false;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        navMovement = GetComponent<NavMeshMovement>();
        attacking = GetComponent<Attacking>();
        unitSelection = GetComponent<UnitSelection>();

        SetAnimations(overrideController);
    }

    private void Start()
    {
        // turn on correct body parts
        for (int i = 0; i < bodyPartsNeedMaterial.Length; i++)
        {
            bodyPartsNeedMaterial[i].material = PlayerColorManager.GetUnitMaterial(identifier.GetTeamID);
        }

        sprintCounter = timeToRegenSprint;
        sprintRenderers = new Image[2] { sprintFill, sprintFill.transform.parent.GetComponent<Image>() };
        startSprintRendColors = new Color[2] { sprintFill.color, sprintFill.transform.parent.GetComponent<Image>().color };
    }

    private void Update()
    {
        moveInput = new Vector3(PlayerInput.GetPlayers[identifier.GetPlayerID].GetAxis(PlayerInput.GetInputMoveHorizontal),
                0f,
                PlayerInput.GetPlayers[identifier.GetPlayerID].GetAxis(PlayerInput.GetInputMoveVertical));

        // if there's a game winner, don't update
        if (GameWinManager.instance != null)
            if (GameWinManager.instance.GetWinnerID() != -1)
            {
                moveInput = Vector3.zero;
            }

        HandleSprint();


        navMovement.SetMoveSpeed(Mathf.Clamp01 (moveInput.magnitude) * stats.maxMoveSpeed * (isSprinting ? 2f : 1f));
        navMovement.SetCanMove(!PlayerInput.GetPlayerIsInMenu(identifier.GetPlayerID) && !isAttacking && !isBlocking);

        if (moveInput.magnitude > 0.1f)
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
    }

    private void HandleSprint ()
    {
        // Fade in and out when full or when using
        bool sprintFull = sprintCounter >= timeToRegenSprint;
        if (sprintFull)
        {
            sprintFullColorCounter = Mathf.Clamp(sprintFullColorCounter - Time.deltaTime * 3f, 0f, 1f);
        }
        for (int i = 0; i < sprintRenderers.Length; i++)
        {
            Color normal = sprintRenderers[i].color;
            sprintRenderers[i].color = new Color(normal.r, normal.g, normal.b, startSprintRendColors[i].a * (sprintFullColorCounter / 1f));
        }

        // adjust fill meter to visually represent sprint
        sprintFill.fillAmount = sprintCounter / timeToRegenSprint;
        sprintFill.transform.parent.localPosition = PlayerHolder.WorldToCanvasLocalPoint(transform.position + new Vector3(0f, -5f, 0f), identifier.GetPlayerID);

        if (isSprinting)
        {
            sprintFullColorCounter = Mathf.Clamp(sprintFullColorCounter + Time.deltaTime * 3f, 0f, 1f);
            sprintCounter = Mathf.Clamp(sprintCounter - Time.deltaTime * sprintTimeMulti, 0f, timeToRegenSprint);
            refreshSprintCounter = refreshSprintWaitTime;
        }
        else 
        {
            refreshSprintCounter -= Time.deltaTime;
        }

        // we waited long enough, we can recharge our sprint
        if (refreshSprintCounter <= 0)
        {
            sprintCounter = Mathf.Clamp(sprintCounter + Time.deltaTime, 0f, timeToRegenSprint);
        }

        if (unitSelection.GetHasTroopsSelected)
            return;

        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonShortPress(PlayerInput.GetInputSprint) && sprintCounter > 0 && navMovement.GetMoveSpeed01 > 0.1f)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    private void SetAnimations(AnimatorOverrideController newController)
    {
        animator.runtimeAnimatorController = newController;
    }

}
