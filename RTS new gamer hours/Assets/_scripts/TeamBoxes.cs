using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TeamBoxes : MonoBehaviour
{
    [SerializeField] private PracticeAreaTeamBox[] teamBoxes;

    [SerializeField] private float countdownTime = 5f;
    [SerializeField] private TMP_Text countdownText;

    private float counter;

    private void Start()
    {
        counter = countdownTime;
    }

    private void Update()
    {
        if (teamBoxes.Any (box => box.GetTouchingPlayers.Length > 0))
        {
            counter -= Time.deltaTime;
            countdownText.text = counter.ToString("F0");
        }
        else
        {
            counter = countdownTime;
            countdownText.text = "";
        }

        if (counter <= 0)
        {
            counter = countdownTime;
            SceneController.ChangeScene(1);
        }
    }
}
