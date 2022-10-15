using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class TreeShake : MonoBehaviour
{
    [SerializeField] private GameObject treeDestroyEffect;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private Vector3 hitEffectOffset = new(0f, 1f, 0f);
    [SerializeField] private Rigidbody body;
    [SerializeField] private Button.ButtonClickedEvent onShake = new Button.ButtonClickedEvent();

    public Vector3 GetHitEffectOffset => hitEffectOffset;
    public GameObject GetHitEffect => hitEffect;

    private void Awake()
    {
    }

    /// <summary>
    /// shakes the tree in dir with force 0 - 1
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="force"></param>
    public void ShakeOnce (Vector3 dir, float force)
    {
        force = Mathf.Clamp01(force);

        body.AddTorque(dir.normalized * force * 10, ForceMode.Impulse);

        onShake.Invoke();
    }

    public void KillTree ()
    {
        GameObject treeDestroyedEffectInstance = Instantiate(treeDestroyEffect, transform.position + new Vector3 (0f, 0.5f, 0f), Quaternion.identity);
        Destroy(treeDestroyedEffectInstance, 5f);

        Destroy(gameObject);
    }

}
