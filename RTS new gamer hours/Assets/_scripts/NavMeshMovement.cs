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

    //private Rigidbody rb;
    private NavMeshAgent agent;
    private Transform lookAtTransform;
    private bool canTurn = true;
    private bool canMove = true;
    private bool isMoving = false;

    private Vector3? loadedNextPos = null;

    private Vector3 debugDestinationPos;
    private float debugMoveSpeed01 = 0;
    private float inGroupMoveSpeed = 0f;

    public void SetGroupMoveSpeed(float moveSpeed) => inGroupMoveSpeed = moveSpeed;
    public float GetCurrentMoveSpeed => agent.velocity.magnitude;
    public UnitStats GetStats => stats;
    public bool GetIsMoving => isMoving;
    public void SetCanTurn (bool canTurn) => this.canTurn = canTurn;
    public void SetCanMove(bool canMove) => this.canMove = canMove;
    public Transform GetLookTarget => lookAtTransform;
    public float GetMoveSpeed01 => Mathf.Clamp01(agent.velocity.sqrMagnitude / (inGroupMoveSpeed * inGroupMoveSpeed));
    public Vector3 GetDestination => agent.destination;

    private void Awake()
    {
        //rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        agent.SetDestination(transform.position);
        agent.speed = stats.maxMoveSpeed;

        inGroupMoveSpeed = stats.maxMoveSpeed;
    }

    private void Update()
    {
        isMoving = GetMoveSpeed01 > 0.05f;
        debugDestinationPos = agent.destination;
        debugMoveSpeed01 = GetMoveSpeed01;
        agent.speed = inGroupMoveSpeed;

        // look at enemy if standing still
        if (!isMoving && lookAtTransform)
        {
            LookTowards(lookAtTransform.position - transform.position);
        }
    }

    public void SetDestination (Vector3 target)
    {
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
}
