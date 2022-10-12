using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

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
    //[SerializeField] private GameObject wheatTuft;
    //private Quaternion wheatRot;

    private bool isGrounded = false;
    private RaycastHit groundHitPoint;
    private Vector3 moveTarget = Vector3.zero;
    private Transform lookAtTarget;
    private Rigidbody rb;
    private CapsuleCollider col;
    private float slowMultiplier = 1f;
    private int currentSlowPriority = 0;
    private Vector3 debugReadVelocity;
    private bool hasReachedMovePos = false;

    private bool canMove = true;
    private bool canTurn = true;
    private bool additionalCanMove = true;
    private bool additionalCanTurn = true;
    private Vector3 moveInput = Vector3.zero;
    private bool inputJumpDown = false;

    public bool GetHasReachedMovePos => hasReachedMovePos;
    public Vector3 GetVelocity => rb.velocity;
    public Transform GetLookTarget => lookAtTarget;
    public void SetInputJumpDown(bool newInput) => inputJumpDown = newInput;
    public float GetMaxMoveSpeed => maxMoveSpeed;
    public Vector3 GetMoveDir => moveInput;
    public bool GetIsGrounded => isGrounded;
    public bool GetCanMove => canMove;
    public Vector3 GetMoveTarget => moveTarget;
    public float GetMoveSpeed01 => Mathf.Clamp01(rb.velocity.sqrMagnitude / (maxMoveSpeed * maxMoveSpeed));

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        if (stats)
        {
            moveForce = stats.moveForce;
            maxMoveSpeed = stats.maxMoveSpeed;
            stopMovingDist = stats.stopMovingDist;
        }

        SetMoveTarget(transform.position);
        //wheatTuft.SetActive(false);
        //wheatRot = Quaternion.identity;

    }

    private void Update()
    {
        moveInput = (new Vector3(moveTarget.x, 0, moveTarget.z) - new Vector3(transform.position.x, 0f, transform.position.z));
        moveInput.y = 0;

        if (isGrounded && inputJumpDown)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // check if grounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out groundHitPoint, (-col.center.y + col.height / 2f) + 0.1f, groundMask);
        rb.drag = isGrounded ? groundedDrag : inAirDrag;

        // update friction
        col.material = isGrounded ? groundedMat : inAirMat;



        hasReachedMovePos = (new Vector3(moveTarget.x, transform.position.y, moveTarget.z) - transform.position).sqrMagnitude <= stopMovingDist * stopMovingDist;
        if (hasReachedMovePos)
            SetMoveTarget(transform.position);

        canMove = additionalCanMove && !hasReachedMovePos && moveInput != Vector3.zero;
        canTurn = additionalCanTurn;
    }


    private void FixedUpdate()
    {
        debugReadVelocity = rb.velocity;

        // look towards target if there is one, if not, look in the direction you're moving
        if (lookAtTarget != null)
        {
            FixedLookTowards(lookAtTarget.position - transform.position);
        }else
            FixedLookTowards(moveInput == Vector3.zero ? transform.forward : moveInput);


        // move
        if (canMove)
        {
            rb.AddForce(transform.forward * moveForce, ForceMode.Force);
            Vector3 clampedVelocity = rb.velocity;
            clampedVelocity.x = Mathf.Clamp(clampedVelocity.x, -maxMoveSpeed * slowMultiplier, maxMoveSpeed * slowMultiplier);
            clampedVelocity.z = Mathf.Clamp(clampedVelocity.z, -maxMoveSpeed * slowMultiplier, maxMoveSpeed * slowMultiplier);
            clampedVelocity.y -= extraGravity * Time.fixedDeltaTime;
            rb.velocity = clampedVelocity;
        }
    }


    public void SetCanMove (bool newCanMove)
    {
        additionalCanMove = newCanMove;
    }

    public void SetCanTurn (bool newCanTurn)
    {
        additionalCanTurn = newCanTurn;
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

    private void FixedLookTowards (Vector3 dir)
    {
        if (dir.magnitude < 0.2f || !canTurn)
            return;

        //Vector3 cross = Vector3.Cross(dir, Vector3.up);
        dir = dir.normalized;

        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
        Quaternion newRotation = Quaternion.Lerp(rb.rotation, lookRot, rotationSpeed * Time.fixedDeltaTime);
        newRotation.eulerAngles = new Vector3(0f, newRotation.eulerAngles.y, 0f);
        rb.rotation = newRotation;
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.CompareTag("Field"))
    //    {
    //        //wheatTuft.SetActive(true);
    //        float distFromEdge = 0.3f;
    //        Bounds bounds = other.bounds;

    //        Vector3 clampedPos = new Vector3(Mathf.Clamp (transform.position.x, other.transform.position.x - bounds.size.x / 2f + distFromEdge, other.transform.position.x + bounds.size.x / 2f - distFromEdge),
    //            other.transform.position.y,
    //            Mathf.Clamp(transform.position.z, other.transform.position.z - bounds.size.z / 2f + distFromEdge, other.transform.position.z + bounds.size.z / 2f - distFromEdge));

    //        wheatTuft.transform.position = clampedPos;
    //        wheatTuft.transform.rotation = wheatRot;
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Field"))
    //    {
    //        wheatTuft.SetActive(false);
    //        wheatRot = Quaternion.LookRotation(Vector3.Lerp(Vector3.forward, -Vector3.right, Random.Range(0f, 1f)), Vector3.up);
    //    }
    //}

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Vector3.down * ((-col.center.y + col.height / 2f) + 0.1f));

            Gizmos.DrawSphere(moveTarget, 0.1f);
        }
    }
}
