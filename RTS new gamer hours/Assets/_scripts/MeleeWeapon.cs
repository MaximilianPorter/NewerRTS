using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private float radius = 2f;
    [SerializeField] private float force = 0.1f;

    [SerializeField] private GameObject woodHitEffect;
    [SerializeField] private GameObject wheatHitEffect;

    public void Attack ()
    {
        Collider[] hits = Physics.OverlapSphere (transform.position, radius, hitMask);

        Collider firstTree = hits.FirstOrDefault(tree => tree.CompareTag("Tree"));
        if (firstTree != null)
        {
            Vector3 dir = Vector3.Cross(firstTree.transform.position - transform.position, Vector3.up).normalized;
            firstTree.GetComponent <TreeShake>().ShakeOnce(-dir, force);
            GameObject woodHitInstance = Instantiate(woodHitEffect, firstTree.transform.position + new Vector3 (0f, 1f, 0f), Quaternion.identity);
            Destroy(woodHitInstance, 5f);
        }

        Collider wheat = hits.FirstOrDefault(hit => hit.CompareTag("Field"));
        if (wheat != null)
        {
            GameObject wheatHitInstance = Instantiate(wheatHitEffect, transform.position, Quaternion.identity);
            Destroy(wheatHitInstance, 5f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
