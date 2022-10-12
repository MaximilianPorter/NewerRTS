using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    public TMP_Text display_Text;
    private int avgFrameRate;

    private readonly float timeBetweenUpdates = 0.2f;
    private float timeBetweenUpdatesCounter = 0f;
    public void Update()
    {
        timeBetweenUpdatesCounter += Time.deltaTime;

        if (timeBetweenUpdatesCounter > timeBetweenUpdates)
        {
            timeBetweenUpdatesCounter = 0f;
            float current = 0;
            current = (int)(1f / Time.unscaledDeltaTime);
            avgFrameRate = (int)current;
            display_Text.text = avgFrameRate.ToString() + " FPS";
        }
    }
}