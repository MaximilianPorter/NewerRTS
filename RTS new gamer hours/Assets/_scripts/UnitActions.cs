using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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

    [Header("Throwing")]
    [SerializeField] private GameObject torch;
    [SerializeField] private float torchDamage = 10f;
    [SerializeField] private float torchThrowForce = 10f;
    [SerializeField] private float timeBetweenThrows = 2f;
    [SerializeField] private float throwAnimSpeed = 1.5f;
    [SerializeField] private float throwFireWaitTime = 0.6f;

    private Identifier identifier;
    private Movement movement;
    private Attacking attacking;
    private Health health;
    private bool isSelected = false;
    private bool isAttacking = false;
    private bool isThrowing = false;
    private float throwCounter = 0f;
    private float throwWaitTimer = 0f;
    private Vector3 tempThrowTarget;
    private bool hasThrown = true;

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
            isThrowing = animator.GetCurrentAnimatorStateInfo(0).IsName("Throw");
            movement.SetCanMove(!isAttacking && !isThrowing);
            movement.SetCanTurn(!isThrowing);

            if (isThrowing)
                animator.speed = throwAnimSpeed;
            else if (isAttacking)
                animator.speed = attackAnimSpeed;

            if (!isAttacking && !isThrowing)
            {
                animator.SetBool("isMoving", movement.GetMoveSpeed01 > 0.1f);
                if (animator.GetBool("isMoving"))
                    animator.speed = walkAnimSpeed;

                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    animator.speed = 1f;
            }
        }

        throwCounter += Time.deltaTime;
        throwWaitTimer -= Time.deltaTime;
        if (throwWaitTimer < 0 && !hasThrown)
            Throw();

        HandleMoveTowardsEnemies();
    }

    private void HandleMoveTowardsEnemies ()
    {
        Vector3 moveTowardsPos = movement.GetMoveTarget;
        Transform enemy = attacking.GetNearestEnemy;

        if (enemy)
        {
            // if we click and there's no enemy close to that pos, then we full commit to that pos and then worry later
            if ((moveTowardsPos - enemy.position).sqrMagnitude > (unitStats.lookRange * unitStats.lookRange) / 2f && moveTowardsPos != transform.position)
            {
                movement.SetLookAt(null);
                return;
            }

            // if we click and there's an enemy close to that clickPos (range / 2f) then we go to that enemy
            // or we're standing still and the enemy is in our range
            else
            {
                float sqrDistFromEnemy = (transform.position - enemy.position).sqrMagnitude;
                if (enemy.TryGetComponent (out Building enemyBuilding))
                {
                    if (sqrDistFromEnemy > enemyBuilding.GetStats.interactionRadius * enemyBuilding.GetStats.interactionRadius)
                    {
                        // if we're out of range to throw shit on the building
                        movement.SetMoveTarget (enemy.position);
                    }
                    else
                    {
                        // if we're in range to lob that stinky poo poo onto the building
                        movement.SetMoveTarget(transform.position);
                        movement.SetLookAt(enemy);

                        if (throwCounter > timeBetweenThrows)
                            StartThrow(enemy.position);
                    }
                }
                else
                {
                    if (sqrDistFromEnemy > unitStats.attackRange * unitStats.attackRange)
                    {
                        // if we're out of range of attacking
                        // move close enough to attack
                        movement.SetMoveTarget(enemy.position);
                    }
                    else
                    {
                        // we're in range of attacking
                        movement.SetMoveTarget(transform.position);
                        movement.SetLookAt(enemy);

                        if (attacking.GetCanAttack && !isAttacking)
                        {
                            Attack();
                        }
                    }
                }
            }
        }
        else
        {
            movement.SetLookAt(null);
        }
    }

    private void Attack ()
    {
        animator.SetTrigger("Attack");
        attacking.SetAttackWaitTime(attackFireWaitTime / animator.speed); // dont use Ienumerators, they suck
    }

    private void StartThrow (Vector3 target)
    {
        throwCounter = 0f;
        hasThrown = false;
        animator.SetTrigger("Throw");
        throwWaitTimer = throwFireWaitTime;

        tempThrowTarget = target;
    }

    private void Throw ()
    {
        hasThrown = true;
        Projectile torchInstance = Instantiate(torch, transform.position, Quaternion.identity).GetComponent<Projectile>();
        torchInstance.SetInfo(torchDamage, identifier.GetTeamID);

        Projectile.SetTrajectory(torchInstance.GetRigidbody, tempThrowTarget, torchThrowForce, 0, 1f);
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
