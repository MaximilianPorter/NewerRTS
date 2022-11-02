using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameGraphManager : MonoBehaviour
{
    [SerializeField] private GameObject endGameMenu;
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private Image fillRestartImage;
    [SerializeField] private LineRenderer[] playerLines;

    [SerializeField] private PlayerGraphTypes viewingGraphType = PlayerGraphTypes.ResourceEfficiency;
    [SerializeField] private GraphTypeHeaders[] graphTypeHeaders;
    [SerializeField] private float graphWidth = 8.5f;
    [SerializeField] private float graphHeight = 4f;

    private List<float>[] resourcesPerMinute = new List<float>[4]
    {
        new List<float>(),
        new List<float>(),
        new List<float>(),
        new List<float>(),
    };

    private List<int>[] unitCountPerMinute = new List<int>[4]
    {
        new List<int>(),
        new List<int>(),
        new List<int>(),
        new List<int>(),
    };

    private float timeBetweenSnapshots = 1f;
    private float snapshotCounter = 0f;

    private float timeToRestartGame = 5f;
    private float restartGameCounter = 0f;


    [Serializable]
    private struct GraphTypeHeaders
    {
        public string HeaderName;
        public PlayerGraphTypes graphType;
    }

    private void Update()
    {
        endGameMenu.SetActive(GameWinManager.instance.GameOver);

        // take snapshots while game is still going
        if (!GameWinManager.instance.GameOver)
        {
            snapshotCounter += Time.deltaTime;
            if (snapshotCounter > timeBetweenSnapshots)
            {
                snapshotCounter = 0f;
                TakeSnapshot();
            }
        }



        // if the game is still going, return
        if (!GameWinManager.instance.GameOver)
            return;

        HandleChangingGraphType();
        HandleGoToMenu();

        for (int i = 0; i < playerLines.Length; i++)
        {

            // set colors
            playerLines[i].startColor = PlayerColorManager.GetPlayerColor(i);
            playerLines[i].endColor = PlayerColorManager.GetPlayerColor(i);

            headerText.text = graphTypeHeaders.FirstOrDefault(graphHeader => graphHeader.graphType == viewingGraphType).HeaderName;

            // set points
            if (viewingGraphType == PlayerGraphTypes.ResourceEfficiency)
            {
                playerLines[i].positionCount = resourcesPerMinute[i].Count;
                for (int j = 0; j < resourcesPerMinute[i].Count; j++)
                {
                    playerLines[i].SetPosition(j, new Vector3(
                        (float)j / resourcesPerMinute[i].Count * graphWidth * 2f - graphWidth,
                        resourcesPerMinute[i][j] / Mathf.Max (1f, resourcesPerMinute[i].Max()) * graphHeight * 2f - graphHeight,
                        0f));
                }
            }
            else if (viewingGraphType == PlayerGraphTypes.UnitCount)
            {
                playerLines[i].positionCount = unitCountPerMinute[i].Count;
                for (int j = 0; j < unitCountPerMinute[i].Count; j++)
                {
                    playerLines[i].SetPosition(j, new Vector3(
                        (float)j / unitCountPerMinute[i].Count * graphWidth * 2f - graphWidth,
                        unitCountPerMinute[i][j] / Mathf.Max(1f, unitCountPerMinute[i].Max()) * graphHeight * 2f - graphHeight,
                        0f));
                }
            }
            
        }
    }

    private void TakeSnapshot ()
    {
        for (int i = 0; i < 4; i++)
        {
            // resources per minute
            ResourceAmount playerResourcesPerMinute = PlayerResourceManager.GetResourcesPerMinute(i);
            resourcesPerMinute[i].Add(playerResourcesPerMinute.GetFood + playerResourcesPerMinute.GetWood + playerResourcesPerMinute.GetStone);

            // unit count
            unitCountPerMinute[i].Add(PlayerHolder.GetUnits(i).Count);
        }
    }

    private void HandleChangingGraphType()
    {
        for (int i = 0; i < 4; i++)
        {
            if (PlayerInput.GetPlayers[i].GetButtonDown(PlayerInput.GetCycleRight))
            {
                int lengthOfEnum = Enum.GetNames(typeof(PlayerGraphTypes)).GetLength(0);
                if ((int)viewingGraphType + 1 >= lengthOfEnum)
                {
                    viewingGraphType = 0;
                }
                else
                {
                    viewingGraphType++;
                }
            }
        }
    }
    private void HandleGoToMenu ()
    {
        for (int i = 0; i < 4; i++)
        {
            if (PlayerInput.GetPlayers[i].GetButton(PlayerInput.GetInputSelectUnits))
            {
                restartGameCounter += Time.deltaTime;
            }
            if (PlayerInput.GetPlayers[i].GetButtonUp(PlayerInput.GetInputSelectUnits))
                restartGameCounter = 0f;
        }
        fillRestartImage.fillAmount = restartGameCounter / timeToRestartGame;

        if (restartGameCounter > timeToRestartGame)
        {
            SceneController.ChangeScene(0);
        }
    }

}

public enum PlayerGraphTypes
{
    ResourceEfficiency = 0,
    UnitCount = 1,
}
