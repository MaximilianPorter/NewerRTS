using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private UnitStats stats;

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
    private Vector3 lookDirAddition = Vector3.zero;
    private float lookAdditionStrength = 0f;

    public bool GetHasReachedMovePos => hasReachedMovePos;
    public Vector3 GetVelocity => rb.velocity;
    public Transform GetLookTarget => lookAtTarget;
    public void SetInputJumpDown(bool newInput) => inputJumpDown = newInput;
    public float GetMaxMoveSpeed => stats.maxMoveSpeed;
    public Vector3 GetMoveDir => moveInput;
    public bool GetIsGrounded => isGrounded;
    public bool GetCanMove => canMove;
    public Vector3 GetMoveTarget => moveTarget;
    public float GetMoveSpeed01 => Mathf.Clamp01(rb.velocity.sqrMagnitude / (stats.maxMoveSpeed * stats.maxMoveSpeed));

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        SetMoveTarget(transform.position);
        //wheatTuft.SetActive(false);
        //wheatRot = Quaternion.identity;

    }

    private void Update()
    {

        moveInput = (new Vector3(moveTarget.x, 0, moveTarget.z) - new Vector3(transform.position.x, 0f, transform.position.z));

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



        hasReachedMovePos = CloseToPos(moveTarget, stats.stopMovingDist);

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
        } else
            FixedLookTowards(moveInput == Vector3.zero ? transform.forward : moveInput);


        // move
        if (canMove)
        {
            rb.AddForce(transform.forward * stats.moveForce, ForceMode.Force);
            Vector3 clampedVelocity = rb.velocity;
            float maxSpeed = stats.maxMoveSpeed * slowMultiplier;

            clampedVelocity.x = Mathf.Clamp(clampedVelocity.x, -maxSpeed, maxSpeed);
            clampedVelocity.z = Mathf.Clamp(clampedVelocity.z, -maxSpeed, maxSpeed);
            clampedVelocity.y -= extraGravity * Time.fixedDeltaTime;
            rb.velocity = clampedVelocity;
        }
    }

    //private Vector3 FindUnobstructedDir()
    //{
    //    float furthestUnobstructedDist = 0f;
    //    Vector3 bestDir = transform.forward;
    //    for (int i = 0; i < rayCount; i++)
    //    {
    //        Vector3 towardsTarget = Vector3.Lerp(transform.forward, (moveTarget - transform.position), 0.3f);
    //        Vector3 dir = Vector3.Lerp(towardsTarget, Vector3.Cross(towardsTarget, Vector3.up) * (i % 2 == 0 ? 1f : -1f), (float)i / (float)rayCount);

    //        RaycastHit hit;
    //        if (Physics.SphereCast(transform.position, .01f, dir, out hit, castDist, wallHitMask))
    //        {
    //            if (hit.distance > furthestUnobstructedDist)
    //            {
    //                bestDir = dir;
    //                furthestUnobstructedDist = hit.distance;
    //            }

    //        }
    //        else
    //        {
    //            return dir;
    //        }
    //    }

    //    return bestDir;
    //}

    private bool CloseToPos (Vector3 pos, float softDist)
    {
        pos.y = 0f;
        Vector3 myPos = new Vector3(transform.position.x, 0f, transform.position.z);

        return (myPos - pos).sqrMagnitude <= softDist * softDist;
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

    public void SetLookDirAddition(Vector3 dir, float strength)
    {
        lookDirAddition = dir;
        lookAdditionStrength = strength;
    }

    private void FixedLookTowards (Vector3 dir)
    {
        if (dir.magnitude < 0.15f || !canTurn)
            return;

        dir = dir.normalized;

        // this is so they actually arrive at their move pos
        float rotSpeed = CloseToPos(moveTarget, 1f) ? 50f : rotationSpeed;

        Vector3 addDir = dir + Vector3.Lerp(Vector3.zero, lookDirAddition, lookAdditionStrength);

        Quaternion lookRot = Quaternion.LookRotation(addDir, Vector3.up);
        Quaternion newRotation = Quaternion.Lerp(rb.rotation, lookRot, rotSpeed * Time.fixedDeltaTime);
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

            Gizmos.color = new Color(1f, 0.5f, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + lookDirAddition * 2f);
        }
    }
}
