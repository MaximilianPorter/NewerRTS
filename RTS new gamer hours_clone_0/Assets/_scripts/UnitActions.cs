using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[RequireComponent(typeof(NavMeshMovement))]
[RequireComponent(typeof(Attacking))]
[RequireComponent(typeof(Health))]
public class UnitActions : MonoBehaviour
{
    [SerializeField] private bool isSelectable = true;
    [SerializeField] private bool isTargetable = true;
    [SerializeField] private GameObject selectedGO;
    [SerializeField] private LineRenderer lineToDestinationVisual;
    [SerializeField] private GameObject movementTargetVisual;
    [SerializeField] private UnitStats unitStats;
    [SerializeField] private bool debugDie = false;
    [SerializeField] private GameObject bloodSplatterImage;
    [SerializeField] private GameObject bloodGoreEffect;


    [Header("Visuals")]
    [SerializeField] private Renderer[] bodyPartsNeedMaterial;

    private GameObject healthBarInstance;
    private Outline outline;

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
    private CellIdentifier cellIdentifier;
    private NavMeshMovement navMovement;
    private Attacking attacking;
    private Health health;
    private bool isSelected = false;
    private bool isAttacking = false;
    private bool isThrowing = false;
    private float throwCounter = 0f;
    private float throwWaitTimer = 0f;
    private Vector3 tempThrowTarget;
    private bool hasThrown = true;
    private UnitMovementType movementType = UnitMovementType.LookNearDestination;

    private Cell lastCell;
    //private Cell activeCell;
    

    public void SetIsTargetable(bool isTargetable) => this.isTargetable = isTargetable;
    public bool GetIsTargetable => isTargetable;
    public void SetIsSelectable(bool isSelectable) => this.isSelectable = isSelectable;
    public bool GetIsSelectable => isSelectable;
    public void SetGroupMoveSpeed (float moveSpeed) => navMovement.SetMoveSpeed (moveSpeed);
    public UnitStats GetStats => unitStats;
    public NavMeshMovement GetMovement => navMovement;
    public Attacking GetAttacking() => attacking;
    public Identifier GetIdentifier() => identifier;
    public Health GetHealth => health;
    public GameObject GetOrderingObject => movementTargetVisual;
    public bool GetIsSelected => isSelected;
    public void SetIsSelected(bool select) => isSelected = select;
    public void SetDestinationWithType (Vector3 destination, UnitMovementType movementType)
    {
        navMovement.SetDestination(destination);
        this.movementType = movementType;
    }

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        cellIdentifier = GetComponent<CellIdentifier>();
        navMovement = GetComponent<NavMeshMovement>();
        attacking = GetComponent<Attacking>();
        health = GetComponent<Health>();

        health.SetValues(unitStats.health, unitStats.armor);


        SetAnimations(unitOverrideController);
    }

    private void Start()
    {
        outline = GetComponent<Outline>();


        UpdateBodyColor();


        // add unit to list of all units for player
        if (isSelectable)
        {
            PlayerHolder.AddUnit(identifier.GetPlayerID, this);
        }

        movementTargetVisual.SetActive(false);
        SetUnitSpecificPlayerLayers(identifier.GetPlayerID);

    }

    private void Update()
    {
        if (outline)
        {
            outline.OutlineColor = PlayerColorManager.GetPlayerColorIgnoreAlpha(identifier.GetPlayerID, outline.OutlineColor.a);
        }
        selectedGO.SetActive(isSelected);
        attacking.SetCanAttack(!navMovement.enabled || navMovement.GetMoveSpeed01 < 0.01f);

        // display line to destination
        if (navMovement.GetDestination != transform.position && isSelected)
        {
            lineToDestinationVisual.gameObject.SetActive(true);
            lineToDestinationVisual.positionCount = navMovement.GetAgentWaypoints.Length;
            lineToDestinationVisual.SetPositions(navMovement.GetAgentWaypoints);
        }
        else
        {
            lineToDestinationVisual.gameObject.SetActive(false);
        }

        
        

        if (debugDie || health.GetIsDead)
            Die();

        if (animator)
        {
            isAttacking = animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
            isThrowing = animator.GetCurrentAnimatorStateInfo(0).IsName("Throw");
            if (navMovement != null)
            {
                navMovement.SetCanMove(!isAttacking && !isThrowing);
                navMovement.SetCanTurn(!isThrowing);
            }

            if (isThrowing)
                animator.speed = throwAnimSpeed;
            else if (isAttacking)
                animator.speed = attackAnimSpeed / (unitStats.timeBetweenAttacks < 1 ? unitStats.timeBetweenAttacks : 1f);

            if (!isAttacking && !isThrowing)
            {
                if (navMovement != null)
                    animator.SetBool("isMoving", navMovement.GetMoveSpeed01 > 0.1f);
                if (animator.GetBool("isMoving"))
                    animator.speed = walkAnimSpeed * navMovement.GetCurrentMoveSpeed;

                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    animator.speed = 1f;
            }
        }

        throwCounter += Time.deltaTime;
        throwWaitTimer -= Time.deltaTime;
        if (throwWaitTimer < 0 && !hasThrown)
            Throw();

        HandleMoveTowardsEnemies();

        UnitCellManager.UpdateActiveCell(cellIdentifier, transform.position, ref lastCell);

    }


    private void HandleMoveTowardsEnemies ()
    {
        if (navMovement == null)
            return;

        // if we're at our destination, go patrol mode
        if (!navMovement.GetIsMoving && Vector3.Distance (navMovement.GetDestination, transform.position) < 0.5f)
            movementType = UnitMovementType.Patrol;


        if (movementType == UnitMovementType.IgnoreEnemies)
        {
            navMovement.SetLookAt(null);
            return;
        }

        Vector3 moveTowardsPos = navMovement.GetDestination;
        CellIdentifier enemy = attacking.GetNearestEnemy;

        if (enemy)
        {
            Vector3 dirMoveTargetAndEnemy = (moveTowardsPos - enemy.transform.position);
            dirMoveTargetAndEnemy.y = 0f;
            Vector3 dirMoveTargetAndMe = (moveTowardsPos - transform.position);
            dirMoveTargetAndMe.y = 0f;


            //// (MOVING) if the destination is far enough away from the enemy, leave the enemy alone
            //if (navMovement.GetIsMoving && dirMoveTargetAndEnemy.sqrMagnitude > (unitStats.lookRange/2f * unitStats.lookRange/2f) && movementType != UnitMovementType.Patrol)
            //{
            //    navMovement.SetLookAt(null);
            //    return;
            //}
            //// (NOT MOVING) if we click far enough away from our own range, leave the enemy
            if (dirMoveTargetAndMe.sqrMagnitude > (unitStats.lookRange/2f * unitStats.lookRange/2f) && movementType != UnitMovementType.Patrol)
            {
                navMovement.SetLookAt(null);
                return;
            }

            // if we click and there's an enemy close to that clickPos (range / 2f) then we go to that enemy
            // or we're standing still and the enemy is in our range
            else
            {
                Vector3 dirEnemyAndMe = (transform.position - enemy.transform.position);
                dirEnemyAndMe.y = 0f;

                float sqrDistFromEnemy = dirEnemyAndMe.sqrMagnitude;
                if (enemy.TryGetComponent (out Building enemyBuilding))
                {
                    float minDist = unitStats.regularAttackBuildings ? attacking.AttackRangeWithHeight * attacking.AttackRangeWithHeight : enemyBuilding.GetStats.interactionRadius * enemyBuilding.GetStats.interactionRadius;
                    if (sqrDistFromEnemy > minDist)
                    {
                        // if we're out of range to throw shit on the building
                        navMovement.SetDestination (enemy.transform.position + dirEnemyAndMe.normalized * (enemyBuilding.GetStats.interactionRadius - 0.1f));
                    }
                    else
                    {
                        // if we're in range to lob that stinky poo poo onto the building
                        navMovement.ResetDestination();
                        navMovement.SetLookAt(enemy.transform);

                        if (unitStats.regularAttackBuildings)
                        {
                            if (attacking.GetCanAttack && !isAttacking && !isThrowing)
                            {
                                Attack();
                            }
                        }
                        else
                        {
                            if (throwCounter > timeBetweenThrows)
                                StartThrow(enemy.transform.position);
                        }
                    }
                }
                else
                {
                    float colliderSqrRadius = ((CapsuleCollider)enemy.GetCollider).radius * ((CapsuleCollider)enemy.GetCollider).radius;
                    if (sqrDistFromEnemy > attacking.AttackRangeWithHeight * attacking.AttackRangeWithHeight + colliderSqrRadius) // - enemy.collider.radius
                    {
                        // if we're out of range of attacking
                        // move close enough to attack
                        navMovement.SetDestination(enemy.transform.position);
                        navMovement.SetLookAt(null);
                    }
                    else
                    {
                        // we're in range of attacking
                        navMovement.ResetDestination();
                        navMovement.SetLookAt(enemy.transform);

                        if (attacking.GetCanAttack && !isAttacking && !isThrowing)
                        {
                            Attack();
                        }
                    }
                }
            }
        }
        else
        {
            navMovement.SetLookAt(null);
        }
    }

    private void Attack ()
    {
        animator.SetTrigger("Attack");
        attacking.SetAttackWaitTime(attackFireWaitTime / animator.speed * (unitStats.timeBetweenAttacks < 1f ? unitStats.timeBetweenAttacks : 1f)); // dont use Ienumerators, they suck
    }
    private void StartThrow (Vector3 target)
    {
        throwCounter = 0f;
        hasThrown = false;
        animator.SetTrigger("Throw");
        throwWaitTimer = throwFireWaitTime / animator.speed;

        tempThrowTarget = target;
    }
    private void Throw ()
    {
        hasThrown = true;
        Projectile torchInstance = Instantiate(torch, transform.position, Quaternion.identity).GetComponent<Projectile>();
        float hotterFireResearch = PlayerHolder.GetCompletedResearch(identifier.GetPlayerID).Contains(BuyIcons.Research_HotterFire) ? 2f : 1f;
        torchInstance.SetInfo(torchDamage * hotterFireResearch, identifier.GetPlayerID, identifier.GetTeamID, true);

        Projectile.SetTrajectory(torchInstance.GetRigidbody, tempThrowTarget, torchThrowForce, 100, 1f);
    }
    public void Die ()
    {
        RemoveUnitFromLists();

        //GameObject bloodInstance = Instantiate(bloodSplatterImage, new Vector3(transform.position.x, 0.01f, transform.position.z),
        //    Quaternion.LookRotation(Vector3.down, Vector3.Lerp(Vector3.forward, Vector3.right, Random.Range(0f, 1f))));

        GameObject goreInstance = Instantiate(bloodGoreEffect, transform.position, Quaternion.identity);
        Destroy(goreInstance, 5f);

        Destroy(healthBarInstance);
        Destroy(gameObject);
    }

    public void SwitchTeams (int newPlayerID, int newTeamID)
    {
        RemoveUnitFromLists();

        Identifier newUnitInstance = Instantiate(this.gameObject, transform.position, transform.rotation).GetComponent<Identifier>();
        newUnitInstance.UpdateInfo(newPlayerID, newTeamID);

        if (newUnitInstance.TryGetComponent(out UnitActions newUnitActions))
        {
            newUnitActions.navMovement.SetDestination(navMovement.GetDestination);
            newUnitActions.SetUnitSpecificPlayerLayers(newPlayerID);
        }

        Destroy(gameObject);
    }

    private void RemoveUnitFromLists ()
    {
        if (!isSelectable)
            return;

        if (lastCell != null && lastCell.unitsInCell.Contains (cellIdentifier))
            lastCell.unitsInCell.Remove(cellIdentifier);

        PlayerHolder.RemoveUnit(identifier.GetPlayerID, this);
    }
    private void SetAnimations (AnimatorOverrideController newController)
    {
        animator.runtimeAnimatorController = newController;
    }

    public void SetUnitSpecificPlayerLayers (int playerID)
    {
        if (playerID < 0)
            return;

        selectedGO.layer = RuntimeLayerController.GetLayer(playerID);
        lineToDestinationVisual.gameObject.layer = RuntimeLayerController.GetLayer(playerID);
        movementTargetVisual.layer = RuntimeLayerController.GetLayer(playerID);


        Transform[] children = movementTargetVisual.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            children[i].gameObject.layer = RuntimeLayerController.GetLayer(playerID);
        }
    }

    public void UpdateBodyColor()
    {
        // turn on correct body parts
        for (int i = 0; i < bodyPartsNeedMaterial.Length; i++)
        {
            if (identifier.GetPlayerID < 0)
                bodyPartsNeedMaterial[i].material = PlayerColorManager.GetNonPlayerUnitMaterial;
            else
                bodyPartsNeedMaterial[i].material = PlayerColorManager.GetUnitMaterial(identifier.GetPlayerID);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = new Color (Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attacking.LookRangeWithHeight);

        Gizmos.color = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attacking.AttackRangeWithHeight);
    }
}
