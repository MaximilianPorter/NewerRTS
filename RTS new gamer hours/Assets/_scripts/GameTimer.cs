using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private float counter = 0f;

    private void Update()
    {
        counter += Time.deltaTime;
        int seconds = Mathf.FloorToInt(counter % 60);
        int minutes = Mathf.FloorToInt(counter / 60f);
        string withZero = Mathf.FloorToInt((float)seconds / 10f) <= 0 ? "0" : "";
        timerText.text = minutes + ":" + withZero + seconds;
    }
}
