using System.Linq;
using UnityEngine;

[RequireComponent(typeof(NavMeshMovement))]
[RequireComponent(typeof(Attacking))]
[RequireComponent(typeof(Health))]
public class UnitActions : MonoBehaviour
{
    [SerializeField] private bool isSelectable = true;
    [SerializeField] private bool isTargetable = true;
    [SerializeField] private GameObject selectedGO;
    [SerializeField] private GameObject movementTargetVisual;
    [SerializeField] private UnitStats unitStats;
    [SerializeField] private bool debugDie = false;
    [SerializeField] private GameObject bloodSplatterImage;
    [SerializeField] private GameObject bloodGoreEffect;

    [Header("Physics")]
    [SerializeField] private LayerMask enemyMask;


    [Header("Visuals")]
    [SerializeField] private Renderer[] bodyPartsNeedMaterial;

    private GameObject healthBarInstance;

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

    private float findNearestEnemyCounter = 0f;

    private Identifier identifier;
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
    private float lookRangeWithHeight = 0f;
    private float attackRangeWithHeight = 0f;

    private int lastTeamID = 0;

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

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        navMovement = GetComponent<NavMeshMovement>();
        attacking = GetComponent<Attacking>();
        health = GetComponent<Health>();

        health.SetValues(unitStats.health);


        SetAnimations(unitOverrideController);
    }

    private void Start()
    {
        lastTeamID = identifier.GetTeamID;

        // turn on correct body parts
        for (int i = 0; i < bodyPartsNeedMaterial.Length; i++)
        {
            bodyPartsNeedMaterial[i].material = PlayerColorManager.GetUnitMaterial(identifier.GetTeamID);
        }


        if (isSelectable)
        {
            // add unit to list of all units for player
            PlayerHolder.AddUnit(identifier.GetPlayerID, this);
        }

        movementTargetVisual.SetActive(false);
        SetUnitSpecificPlayerLayers(identifier.GetPlayerID);
        //transform.SetParent(null);
        //identifier.SetIsParent(true);
    }

    private void Update()
    {
        selectedGO.SetActive(isSelected);
        attacking.SetCanAttack(navMovement.GetMoveSpeed01 < 0.01f);

        // increase look distace and attack distance with height up to 3x
        if (unitStats.isRanged)
        {
            float heightMultiplier = 1.8f;
            lookRangeWithHeight = unitStats.lookRange + Mathf.Clamp((-navMovement.GetBaseOffset + transform.position.y) * heightMultiplier, 0f, unitStats.lookRange * 2f);
            attackRangeWithHeight = unitStats.attackRange + Mathf.Clamp((-navMovement.GetBaseOffset + transform.position.y) * heightMultiplier, 0f, unitStats.attackRange * 2f);
        }
        else
        {
            lookRangeWithHeight = unitStats.lookRange;
            attackRangeWithHeight = unitStats.attackRange;
        }

        if (debugDie || health.GetIsDead)
            Die();

        if (animator)
        {
            isAttacking = animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
            isThrowing = animator.GetCurrentAnimatorStateInfo(0).IsName("Throw");
            navMovement.SetCanMove(!isAttacking && !isThrowing);
            navMovement.SetCanTurn(!isThrowing);

            if (isThrowing)
                animator.speed = throwAnimSpeed;
            else if (isAttacking)
                animator.speed = attackAnimSpeed / (unitStats.timeBetweenAttacks < 1 ? unitStats.timeBetweenAttacks : 1f);

            if (!isAttacking && !isThrowing)
            {
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


        UnitCellManager.UpdateActiveCell (identifier, transform.position, ref lastCell);


        findNearestEnemyCounter -= Time.deltaTime;
        if (navMovement.GetMoveSpeed01 > 0.1f || findNearestEnemyCounter <= 0)
        {
            CellFindNearestEnemy();
            findNearestEnemyCounter = Random.Range (0.1f, 0.3f);
        }

        if (identifier.GetTeamID != lastTeamID)
        {
            SwitchTeams(identifier.GetPlayerID, identifier.GetTeamID);
        }

        
    }

    private void CellFindNearestEnemy()
    {
        int cellsOutToCheck = Mathf.CeilToInt(lookRangeWithHeight / UnitCellManager.cellWidth);
        Cell activeCell = UnitCellManager.GetCell(transform.position);

        Vector2Int bottomLeft = new Vector2Int(activeCell.pos.x - cellsOutToCheck, activeCell.pos.y - cellsOutToCheck);
        Vector2Int topRight = new Vector2Int(activeCell.pos.x + cellsOutToCheck, activeCell.pos.y + cellsOutToCheck);
        Cell[] cellsAroundMe = UnitCellManager.GetCells(bottomLeft, topRight);


        attacking.SetNearestEnemy(ReturnClosestEnemy(cellsAroundMe));
    }

    private Transform ReturnClosestEnemy(Cell[] cellsToCheck)
    {
        Transform closestEnemy = null;

        //int totalUnitsAroundMe = cellsToCheck.Sum(cell => cell.unitsInCell.Count);

        for (int i = 0; i < cellsToCheck.Length; i++)
        {
            if (cellsToCheck[i] == null)
                continue;


            foreach (Identifier unit in cellsToCheck[i].unitsInCell)
            {
                if (unit == null)
                    continue;

                if (unit == identifier) // don't look for self
                    continue;

                if (unit.GetIsTargetable == false)
                    continue;

                Vector3 unitDir = (unit.transform.position - transform.position);
                unitDir.y = 0; // height doesn't matter

                if (unitDir.sqrMagnitude > lookRangeWithHeight * lookRangeWithHeight)
                {
                    continue;
                }
                

                // detect closest enemy
                if (unit.GetTeamID != identifier.GetTeamID)
                {
                    if (closestEnemy == null)
                        closestEnemy = unit.transform;
                    else if (unitDir.sqrMagnitude < (closestEnemy.position - transform.position).sqrMagnitude)
                        closestEnemy = unit.transform;
                }
            }




            // if we already found an enmy in the first 12 squares, there's no point to keep checking
            if (i >= 12 && closestEnemy != null)
                break;
        }

        return closestEnemy;
    }

    //private Transform FindNearestEnemyPhysics(Vector3 fromPoint)
    //{
    //    Transform closestEnemy = null;

    //    Collider[] nearbyUnits = Physics.OverlapSphere(fromPoint, unitStats.lookRange, enemyMask).Where(unit => unit.GetComponent<Identifier>().GetTeamID != identifier.GetTeamID).ToArray();

    //    if (nearbyUnits.Length <= 0)
    //    {
    //        return null;
    //    }

    //    foreach (Collider enemyCol in nearbyUnits)
    //    {
    //        if (closestEnemy == null || (enemyCol.transform.position - fromPoint).sqrMagnitude <
    //            (nearestEnemy.position - fromPoint).sqrMagnitude)
    //        {
    //            nearestEnemy = enemyCol.transform;
    //        }
    //    }
    //}

    private void HandleMoveTowardsEnemies ()
    {
        Vector3 moveTowardsPos = navMovement.GetDestination;
        Transform enemy = attacking.GetNearestEnemy;

        if (enemy)
        {
            Vector3 dirMoveTargetAndEnemy = (moveTowardsPos - enemy.position);
            dirMoveTargetAndEnemy.y = 0f;
            Vector3 dirMoveTargetAndMe = (moveTowardsPos - transform.position);
            dirMoveTargetAndMe.y = 0f;

            // (MOVING) if we click far enough away from the enemy
            if (navMovement.GetIsMoving && dirMoveTargetAndEnemy.sqrMagnitude > (unitStats.lookRange/2f * unitStats.lookRange/2f))
            {
                navMovement.SetLookAt(null);
                return;
            }
            // (NOT MOVING) if we click far enough away from our own range
            else if (!navMovement.GetIsMoving && dirMoveTargetAndMe.sqrMagnitude > (unitStats.lookRange/2f * unitStats.lookRange/2f))
            {
                navMovement.SetLookAt(null);
                return;
            }

            // if we click and there's an enemy close to that clickPos (range / 2f) then we go to that enemy
            // or we're standing still and the enemy is in our range
            else
            {
                Vector3 dirEnemyAndMe = (transform.position - enemy.position);
                dirEnemyAndMe.y = 0f;

                float sqrDistFromEnemy = dirEnemyAndMe.sqrMagnitude;
                if (enemy.TryGetComponent (out Building enemyBuilding))
                {
                    if (sqrDistFromEnemy > enemyBuilding.GetStats.interactionRadius * enemyBuilding.GetStats.interactionRadius)
                    {
                        // if we're out of range to throw shit on the building
                        navMovement.SetDestination (enemy.position + dirEnemyAndMe.normalized * (enemyBuilding.GetStats.interactionRadius - 0.1f));
                    }
                    else
                    {
                        // if we're in range to lob that stinky poo poo onto the building
                        navMovement.ResetDestination();
                        navMovement.SetLookAt(enemy);

                        if (throwCounter > timeBetweenThrows)
                            StartThrow(enemy.position);
                    }
                }
                else
                {
                    if (sqrDistFromEnemy > attackRangeWithHeight * attackRangeWithHeight)
                    {
                        // if we're out of range of attacking
                        // move close enough to attack
                        navMovement.SetDestination(enemy.position);
                        navMovement.SetLookAt(null);
                    }
                    else
                    {
                        // we're in range of attacking
                        navMovement.ResetDestination();
                        navMovement.SetLookAt(enemy);

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
        torchInstance.SetInfo(torchDamage, identifier.GetPlayerID, identifier.GetTeamID);

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
        newUnitInstance.SetPlayerID(newPlayerID);
        newUnitInstance.SetTeamID(newTeamID);

        if (newUnitInstance.TryGetComponent (out UnitActions newUnitActions))
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

        lastCell.unitsInCell.Remove(identifier);
        PlayerHolder.RemoveUnit(identifier.GetPlayerID, this);
    }
    private void SetAnimations (AnimatorOverrideController newController)
    {
        animator.runtimeAnimatorController = newController;
    }

    public void SetUnitSpecificPlayerLayers (int playerID)
    {
        selectedGO.layer = RuntimeLayerController.GetLayer(playerID);
        movementTargetVisual.layer = RuntimeLayerController.GetLayer(playerID);


        Transform[] children = movementTargetVisual.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            children[i].gameObject.layer = RuntimeLayerController.GetLayer(playerID);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = new Color (Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.3f);
        Gizmos.DrawWireSphere(transform.position, lookRangeWithHeight);

        Gizmos.color = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRangeWithHeight);
    }
}
