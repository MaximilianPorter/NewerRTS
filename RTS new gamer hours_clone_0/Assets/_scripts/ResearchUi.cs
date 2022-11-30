using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchUi : MonoBehaviour
{
    [SerializeField] private ResearchStats researchStats;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image completedImage;
    [SerializeField] private TMP_Text timeRemainingText;

    private float counter = 0f;
    private bool isResearching = false;
    private bool isFinished = false;

    public bool GetIsResearching => isResearching;
    public void StartResearch() => isResearching = true;
    public void StopResearch() => isResearching = false;

    public ResearchStats GetStats => researchStats;
    public bool GetIsFinished => isFinished;

    private void Update()
    {
        if (isResearching)
            counter = Mathf.Clamp(counter + Time.deltaTime, 0f, researchStats.timeToResearch);
        else if (!isFinished)
            counter = 0f;

        fillImage.fillAmount = counter / researchStats.timeToResearch;


        if (counter >= researchStats.timeToResearch)
        {
            isFinished = true;
            isResearching = false;
        }

        if (isFinished)
        {
            completedImage.enabled = true;
            fillImage.enabled = false;
            timeRemainingText.text = "";
        }
        else
        {
            completedImage.enabled = false;
            fillImage.enabled = true;
            timeRemainingText.text = Mathf.Abs(counter - researchStats.timeToResearch).ToString("F0");
        }
    }
}
