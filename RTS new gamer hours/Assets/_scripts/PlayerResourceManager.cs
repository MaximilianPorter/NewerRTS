using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerResourceManager : MonoBehaviour
{
    public static PlayerResourceManager instance;

    [SerializeField] private int debugStartResources = 0;
    [SerializeField] private GameObject giveResourcesUiPrefab;

    private static Canvas[] playerCanvases;
    private static Camera[] playerCameras;
    private Identifier[] playerIDs;

    private bool[] usedCheatCode = new bool[4] { false, false, false, false };

    private int[] debugFood = new int[4] { 0, 0, 0, 0 };
    private int[] debugWood = new int[4] { 0, 0, 0, 0 };
    private int[] debugStone = new int[4] { 0, 0, 0, 0 };

    public static int[] PopulationCap = new int[4] { 0, 0, 0, 0 };
    public static ResourceAmount[] PlayerResourceAmounts = new ResourceAmount[4]
    {
        new ResourceAmount (),
        new ResourceAmount (),
        new ResourceAmount (),
        new ResourceAmount ()
    };

    public static Canvas[] GetPlayerCanvas => playerCanvases;
    public static Camera[] GetPlayerCamera => playerCameras;



    private void Awake()
    {
        instance = this;


        for (int i = 0; i < 4; i++)
        {
            PlayerResourceAmounts[i].SetFood(debugStartResources);
            PlayerResourceAmounts[i].SetWood(debugStartResources);
            PlayerResourceAmounts[i].SetStone(debugStartResources);
        }
    }

    private void Start()
    {
        playerCanvases = FindObjectsOfType<Canvas>().Where(canvas => canvas.GetComponent<Identifier>()).OrderBy(canvas => canvas.GetComponent<Identifier>().GetPlayerID).ToArray();
        playerCameras = Camera.allCameras.Where(cam => cam.GetComponent<Identifier>()).OrderBy(cam => cam.GetComponent<Identifier>().GetPlayerID).ToArray();
        playerIDs = FindObjectsOfType<Identifier>().Where(identifier => identifier.GetIsPlayer == true).ToArray();
        
    }

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            debugFood[i] = PlayerResourceAmounts[i].GetFood;
            debugWood[i] = PlayerResourceAmounts[i].GetWood;
            debugStone[i] = PlayerResourceAmounts[i].GetStone;

            PopulationCap[i] = PlayerHolder.GetBuildings(i).Sum(building => building.GetStats.population);
        }

        HandleCheatCode();
    }

    /// <summary>
    /// adds resources to every player on that team
    /// </summary>
    public void AddResourcesToTeam(int teamID, ResourceAmount amount)
    {
        for (int i = 0; i < playerIDs.Length; i++)
        {
            if (playerIDs[i].GetTeamID == teamID)
            {
                PlayerResourceAmounts[playerIDs[i].GetPlayerID].AddResources(amount);
            }
        }
    }

    public void AddResourcesWithUI (int playerID, ResourceAmount amount, Vector3 worldPos)
    {
        PlayerResourceAmounts[playerID].AddResources(amount);

        ResourceUiFloating resourcesInstance = Instantiate(giveResourcesUiPrefab, playerCameras[playerID].WorldToScreenPoint(worldPos),
            Quaternion.identity, playerCanvases[playerID].transform).
            GetComponent<ResourceUiFloating>();

        resourcesInstance.SetAmount(amount);

        Destroy(resourcesInstance.gameObject, 5f);
    }

    private void HandleCheatCode ()
    {
        for (int i = 0; i < 4; i++)
        {
            if (usedCheatCode[i])
                continue;

            if (PlayerInput.GetPlayers[i].GetButton(PlayerInput.GetInputDpadUp))
            {
                if (PlayerInput.GetPlayers[i].GetButton(PlayerInput.GetInputDpadRight))
                {
                    if (PlayerInput.GetPlayers[i].GetButton(PlayerInput.GetInputSelect))
                    {
                        if (PlayerInput.GetPlayers[i].GetButton(PlayerInput.GetInputOpenBuildMenu))
                        {
                            if (PlayerInput.GetPlayers[i].GetButtonDown(PlayerInput.GetInputOpenUnitMenu))
                            {
                                PlayerResourceAmounts[i].AddResources(1000, 1000, 1000);
                                usedCheatCode[i] = true;
                                return;
                            }
                        }

                    }
                }
            }
        }
    }
}
