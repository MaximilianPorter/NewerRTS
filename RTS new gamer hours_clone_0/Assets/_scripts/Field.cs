using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    [SerializeField] private GameObject killEffect;
    public void KillField ()
    {
        GameObject destroyedEffectInstance = Instantiate(killEffect, transform.position, Quaternion.identity);
        Destroy(destroyedEffectInstance, 5f);

        Destroy(gameObject.transform.parent.gameObject);
    }
}
