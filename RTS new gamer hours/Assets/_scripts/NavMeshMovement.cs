using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;

//[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof (NavMeshAgent))]
public class NavMeshMovement : MonoBehaviour
{
    [SerializeField] private UnitStats stats;
    [SerializeField] private float standingRotSpeed = 25f;
    [SerializeField] private bool onlyLookWhenStill = true;
    [SerializeField] private bool disableMovement = false;

    //private Rigidbody rb;
    private NavMeshAgent agent;
    private Transform lookAtTransform;
    private Vector3? lookAtPos;
    private bool canTurn = true;
    private bool canMove = true;
    private bool isMoving = false;

    private Vector3? loadedNextPos = null;

    private Vector3 debugDestinationPos;
    private float debugMoveSpeed01 = 0;
    private float moveSpeed = 0f;

    public void SetMoveSpeed(float moveSpeed) => this.moveSpeed = moveSpeed;
    public float GetCurrentMoveSpeed => agent.velocity.magnitude;
    public UnitStats GetStats => stats;
    public bool GetIsMoving => isMoving;
    public void SetCanTurn (bool canTurn) => this.canTurn = canTurn;
    public void SetCanMove(bool canMove) => this.canMove = canMove;
    public float GetBaseOffset => agent.baseOffset;
    public Transform GetLookTarget => lookAtTransform;
    public float GetMoveSpeed01 => Mathf.Clamp01(agent.velocity.sqrMagnitude / (moveSpeed * moveSpeed));
    public Vector3 GetDestination => agent.destination;

    private void Awake()
    {
        //rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        agent.speed = stats.maxMoveSpeed;

        moveSpeed = stats.maxMoveSpeed;
    }

    private void Update()
    {
        isMoving = GetMoveSpeed01 > 0.05f;
        debugDestinationPos = agent.destination;
        debugMoveSpeed01 = GetMoveSpeed01;
        agent.speed = moveSpeed;

        // look at enemy if standing still
        if ((!onlyLookWhenStill || !isMoving) && (lookAtTransform || lookAtPos != null))
        {
            if (lookAtTransform)
                LookTowards(lookAtTransform.position - transform.position);
            else if (lookAtPos != null)
                LookTowards(lookAtPos.GetValueOrDefault() - transform.position);
        }
    }

    public void SetDestination (Vector3 target)
    {
        if (disableMovement)
            return;

        if (!canMove)
        {
            // we made the SetDestination call while we can't move
            // so load the next position for when we can move
            if (loadedNextPos == null)
            {
                loadedNextPos = target;
            }

            return;
        }

        // if we have a loaded position, set the target to that and discard it
        if (loadedNextPos != null)
        {
            target = loadedNextPos.GetValueOrDefault();
            loadedNextPos = null;
        }

        agent.SetDestination (target);
    }

    public void MoveTowards (Vector3 target)
    {
        if (!canMove)
            return;

        agent.velocity = agent.speed * (target - transform.position).normalized;
    }

    public void ResetDestination ()
    {
        // if we have a loaded position, we can't reset the destination yet
        if (loadedNextPos != null)
        {
            SetDestination(loadedNextPos.GetValueOrDefault());
            return;
        }

        agent.ResetPath();
    }

    public void SetLookAt (Transform lookAt)
    {
        lookAtTransform = lookAt;
    }
    public void SetLookAt (Vector3 lookAt)
    {
        lookAtPos = lookAt;
    }

    private void LookTowards(Vector3 dir)
    {
        if (!canTurn)
            return;

        dir.y = 0f;
        dir = dir.normalized;

        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
        Quaternion newRotation = Quaternion.Lerp(transform.rotation, lookRot, standingRotSpeed * Time.fixedDeltaTime);
        newRotation.eulerAngles = new Vector3(0f, newRotation.eulerAngles.y, 0f);
        transform.rotation = newRotation;
    }

    private void OnDrawGizmosSelected()
    {
        if (!agent)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(agent.destination, 0.2f);
    }
}
