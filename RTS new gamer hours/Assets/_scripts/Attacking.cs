using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Attacking : MonoBehaviour
{
    [SerializeField] private Animator swordAnimator;
    [SerializeField] private Transform swordHolder;
    [SerializeField] private Transform rightHandTarget;

    [SerializeField] private Animator shieldAnimator;
    [SerializeField] private Transform shieldHolder;
    [SerializeField] private Transform leftHandTarget;

    private Identifier identifier;

    [Header("Stats")]
    [SerializeField] private UnitStats stats;
    [SerializeField] private bool isRanged = true;
    [SerializeField] private float lookRange = 5f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float timeBetweenAttacks = 0.5f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float projectileForce;
    [SerializeField] private GameObject projectile;
    [SerializeField] private float slowMultiplierBlocking = 0.2f;

    [Space(10)]

    [SerializeField] private bool debugShoot = false;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask enemyMask;


    private Movement movement;
    private Transform target;
    private Transform nearestEnemy;
    private bool canAttack = true;
    private float attackCounter = 10000f;

    private float checkForEnemyCounter = 0;
    private const float checkForEnemyTime = 2f;


    public void SetNearestEnemy (Transform newEnemy) => nearestEnemy = newEnemy;
    public Transform GetNearestEnemy => nearestEnemy;
    public UnitStats GetStats => stats;
    public float GetLookRange => lookRange;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        movement = GetComponent<Movement>();

        if (stats)
        {
            isRanged = stats.isRanged;
            lookRange = stats.lookRange;
            attackRange = stats.attackRange;
            damage = stats.damage;
            timeBetweenAttacks = stats.timeBetweenAttacks;
            projectileForce = stats.projectileForce;
            projectile = stats.projectile;
            slowMultiplierBlocking = stats.slowMultiplierBlocking;
        }
    }

    private void Update()
    {
        if (identifier.GetIsPlayer)
        {
            if (canAttack && PlayerInput.players[identifier.GetPlayerID].GetButton(PlayerInput.GetInputAttack) && !shieldAnimator.GetBool("isBlocking"))
            {
                swordAnimator.SetTrigger("Attack");
                PlayerInput.VibrateController(identifier.GetPlayerID, .5f, .2f);
                Attack();
            }

            bool isBlocking = PlayerInput.players[identifier.GetPlayerID].GetButton(PlayerInput.GetInputBlock);
            swordAnimator.SetBool ("isBlocking", isBlocking);
            shieldAnimator.SetBool("isBlocking", isBlocking);
            if (isBlocking)
                movement.SetSlowMultiplier(0, slowMultiplierBlocking);
            else
                movement.SetSlowMultiplier(0, 1f);
        }



        rightHandTarget.position = swordHolder.position;


        if (shieldHolder)
            leftHandTarget.position = shieldHolder.position;

        if (debugShoot)
        {
            Shoot();
            debugShoot = false;
        }

        if (!identifier.GetIsPlayer)
        {
            NonPlayerBehaviour();
        }

        HandleAttackingTime();
    }

    private void FixedUpdate()
    {
        if (!identifier.GetIsPlayer)
        {
            checkForEnemyCounter += Time.fixedDeltaTime;
            if (checkForEnemyCounter > checkForEnemyTime || nearestEnemy == null)
            {
                FindNearestEnemy();
                checkForEnemyCounter = 0f;
            }
        }
    }

    private void NonPlayerBehaviour()
    {
        if (nearestEnemy != null)
        {
            movement.SetLookAt(nearestEnemy);

            // if the place we're going to is already within (range / 2) of the target, stop moving and attack the target
            if ((movement.GetMoveTarget - nearestEnemy.position).sqrMagnitude < (lookRange * lookRange) / 2f)
            {
                movement.SetMoveTarget(transform.position);
            }

            // melee chase after the enemy if they've reached they're pos
            if (!isRanged && !movement.GetCanMove && 
                (transform.position - nearestEnemy.position).sqrMagnitude > attackRange * attackRange) // don't worry about pos if you're already in attacking range
            {
                // pos = enemy + attackrange / 4
                // not exactly on the enemy position, but close enough to attack
                movement.SetMoveTarget(nearestEnemy.position + (transform.position - nearestEnemy.position).normalized * (attackRange * 0.75f));
            }

            // if canAttack and there's an enemy IN RANGE
            if (canAttack && (transform.position - nearestEnemy.position).sqrMagnitude < attackRange * attackRange)
            {
                Attack();
            }
        }
        else
        {
            movement.SetLookAt(null);

        }
    }

    private void HandleAttackingTime ()
    {
        attackCounter += Time.deltaTime;
        canAttack = attackCounter > timeBetweenAttacks && movement.GetMoveSpeed01 < 0.01f; // can attack if your timer is ready and you're not moving
    }
    private void Attack ()
    {
        attackCounter = 0f;
        FindNearestEnemy();
        
        if (isRanged)
        {
            Shoot();
        }
        else
        {
            swordAnimator.SetTrigger("Attack");
        }
    }

    private void FindNearestEnemy ()
    {
        Collider[] nearbyUnits = Physics.OverlapSphere(transform.position, lookRange, enemyMask).Where(unit => unit.GetComponent<Identifier>().GetTeamID != identifier.GetTeamID).ToArray();

        if (nearbyUnits.Length <= 0)
        {
            nearestEnemy = null;
            return;
        }

        foreach (Collider enemyCol in nearbyUnits)
        {
            if (nearestEnemy == null || (enemyCol.transform.position - transform.position).sqrMagnitude <
                (nearestEnemy.position - transform.position).sqrMagnitude)
            {
                nearestEnemy = enemyCol.transform;
            }
        }
    }

    private void Shoot ()
    {
        Projectile projInstance = Instantiate(projectile, firePoint.position, Quaternion.identity).GetComponent<Projectile>();
        projInstance.Launch(target.position, damage, projectileForce);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
