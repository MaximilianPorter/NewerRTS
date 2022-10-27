using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{
    [SerializeField] private Gradient healthGradient;

    [SerializeField] private Image healthBar;

    private List<List<Image>> unitHealthBarInstances = new List<List<Image>>();
    private List<List<Image>> buildingHealthBarInstances = new List<List<Image>>();

    private static bool[] healthBarsOn = new bool[4] { false, false, false, false };
    public static bool[] GetHealthBarsOn => healthBarsOn;
    public static void SetHealthBarsOn(int playerID, bool isOn) => healthBarsOn[playerID] = isOn;

    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            unitHealthBarInstances.Add(new List<Image>());
            buildingHealthBarInstances.Add(new List<Image>());
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < 4; i++)
        {
            // skip if the player has their health bars turned off
            if (!GetHealthBarsOn[i])
            {
                // turn off all the health bars
                for (int j = 0; j < unitHealthBarInstances[i].Count; j++)
                {
                    unitHealthBarInstances[i][j].gameObject.SetActive(false);
                }
                for (int j = 0; j < buildingHealthBarInstances[i].Count; j++)
                {
                    buildingHealthBarInstances[i][j].gameObject.SetActive(false);
                }
                continue;
            }


            // manage units health bars
            if (unitHealthBarInstances[i].Count < PlayerHolder.GetUnits(i).Count)
            {
                Image newHealthBarInstance = Instantiate(healthBar, PlayerHolder.GetPlayerCanvasRects[i].GetChild (0)).GetComponent<Image>();
                unitHealthBarInstances[i].Add(newHealthBarInstance);
            }

            for (int j = 0; j < unitHealthBarInstances[i].Count; j++)
            {
                // the unit has been destroyed, destroy and remove the health bar
                if (j >= PlayerHolder.GetUnits(i).Count || PlayerHolder.GetUnits(i)[j] == null)
                {
                    Destroy(unitHealthBarInstances[i][j].gameObject);
                    unitHealthBarInstances[i].RemoveAt(j);
                    break;
                }

                if (!PlayerHolder.GetUnits(i)[j].GetIsSelected)
                {
                    unitHealthBarInstances[i][j].gameObject.SetActive(false);
                    continue;
                }

                Vector2? localPos = PlayerHolder.WorldToCanvasLocalPoint(PlayerHolder.GetUnits(i)[j].transform.position + new Vector3(0f, 1f, 0f), i);
                if (localPos == null)
                {
                    unitHealthBarInstances[i][j].gameObject.SetActive(false);
                    continue;
                }
                unitHealthBarInstances[i][j].gameObject.SetActive(true);
                unitHealthBarInstances[i][j].color = healthGradient.Evaluate (PlayerHolder.GetUnits(i)[j].GetHealth.GetCurrentHealth / PlayerHolder.GetUnits(i)[j].GetHealth.GetMaxHealth);
                unitHealthBarInstances[i][j].transform.localPosition = localPos.GetValueOrDefault();
                unitHealthBarInstances[i][j].transform.localScale = Vector3.one * PlayerHolder.ScaleWithScreenOrthoSizeMultiplier(i);
            }




            // manage buildings health bars
            if (buildingHealthBarInstances[i].Count < PlayerHolder.GetBuildings(i).Count)
            {
                Image newHealthBarInstance = Instantiate(healthBar.gameObject, PlayerHolder.GetPlayerCanvasRects[i].GetChild(0)).GetComponent<Image>();
                buildingHealthBarInstances[i].Add(newHealthBarInstance);
            }
            buildingHealthBarInstances[i].RemoveAll(healthBarInstance => healthBarInstance == null);

            for (int j = 0; j < buildingHealthBarInstances[i].Count; j++)
            {
                // the building has been destroyed, destroy and remove the health bar
                if (j >= PlayerHolder.GetBuildings(i).Count || PlayerHolder.GetBuildings(i)[j] == null)
                {
                    Destroy(buildingHealthBarInstances[i][j].gameObject);
                    buildingHealthBarInstances[i].RemoveAt(j);
                    break;
                }

                Vector2? localPos = PlayerHolder.WorldToCanvasLocalPoint(PlayerHolder.GetBuildings(i)[j].transform.position + new Vector3(0f, 1f, 0f), i);

                if (localPos == null)
                {
                    buildingHealthBarInstances[i][j].gameObject.SetActive(false);
                    continue;
                }

                buildingHealthBarInstances[i][j].gameObject.SetActive(true);
                buildingHealthBarInstances[i][j].color = healthGradient.Evaluate (PlayerHolder.GetBuildings(i)[j].GetHealth.GetCurrentHealth / PlayerHolder.GetBuildings(i)[j].GetHealth.GetMaxHealth);
                buildingHealthBarInstances[i][j].transform.localPosition = localPos.GetValueOrDefault();
                buildingHealthBarInstances[i][j].transform.localScale = Vector3.one * PlayerHolder.ScaleWithScreenOrthoSizeMultiplier(i);

            }
        }
    }
}
