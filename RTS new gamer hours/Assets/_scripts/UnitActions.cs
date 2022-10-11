using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Attacking))]
[RequireComponent(typeof(Health))]
public class UnitActions : MonoBehaviour
{
    [SerializeField] private GameObject selectedGO;
    [SerializeField] private GameObject orderingObject;
    [SerializeField] private UnitStats unitStats;
    [SerializeField] private bool debugDie = false;
    [SerializeField] private GameObject bloodSplatterImage;
    [SerializeField] private GameObject bloodGoreEffect;

    [Header("Animated")]
    [SerializeField] private Renderer[] bodyPartsNeedMaterial;

    [Header("Animations")]
    [SerializeField] private AnimatorOverrideController unitOverrideController;
    [SerializeField] private Animator animator;
    [SerializeField] private float walkAnimSpeed = 1f;
    [SerializeField] private float attackAnimSpeed = 1f;
    [SerializeField] private float attackFireWaitTime = 0.4f;

    private Identifier identifier;
    private Movement movement;
    private Attacking attacking;
    private Health health;
    private bool isSelected = false;
    private bool isAttacking = false;

    public UnitStats GetStats => unitStats;
    public Movement GetMovement => movement;
    public Attacking GetAttacking() => attacking;
    public Identifier GetIdentifier() => identifier;
    public GameObject GetOrderingObject => orderingObject;
    public bool GetIsSelected => isSelected;
    public void SetIsSelected(bool select) => isSelected = select;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        movement = GetComponent<Movement>();
        attacking = GetComponent<Attacking>();
        health = GetComponent<Health>();

        health.SetValues(unitStats.health);
        orderingObject.SetActive(false);


        SetAnimations(unitOverrideController);
    }

    private void Start()
    {
        // turn on correct body parts
        for (int i = 0; i < bodyPartsNeedMaterial.Length; i++)
        {
            bodyPartsNeedMaterial[i].material = PlayerColorManager.GetUnitMaterial(identifier.GetTeamID);
        }


        // add unit to list of all units for player
        PlayerHolder.AddUnit(identifier.GetPlayerID, this);
    }

    private void Update()
    {
        selectedGO.SetActive(isSelected);

        if (debugDie || health.GetIsDead)
            Die();

        if (animator)
        {
            isAttacking = animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
            animator.ResetTrigger("Attack");

            if (!isAttacking)
            {
                if (attacking.GetHasLoadedAttack)
                {
                    animator.speed = attackAnimSpeed;
                    animator.SetTrigger("Attack");
                    attacking.SetAttackWaitTime(attackFireWaitTime / animator.speed);
                    return;
                }


                animator.SetBool("isMoving", movement.GetMoveSpeed01 > 0.1f);
                if (animator.GetBool("isMoving"))
                    animator.speed = walkAnimSpeed;



                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    animator.speed = 1f;
            }
        }
    }

    public void Die ()
    {
        PlayerHolder.RemoveUnit(identifier.GetPlayerID, this);

        //GameObject bloodInstance = Instantiate(bloodSplatterImage, new Vector3(transform.position.x, 0.01f, transform.position.z),
        //    Quaternion.LookRotation(Vector3.down, Vector3.Lerp(Vector3.forward, Vector3.right, Random.Range(0f, 1f))));

        GameObject goreInstance = Instantiate(bloodGoreEffect, transform.position, Quaternion.identity);
        Destroy(goreInstance, 5f);

        Destroy(gameObject);
    }

    private void SetAnimations (AnimatorOverrideController newController)
    {
        animator.runtimeAnimatorController = newController;
    }


}
