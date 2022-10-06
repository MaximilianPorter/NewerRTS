using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

[RequireComponent (typeof (Identifier))]
public class Movement : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private UnitStats stats;
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float maxMoveSpeed = 5f;
    [SerializeField] private float stopMovingDist = 0.02f;

    [Space(10)]

    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float jumpForce = 100f;
    [SerializeField] private float inAirDrag = 0f;
    [SerializeField] private float groundedDrag = 2f;
    [SerializeField] private float extraGravity = 2f;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private PhysicMaterial groundedMat;
    [SerializeField] private PhysicMaterial inAirMat;

    private bool isGrounded = false;
    private RaycastHit groundHitPoint;
    private Vector3 moveTarget = Vector3.zero;
    private Transform lookAtTarget;
    private Rigidbody rb;
    private BoxCollider col;
    private float slowMultiplier = 1f;
    private int currentSlowPriority = 0;

    private bool canMove = true;
    private Vector3 moveInput = Vector3.zero;
    private bool inputJumpDown = false;
    private Identifier playerIdentifier;

    public float GetMaxMoveSpeed => maxMoveSpeed;
    public bool GetIsGrounded => isGrounded;
    public bool GetCanMove => canMove;
    public Vector3 GetMoveTarget => moveTarget;

    private void Awake()
    {
        if (TryGetComponent(out Identifier identifier))
        {
            playerIdentifier = identifier;
        }

        rb = GetComponent<Rigidbody>();
        col = GetComponent<BoxCollider>();

        if (stats)
        {
            moveForce = stats.moveForce;
            maxMoveSpeed = stats.maxMoveSpeed;
            stopMovingDist = stats.stopMovingDist;
        }
    }

    private void Update()
    {
        if (playerIdentifier.GetIsPlayer)
        {
            moveInput = new Vector3(PlayerInput.players[playerIdentifier.GetPlayerID].GetAxis(PlayerInput.GetInputMoveHorizontal),
                0f,
                PlayerInput.players[playerIdentifier.GetPlayerID].GetAxis(PlayerInput.GetInputMoveVertical));

            inputJumpDown = PlayerInput.players[playerIdentifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputJump);
        }
        else
        {
            moveInput = moveTarget == Vector3.zero ? Vector3.zero : (moveTarget - transform.position).normalized;
        }
        moveInput.y = 0;


        if (isGrounded && inputJumpDown)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // check if grounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out groundHitPoint, (-col.center.y + col.size.y / 2f) + 0.1f, groundMask);
        rb.drag = isGrounded ? groundedDrag : inAirDrag;

        // update friction
        col.material = isGrounded ? groundedMat : inAirMat;

        canMove = (new Vector3(moveTarget.x, transform.position.y, moveTarget.z) - transform.position).sqrMagnitude > stopMovingDist * stopMovingDist
            && moveInput != Vector3.zero
            && ((playerIdentifier.GetIsPlayer && !PlayerInput.GetPlayerIsInMenu(playerIdentifier.GetPlayerID)) || !playerIdentifier.GetIsPlayer);




        if (!playerIdentifier.GetIsPlayer)
        {
            if (canMove)
            {
                LookTowards(moveInput);
            }
            else if (lookAtTarget != null)
            {
                LookTowards(lookAtTarget.position - transform.position);
            }
        }else if (canMove)
        {
            LookTowards(moveInput);
        }
    }

    private void FixedUpdate()
    {
        // move
        if (canMove)
        {
            rb.AddForce(transform.forward * moveForce * moveInput.magnitude, ForceMode.Force);
            Vector3 clampedVelocity = rb.velocity;
            clampedVelocity.x = Mathf.Clamp(clampedVelocity.x, -maxMoveSpeed * slowMultiplier, maxMoveSpeed * slowMultiplier);
            clampedVelocity.z = Mathf.Clamp(clampedVelocity.z, -maxMoveSpeed * slowMultiplier, maxMoveSpeed * slowMultiplier);
            clampedVelocity.y -= extraGravity * Time.fixedDeltaTime;
            rb.velocity = clampedVelocity;
        }
    }



    public void SetMoveTarget (Vector3 newTarget)
    {
        moveTarget = newTarget;
    }

    public void SetLookAt (Transform newLookAtTarget)
    {
        lookAtTarget = newLookAtTarget;
    }

    /// <summary>
    /// Sets the value of the slowMultiplier to determine the max movespeed (maxmovespeed * multiplier)
    /// </summary>
    /// <param name="priority"> if this value is higher than the current value, it will override the multiplier </param>
    /// <param name="multiplier"> 0 - 1 0=not moving, 1=normal movement </param>
    /// <returns>returns true if setting the multiplier was succesful</returns>
    public bool SetSlowMultiplier (int priority, float multiplier)
    {
        if (priority >= currentSlowPriority)
        {
            slowMultiplier = multiplier;
            return true;
        }
        return false;
    }

    private void LookTowards (Vector3 dir)
    {
        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
        rb.rotation = Quaternion.Lerp(rb.rotation, lookRot, rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Vector3.down * ((-col.center.y + col.size.y / 2f) + 0.1f));
        }
    }
}
