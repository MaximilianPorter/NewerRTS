using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof (Identifier))]
public class PlayerResourceHolder : MonoBehaviour
{
    [SerializeField] private TMP_Text populationText;
    [SerializeField] private TMP_Text foodText;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text stoneText;

    [Space(10)]
    [SerializeField] private Color addColor = Color.white;
    [SerializeField] private Color subColor = Color.red;
    [SerializeField] private Transform changeResourcesLayout;
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private AnimationCurve fadeCurve;
    private TMP_Text[] changeText;
    private Image[] changeImages;
    private float[] fadeCounter = { 0, 0, 0, 0 };
    private int lastPopulationCap = 0;
    private ResourceAmount lastResources = new ResourceAmount();

    private Identifier identifier;

    private void Start()
    {
        identifier = GetComponent<Identifier>();

        changeText = changeResourcesLayout.GetComponentsInChildren<TMP_Text>();
        changeImages = changeResourcesLayout.GetComponentsInChildren<Image>();

        ChangeResources(1, 1, 1, 1); // this is here so the 500/500 for population goes away as well as the others when switching scenes
    }

    private void Update()
    {
        populationText.text = PlayerHolder.GetUnits(identifier.GetPlayerID).Count.ToString() + "/" +
            PlayerResourceManager.PopulationCap[identifier.GetPlayerID].ToString();

        foodText.text = PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetFood.ToString();

        woodText.text = PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetWood.ToString();

        stoneText.text = PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetStone.ToString();



        // change last resources if it's differnt
        if (!PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].CompareResources(lastResources))
        {
            ChangeResources(0, PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetFood - lastResources.GetFood,
                PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetWood - lastResources.GetWood,
                PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetStone - lastResources.GetStone);

            lastResources.SetResources(PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID]);
        }

        if (lastPopulationCap != PlayerResourceManager.PopulationCap[identifier.GetPlayerID])
        {
            ChangeResources(PlayerResourceManager.PopulationCap[identifier.GetPlayerID] - lastPopulationCap, 0, 0, 0);

            lastPopulationCap = PlayerResourceManager.PopulationCap[identifier.GetPlayerID];
        }



        for (int i = 0; i < 4; i++)
        {
            fadeCounter[i] = Mathf.Clamp(fadeCounter[i] - Time.deltaTime, 0f, Mathf.Infinity);
            if (fadeCounter[i] >= 0)
                FadeColors(i);
        }
    }

    private void FadeColors (int index)
    {
        Color textColor = changeText[index].color;
        changeText[index].color = new Color(textColor.r, textColor.g, textColor.b, Mathf.Lerp (0f, 1f, fadeCurve.Evaluate(fadeCounter[index]) / fadeTime));


        Color imageColor = changeImages[index].color;
        changeImages[index].color = new Color(imageColor.r, imageColor.g, imageColor.b, Mathf.Lerp(0f, 1f, fadeCurve.Evaluate (fadeCounter[index]) / fadeTime));
    }

    public void ChangeResources(int populationChange, int food, int wood, int stone)
    {
        // turn on correct containers
        TurnShitOn(populationChange, 0);
        TurnShitOn(food, 1);
        TurnShitOn(wood, 2);
        TurnShitOn(stone, 3);
    }

    private void TurnShitOn (int changeAmount, int index)
    {
        if (changeAmount != 0)
        {
            fadeCounter[index] = fadeTime;

            changeImages[index].color = new Color(1, 1, 1, 1);
            //changeText[index].transform.parent.gameObject.SetActive(true);

            changeText[index].text = (changeAmount > 0 ? "+" : "") + changeAmount.ToString();
            changeText[index].color = changeAmount > 0 ? addColor : subColor;
        }
    }
}
