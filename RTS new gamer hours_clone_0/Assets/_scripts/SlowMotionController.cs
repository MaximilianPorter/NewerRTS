using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowMotionController : MonoBehaviour
{
    [Range(0f, 3f)]public float timeValue = 1f;

    private float lastTimeValue = 1f;

    private void Update()
    {
        if (lastTimeValue != timeValue)
        {
            lastTimeValue = timeValue;

            Time.timeScale = timeValue;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }
    }
}
