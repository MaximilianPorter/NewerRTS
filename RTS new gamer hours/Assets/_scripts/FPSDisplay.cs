using UnityEngine;
using TMPro;
using Rewired.Utils.Classes.Data;

public class FPSDisplay : MonoBehaviour
{
    [SerializeField] private bool displayFPS = false;
    private int avgFrameRate;

    private readonly float timeBetweenUpdates = 0.2f;
    private float timeBetweenUpdatesCounter = 0f;
    public void Update()
    {
        timeBetweenUpdatesCounter += Time.deltaTime;

        if (timeBetweenUpdatesCounter > timeBetweenUpdates)
        {
            timeBetweenUpdatesCounter = 0f;
            avgFrameRate = (int)(1f / Time.unscaledDeltaTime);
        }
    }

    private void OnGUI()
    {
        if (displayFPS)
        {
            Vector2 pos = new Vector2(Screen.width / 2f - 50f, 10f);
            string fps = avgFrameRate.ToString() + " FPS";

            // drop shadow
            GUI.color = Color.black;
            GUI.Label(new Rect (pos + Vector2.one, Vector2.one * 100f), fps);

            // text color
            GUI.color = Color.white;
            GUI.Label(new Rect(pos, Vector2.one * 100f), fps);
        }
    }
}