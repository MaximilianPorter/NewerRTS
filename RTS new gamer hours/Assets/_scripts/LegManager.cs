using DitzelGames.FastIK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegManager : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private UnitStats stats;
    [SerializeField] private float maxStepDist = 1f;
    [SerializeField] private float legSwitchSpeed = 5f;

    [Space(10)]

    [SerializeField] private Movement movement;
    [SerializeField] private AnimationCurve legHeightCurve;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform bodyHolder;
    [SerializeField] private AnimationCurve bodyVerticalCurve;


    [SerializeField] private List<Leg> legs = new List<Leg>();

    private Rigidbody rb;
    private float moveSpeed01;
    private float[] legCounters;
    private Vector3[] nextPositions;
    private Vector3[] lastPositions;
    private bool[] legsGrounded;
    private int activeLeg = 0;
    private float legLength;

    [Serializable]
    private struct Leg
    {
        public LineRenderer rend;
        public Transform hip;
        public Transform target;
        public Transform jumpTargetPos;
    }

    public float GetLegLength => legLength;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (stats)
        {
            maxStepDist = stats.maxStepDistance;
            legSwitchSpeed = stats.legSwitchSpeed;
        }
    }

    private void Start()
    {
        legCounters = new float[legs.Count];
        nextPositions = new Vector3[legs.Count];
        lastPositions = new Vector3[legs.Count];
        legsGrounded = new bool[legs.Count];

        legLength = Vector3.Distance(legs[0].hip.transform.position, legs[0].hip.GetChild(0).GetChild(0).position);

        for (int i = 0; i < legs.Count; i++)
        {
            legs[i].target.position = legs[i].hip.position + new Vector3(0f, -legLength * 0.8f, 0f);
            lastPositions[i] = legs[i].hip.position + new Vector3(0f, -legLength * 0.8f, 0f);
        }
    }

    private void Update()
    {
        moveSpeed01 = Mathf.Clamp01(rb.velocity.sqrMagnitude / (movement.GetMaxMoveSpeed * movement.GetMaxMoveSpeed));

        if (!movement.GetIsGrounded)
        {
            for (int i = 0; i < legs.Count; i++)
            {
                legs[i].target.position = legs[i].jumpTargetPos.position;
                lastPositions[i] = legs[i].target.position;
            }
            return;
        }

        if (moveSpeed01 < 0.01f)
            return;

        // increment the active leg to move
        legCounters[activeLeg] += Time.deltaTime * legSwitchSpeed;

        bodyHolder.localPosition = new Vector3(0f, bodyVerticalCurve.Evaluate(legCounters[activeLeg]) * moveSpeed01, 0f);

        // keep next position for each leg chosen
        for (int i = 0; i < legs.Count; i++)
        {
            Vector3 legCastPos = legs[i].hip.position + (transform.forward * 0.001f) + rb.velocity.normalized * maxStepDist * moveSpeed01;
            RaycastHit hit;
            if (Physics.Raycast(legCastPos, Vector3.down, out hit, legLength, groundMask))
            {
                nextPositions[i] = hit.point;
            }


            // move leg
            Vector3 horLegMovement = Vector3.Lerp(lastPositions[i], nextPositions[i], legCounters[i]);
            legs[i].target.position = horLegMovement + new Vector3 (0f, legHeightCurve.Evaluate(legCounters[i]) * moveSpeed01 * legLength, 0f);
            


            // line renderers
            if (legs[i].rend)
            {
                legs[i].rend.SetPosition(0, legs[i].hip.position);
                legs[i].rend.SetPosition(1, legs[i].hip.GetChild (0).position);
                legs[i].rend.SetPosition(2, legs[i].hip.GetChild (0).GetChild (0).position);
            }
        }


        // active leg has reached it's location
        if (legCounters[activeLeg] > 1)
        {
            // set last position to the final target location
            lastPositions[activeLeg] = legs[activeLeg].target.position;
            legCounters[activeLeg] = 0f;
            legsGrounded[activeLeg] = true;
            SwitchToNextLeg();
            legsGrounded[activeLeg] = false;

        }

    }

    private void SwitchToNextLeg ()
    {
        if (activeLeg + 1 >= legs.Count)
            activeLeg = 0;
        else
            activeLeg++;
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < nextPositions.Length; i++)
            {
                Gizmos.DrawSphere(nextPositions[i], 0.1f);

            }
        }

        //Gizmos.DrawRay(body.position, body.velocity.normalized * 10f);
        //Gizmos.DrawRay(body.position, (new Vector3(rLegNextPos.x, rHip.position.y, rLegNextPos.z) - rHip.position).normalized * 10f);
    }
}
