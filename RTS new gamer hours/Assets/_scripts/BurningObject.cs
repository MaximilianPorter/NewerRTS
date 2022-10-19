using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningObject : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] burnEffects;
    [SerializeField] private float burnDuration = 5f;
    [SerializeField] private float damagePerSecond = 1f;

    private float burnCounter = 0f;
    private Health health;

    public bool GetIsBurning => burnCounter > 0;

    private void Awake()
    {
        health = GetComponent<Health>();
        
    }

    private void Start()
    {

        for (int i = 0; i < burnEffects.Length; i++)
        {
            if (burnEffects[i].isPlaying)
                burnEffects[i].Stop();
        }
    }

    private void Update()
    {
        if (burnCounter > 0)
        {
            burnCounter -= Time.deltaTime;

            if (health)
            {
                health.TakeDamage (Time.deltaTime * damagePerSecond, health.GetLastHitByPlayer, transform.position);
            }
        }

        for (int i = 0; i < burnEffects.Length; i++)
        {
            if (burnCounter > 0 && !burnEffects[i].isPlaying)
                burnEffects[i].Play();
            else if (burnCounter <= 0 && burnEffects[i].isPlaying)
                burnEffects[i].Stop();
        }

    }

    public void ResetBurn()
    {
        burnCounter = burnDuration;
    }
}
