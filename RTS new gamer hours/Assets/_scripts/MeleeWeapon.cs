using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Identifier))]
public class MeleeWeapon : MonoBehaviour
{
    //[SerializeField] private LayerMask hitMask;
    //[SerializeField] private float radius = 2f;
    //[SerializeField] private float force = 0.1f;

    ////[SerializeField] private float attackingKnockbackForce = 0.1f;

    //[SerializeField] private GameObject defaultHitEffect;
    //[SerializeField] private GameObject woodHitEffect;
    //[SerializeField] private GameObject wheatHitEffect;

    //private Identifier identifier;
    //private Attacking attacking;

    //private void Start()
    //{
    //    identifier = GetComponent<Identifier>();
    //    attacking = GetComponentInParent<Attacking>();
    //}

    //public void Attack ()
    //{
    //    Collider[] hits = Physics.OverlapSphere (transform.position, radius, hitMask);

    //    Collider firstTree = hits.FirstOrDefault(tree => tree.CompareTag("Tree"));
    //    if (firstTree != null)
    //    {
    //        Vector3 dir = Vector3.Cross(firstTree.transform.position - transform.position, Vector3.up).normalized;
    //        firstTree.GetComponent <TreeShake>().ShakeOnce(-dir, force);
    //        GameObject woodHitInstance = Instantiate(woodHitEffect, firstTree.transform.position + new Vector3 (0f, 1f, 0f), Quaternion.identity);
    //        Destroy(woodHitInstance, 5f);
    //    }

    //    Collider wheat = hits.FirstOrDefault(hit => hit.CompareTag("Field"));
    //    if (wheat != null)
    //    {
    //        GameObject wheatHitInstance = Instantiate(wheatHitEffect, transform.position, Quaternion.identity);
    //        Destroy(wheatHitInstance, 5f);
    //    }


    //    // hits unit with different team ID
    //    Collider enemy = hits.FirstOrDefault(hit => hit.TryGetComponent(out Identifier otherId) && otherId.GetTeamID != identifier.GetTeamID);
    //    if (enemy != null)
    //    {
    //        if (PlayerHolder.GetUnits(identifier.GetPlayerID).Count > 100)
    //            if (Random.Range(0f, 1f) > 0.5f)
    //                return;

    //        // hit effect
    //        GameObject hitEffectInstance = Instantiate(defaultHitEffect, enemy.ClosestPoint(transform.position), Quaternion.identity);
    //        Destroy(hitEffectInstance, 1f);

    //        // damage enemy unit
    //        if (enemy.TryGetComponent(out Health enemyHealth))
    //            if (attacking) enemyHealth.TakeDamage(attacking.GetStats.damage, null, transform.position);
    //        //unit.attachedRigidbody.AddForce((unit.transform.position - transform.position).normalized * attackingKnockbackForce, ForceMode.Impulse);
    //    }
    //}

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, radius);
    //}
}
