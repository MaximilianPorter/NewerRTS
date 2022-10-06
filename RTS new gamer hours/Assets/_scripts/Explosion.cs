using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float radius = 20f;
    [SerializeField] [Range(0f, 1f)]private float force = 0.8f;

    private void Start()
    {
        Time.timeScale = 0.1f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        Collider[] trees = Physics.OverlapSphere(transform.position, radius);
        for (int i = 0; i < trees.Length; i++)
        {
            if (trees[i].TryGetComponent (out TreeShake tree))
            {
                tree.ShakeOnce(-Vector3.Cross ((tree.transform.position - transform.position), Vector3.up).normalized, force * (1 - Vector3.Distance (tree.transform.position, transform.position) / radius));
            }
        }
    }

    private void Update()
    {
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
