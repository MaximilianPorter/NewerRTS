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

    private Identifier playerIdentifier;

    [Header("Stats")]
    [SerializeField] private UnitStats stats;
    [SerializeField] private bool isRanged = true;
    [SerializeField] private float range = 5f;
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
    public Transform GetNearestEnemy => nearestEnemy;

    private void Awake()
    {
        playerIdentifier = GetComponent<Identifier>();
        movement = GetComponent<Movement>();

        if (stats)
        {
            isRanged = stats.isRanged;
            range = stats.range;
            damage = stats.damage;
            timeBetweenAttacks = stats.timeBetweenAttacks;
            projectileForce = stats.projectileForce;
            projectile = stats.projectile;
            slowMultiplierBlocking = stats.slowMultiplierBlocking;
        }
    }

    private void Update()
    {
        if (playerIdentifier.GetIsPlayer)
        {
            if (canAttack && PlayerInput.players[playerIdentifier.GetPlayerID].GetButton(PlayerInput.GetInputAttack) && !shieldAnimator.GetBool("isBlocking"))
            {
                swordAnimator.SetTrigger("Attack");
                PlayerInput.VibrateController(playerIdentifier.GetPlayerID, .5f, .2f);
                Attack();
            }

            bool isBlocking = PlayerInput.players[playerIdentifier.GetPlayerID].GetButton(PlayerInput.GetInputBlock);
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

        if (!playerIdentifier.GetIsPlayer)
        {
            NonPlayerBehaviour();
        }

        HandleAttackingTime();
    }

    private void FixedUpdate()
    {
        if (!playerIdentifier.GetIsPlayer)
        {
            FindNearestEnemy();
        }
    }

    private void NonPlayerBehaviour()
    {
        if (nearestEnemy != null)
        {
            movement.SetLookAt(nearestEnemy);

            // if the place we're going to is already within (range / 2) of the target, stop moving and attack the target
            if ((movement.GetMoveTarget - nearestEnemy.position).sqrMagnitude < (range * range) / 2f)
            {
                movement.SetMoveTarget(transform.position);
            }

            // melee chase after the enemy if they've reached they're pos
            if (!isRanged && !movement.GetCanMove)
            {
                movement.SetMoveTarget(nearestEnemy.position);
            }

            // if you're able to attack and there's an enemy
            if (canAttack)
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
        canAttack = attackCounter > timeBetweenAttacks;
    }
    private void Attack ()
    {
        attackCounter = 0f;
        
        if (isRanged)
        {
            Shoot();
        }
    }

    private void FindNearestEnemy ()
    {
        Collider[] nearbyUnits = Physics.OverlapSphere(transform.position, range, enemyMask).Where(unit => unit.GetComponent<Identifier>().GetTeamID != playerIdentifier.GetTeamID).ToArray();

        if (nearbyUnits.Length <= 0)
        {
            nearestEnemy = null;
            return;
        }

        //Collider[] enemyUnits = nearbyUnits.Where(unit => unit.GetComponent<PlayerIdentifier>().GetTeamID != playerIdentifier.GetTeamID).ToArray();
        foreach (Collider enemyCol in nearbyUnits)
        {
            if (nearestEnemy == null || (enemyCol.transform.position - transform.position).sqrMagnitude <
                (nearestEnemy.position - transform.position).sqrMagnitude * (nearestEnemy.position - transform.position).sqrMagnitude)
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
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
