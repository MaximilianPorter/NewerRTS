using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Attacking : MonoBehaviour
{
    [SerializeField] private Animator swordAnimator;
    [SerializeField] private Transform swordHolder;
    [SerializeField] private Transform rightHandTarget;

    private Identifier playerIdentifier;

    [Header("Stats")]
    [SerializeField] private UnitStats stats;
    [SerializeField] private bool isRanged = true;
    [SerializeField] private float range = 5f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float projectileForce;
    [SerializeField] private GameObject projectile;

    [Space(10)]

    [SerializeField] private bool debugShoot = false;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask enemyMask;


    private Movement movement;
    private Transform target;
    private Transform nearestEnemy;
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
            projectileForce = stats.projectileForce;
            projectile = stats.projectile;
        }
    }

    private void Update()
    {
        if (playerIdentifier.GetIsPlayer)
        {
            if (PlayerInput.players[playerIdentifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputAttack))
            {
                swordAnimator.SetTrigger("Attack");
                PlayerInput.VibrateController(playerIdentifier.GetPlayerID, .5f, .2f);
            }

        }
        rightHandTarget.position = swordHolder.position;

        if (debugShoot)
        {
            Shoot();
            debugShoot = false;
        }

        if (!playerIdentifier.GetIsPlayer)
        {
            NonPlayerBehaviour();
        }
    }

    private void FixedUpdate()
    {
        if (!playerIdentifier.GetIsPlayer)
        {
            FindNearestEnemy();
        }
    }

    private void NonPlayerBehaviour ()
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
        }
        else
        {
            movement.SetLookAt(null);

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
