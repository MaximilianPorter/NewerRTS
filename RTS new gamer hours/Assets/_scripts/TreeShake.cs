using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TreeShake : MonoBehaviour
{
    [SerializeField] private GameObject treeDestroyEffect;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// shakes the tree in dir with force 0 - 1
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="force"></param>
    public void ShakeOnce (Vector3 dir, float force)
    {
        force = Mathf.Clamp01(force);

        rb.AddTorque(dir.normalized * force * 10, ForceMode.Impulse);
    }

    public void KillTree ()
    {
        GameObject treeDestroyedEffectInstance = Instantiate(treeDestroyEffect, transform.position + new Vector3 (0f, 0.5f, 0f), Quaternion.identity);
        Destroy(treeDestroyedEffectInstance, 5f);

        Destroy(gameObject);
    }

}
