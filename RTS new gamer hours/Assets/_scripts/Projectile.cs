using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

[RequireComponent(typeof (Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private bool lookTowardsVelocity = true;
    [SerializeField] private bool destroyOnContact = false;
    [SerializeField] private bool burns = false;

    [SerializeField] private GameObject spawnOnHit;
    [SerializeField] private LayerMask hitMask;

    private ParentConstraint parentConstraint;
    private Rigidbody rb;
    private float damage = 0;
    private int teamID = -1;
    private int playerID = -1;
    private Vector3 lastPos = Vector3.zero;

    public Rigidbody GetRigidbody => rb;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
            Vector3 castPos = transform.position - transform.forward * 2f;
            if (Physics.Raycast(castPos, transform.forward, out hit, 2f, hitMask))
            {
                // don't hit anything that's on our team
                if (hit.transform.TryGetComponent(out Identifier testIdentifier) && testIdentifier.GetTeamID == teamID)
                    return;

                float diff = 2f - hit.distance;
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
                if (hit.transform.TryGetComponent(out Identifier identifier) && identifier.GetTeamID != teamID)
                {
                    if (hit.transform.TryGetComponent(out Health health))
                        health.TakeDamage(damage, playerID, transform.position);
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
        this.playerID = playerID;
        this.teamID = teamID;
    }

    public void ResetProjectile()
    {
        damage = 0f;
        teamID = 10000;

        rb.isKinematic = false;
        parentConstraint.SetSource(0, new ConstraintSource());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (Application.isPlaying)
        {
            Vector3 lastDir = -transform.forward;
            Gizmos.DrawRay(transform.position + lastDir.normalized * 2f, transform.forward * 2f);
        }
        //Gizmos.DrawWireSphere(transform.position, hitRadius);
    }


    /// <summary>
    /// Applies the force to the Rigidbody such that it will land, if unobstructed, at the target position.  The arch [0, 1] determines the percent of arch to provide between the minimum and maximum arch.  If target is out of range, it will fail to launch and return false; otherwise, it will launch and return true.  This only takes the Y gravity into account, and X gravity will not affect the trajectory.
    /// </summary>
    /// <param name="accuracy">determines between random[-accuracy, +accuracy] how far off the shot will be on the x and z axes</param>
    public static bool SetTrajectory(Rigidbody rigidbody, Vector3 target, float force, float accuracy = 0f, float arch = 0.5f, Vector3? targetVelocity = null)
    {
        target += new Vector3(Random.Range(-accuracy, accuracy), 0f, Random.Range(-accuracy, accuracy));
        float targetVelX = targetVelocity != null ? targetVelocity.GetValueOrDefault().x : 0f;
        float targetVelZ = targetVelocity != null ? targetVelocity.GetValueOrDefault().z : 0f;
        Mathf.Clamp(arch, 0, 1);
        var origin = rigidbody.position;
        float x = target.x - origin.x + targetVelX*1.2f;
        float y = target.y - origin.y;
        float z = target.z - origin.z + targetVelZ*1.2f;
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
        float vx = x / time;
        float vy = y / time + time * gravity / 2;
        float vz = z / time;
        var trajectory = new Vector3(vx, vy, vz) * rigidbody.mass;
        rigidbody.AddForce(trajectory, ForceMode.Impulse);
        return true;
    }
}


