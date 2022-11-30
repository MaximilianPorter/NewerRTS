using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FadeBloodSplatter : MonoBehaviour
{
    [SerializeField] private float timeToFade = 60f;
    [SerializeField] private Projector[] projectors;


    private float fadeCounter = 0f;
    private float[] startFarClipPlanes;

    private void Start()
    {
        startFarClipPlanes = new float[projectors.Length];
        for (int i = 0; i < startFarClipPlanes.Length; i++)
        {
            startFarClipPlanes[i] = projectors[i].farClipPlane;
        }
    }

    private void Update()
    {
        fadeCounter += Time.deltaTime;
        for (int i = 0; i < projectors.Length; i++)
        {
            projectors[i].farClipPlane = Mathf.Lerp(startFarClipPlanes[i], 0f, fadeCounter / timeToFade);
        }

        if (fadeCounter > timeToFade)
            Destroy (gameObject);
    }
}
