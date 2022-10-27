using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

[RequireComponent(typeof (Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private ProjectileType projectileType = ProjectileType.NONE;
    [SerializeField] private bool lookTowardsVelocity = true;
    [SerializeField] private bool destroyOnContact = false;
    [SerializeField] private bool burns = false;

    [SerializeField] private GameObject spawnOnHit;
    [SerializeField] private LayerMask hitMask;

    private ParentConstraint parentConstraint;
    private Rigidbody rb;
    private float damage = 0;
    private bool negatesArmor = false;
    private Identifier identifier;
    private Vector3 lastPos = Vector3.zero;

    public Rigidbody GetRigidbody => rb;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        identifier = GetComponent<Identifier>();
        parentConstraint = GetComponent<ParentConstraint>();
    }

    private void Start()
    {
        Destroy(gameObject, 10f);
    }

    private void FixedUpdate()
    {
        if (lookTowardsVelocity)
            transform.LookAt(transform.position + rb.velocity);

        // stop raycasting when we hit something
        if (rb.isKinematic)
            return;

        if (lastPos != Vector3.zero)
        {
            // check for hits
            RaycastHit hit;
            //Vector3 lastDir = (lastPos - transform.position);
            float castDistBack = 0.5f;
            Vector3 castPos = transform.position - transform.forward * castDistBack;
            if (Physics.Raycast(castPos, transform.forward, out hit, castDistBack, hitMask))
            {
                // don't hit anything that's on our team
                if (hit.transform.TryGetComponent(out Identifier testIdentifier) && testIdentifier.GetTeamID == identifier.GetTeamID)
                    return;

                float diff = castDistBack - hit.distance;
                transform.position += -transform.forward * diff;

                rb.isKinematic = true;
                rb.velocity = Vector3.zero;

                // setting parent without setting parent
                //ConstraintSource constraintSource = parentConstraint.GetSource(0);

                if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Ground"))
                {
                    transform.SetParent(hit.transform);

                    //constraintSource.sourceTransform = hit.transform;
                    //parentConstraint.SetRotationOffset(0, new Vector3 (-transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, -transform.rotation.eulerAngles.z));
                    //parentConstraint.constraintActive = true;
                }

               //parentConstraint.SetSource(0, constraintSource);

                // deal damage to different team
                if (hit.transform.TryGetComponent(out Identifier enemyIdentifier) && enemyIdentifier.GetTeamID != identifier.GetTeamID)
                {
                    if (hit.transform.TryGetComponent(out Health health))
                        health.TakeDamage(damage, identifier, transform.position, negatesArmor);
                }

                if (burns && hit.transform.TryGetComponent (out BurningObject burnedObject))
                {
                    burnedObject.ResetBurn();
                }

                if (spawnOnHit)
                {
                    GameObject hitEffectInstance = Instantiate(spawnOnHit, hit.point, Quaternion.identity);
                    Destroy(hitEffectInstance, 5f);
                }

                if (destroyOnContact)
                    Destroy(gameObject);
            }
        }

        lastPos = transform.position;
    }

    public void SetInfo (float damage, int playerID, int teamID)
    {
        this.damage = damage;
        identifier.SetPlayerID(playerID);
        identifier.SetTeamID(teamID);

        negatesArmor = PlayerHolder.GetCompletedResearch(identifier.GetPlayerID).Contains(BuyIcons.Research_SharpArrows);
    }

    public void ResetProjectile()
    {
        damage = 0f;
        identifier.SetPlayerID(-1);
        identifier.SetTeamID(-1);

        rb.isKinematic = false;
        parentConstraint.SetSource(0, new ConstraintSource());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (Application.isPlaying)
        {
            Vector3 lastDir = -transform.forward;
            Gizmos.DrawRay(transform.position + lastDir.normalized * .5f, transform.forward * .5f);
        }
        //Gizmos.DrawWireSphere(transform.position, hitRadius);
    }


    /// <summary>
    /// Applies the force to the Rigidbody such that it will land, if unobstructed, at the target position.  The arch [0, 1] determines the percent of arch to provide between the minimum and maximum arch.  If target is out of range, it will fail to launch and return false; otherwise, it will launch and return true.  This only takes the Y gravity into account, and X gravity will not affect the trajectory.
    /// </summary>
    /// <param name="accuracy">a percentage value between [0, 100] that randomly determins if the shot will hit</param>
    public static bool SetTrajectory(Rigidbody rigidbody, Vector3 target, float force, float accuracy = 0f, float arch = 0.5f, Vector3? targetVelocity = null)
    {
        Vector3 distanceMissed = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));

        if (Random.Range(0f, 100f) < accuracy)
            distanceMissed = Vector3.zero;

        target += distanceMissed;
        float targetVelX = targetVelocity != null ? targetVelocity.GetValueOrDefault().x : 0f;
        float targetVelY = targetVelocity != null ? targetVelocity.GetValueOrDefault().y : 0f;
        float targetVelZ = targetVelocity != null ? targetVelocity.GetValueOrDefault().z : 0f;
        Mathf.Clamp(arch, 0, 1);
        var origin = rigidbody.position;
        float x = target.x - origin.x;
        float y = target.y - origin.y;
        float z = target.z - origin.z;
        float gravity = -Physics.gravity.y;
        float b = force * force - y * gravity;
        float discriminant = b * b - gravity * gravity * (x * x + y * y);
        if (discriminant < 0)
        {
            return false;
        }
        float discriminantSquareRoot = Mathf.Sqrt(discriminant);
        float minTime = Mathf.Sqrt((b - discriminantSquareRoot) * 2) / Mathf.Abs(gravity);
        float maxTime = Mathf.Sqrt((b + discriminantSquareRoot) * 2) / Mathf.Abs(gravity);
        float time = (maxTime - minTime) * arch + minTime;
        float vx = x / time + targetVelX;
        float vy = y / time + time * gravity / 2 + targetVelY;
        float vz = z / time + targetVelZ;
        var trajectory = new Vector3(vx, vy, vz) * rigidbody.mass;
        rigidbody.AddForce(trajectory, ForceMode.Impulse);
        return true;
    }
}

public enum ProjectileType
{
    NONE = 0,
    Arrow = 1,
    Torch = 2,
}


