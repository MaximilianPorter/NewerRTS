using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;

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
    //[SerializeField] private float fadeTime = 1f;
    //[SerializeField] private AnimationCurve fadeCurve;

    private TMP_Text[] changeText;
    private Image[] changeImages;
    //private float[] fadeCounter = { 0, 0, 0, 0 };
    //private int lastPopulationCap = 0;
    private ResourceAmount lastResources = new ResourceAmount();
    private List<float> foodCounterAndValue = new List<float>();
    private List<float> woodCounterAndValue = new List<float>();
    private List<float> stoneCounterAndValue = new List<float>();

    private float checkCounter = 0f;
    private readonly float timeToCheck = 0.2f;

    private Identifier identifier;

    private void Start()
    {
        identifier = GetComponent<Identifier>();

        changeText = changeResourcesLayout.GetComponentsInChildren<TMP_Text>();
        changeImages = changeResourcesLayout.GetComponentsInChildren<Image>();


        // turn off population change, I don't care about it but I don't want to get rid of it
        changeText[0].gameObject.SetActive(false);
        changeImages[0].gameObject.SetActive(false);

        for (int i = 0; i < Mathf.RoundToInt((60f / timeToCheck)); i++)
        {
            foodCounterAndValue.Add(0f);
        }
        for (int i = 0; i < Mathf.RoundToInt((60f / timeToCheck)); i++)
        {
            woodCounterAndValue.Add(0f);
        }
        for (int i = 0; i < Mathf.RoundToInt((60f / timeToCheck)); i++)
        {
            stoneCounterAndValue.Add(0f);
        }

        //ChangeResources(1, 1, 1, 1); // this is here so the 500/500 for population goes away as well as the others when switching scenes
    }

    private void Update()
    {
        DisplayResources();

        int playerFood = PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetFood;
        int playerWood = PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetWood;
        int playerStone = PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetStone;

        float foodChange = (foodCounterAndValue.Count > 0 ? foodCounterAndValue.Sum() / foodCounterAndValue.Count : 0f) * (60 / timeToCheck);
        changeText[1].text = foodChange.ToString("F0") + "<sub>/min</sub>";
        //changeText[1].color = foodChange >= 0 ? addColor : subColor;

        float woodChange = (woodCounterAndValue.Count > 0 ? woodCounterAndValue.Sum() / woodCounterAndValue.Count : 0f) * (60 / timeToCheck);
        changeText[2].text = woodChange.ToString("F0") + "<sub>/min</sub>";
        //changeText[2].color = woodChange >= 0 ? addColor : subColor;

        float stoneChange = (stoneCounterAndValue.Count > 0 ? stoneCounterAndValue.Sum() / stoneCounterAndValue.Count : 0f) * (60 / timeToCheck);
        changeText[3].text = stoneChange.ToString("F0") + "<sub>/min</sub>";
        //changeText[3].color = stoneChange >= 0 ? addColor : subColor;



        if (foodCounterAndValue.Count > (60f / timeToCheck))
            foodCounterAndValue.RemoveAt(Mathf.FloorToInt((60f / timeToCheck)) - 1);
        if (woodCounterAndValue.Count > (60f / timeToCheck))
            woodCounterAndValue.RemoveAt(Mathf.FloorToInt((60f / timeToCheck)) - 1);
        if (stoneCounterAndValue.Count > (60f / timeToCheck))
            stoneCounterAndValue.RemoveAt(Mathf.FloorToInt((60f / timeToCheck)) - 1);

        //for (int i = 0; i < foodCounterAndValue.Count; i++)
        //{
        //    foodCounterAndValue[i] = new Vector2(foodCounterAndValue[i].x + Time.deltaTime, foodCounterAndValue[i].y);
        //}
        //for (int i = 0; i < woodCounterAndValue.Count; i++)
        //{
        //    woodCounterAndValue[i] = new Vector2(woodCounterAndValue[i].x + Time.deltaTime, woodCounterAndValue[i].y);
        //}
        //for (int i = 0; i < stoneCounterAndValue.Count; i++)
        //{
        //    stoneCounterAndValue[i] = new Vector2(stoneCounterAndValue[i].x + Time.deltaTime, stoneCounterAndValue[i].y);
        //}



        //foodCounterAndValue.RemoveAll(counter => counter.x > 60f);
        //woodCounterAndValue.RemoveAll(counter => counter.x > 60f);
        //stoneCounterAndValue.RemoveAll(counter => counter.x > 60f);

        checkCounter += Time.deltaTime;

        if (checkCounter > timeToCheck)
        {
            checkCounter = 0f;

            foodCounterAndValue.Insert(0, Mathf.Clamp (playerFood - lastResources.GetFood, 0f, Mathf.Infinity));
            lastResources.SetFood(playerFood);

            woodCounterAndValue.Insert(0, Mathf.Clamp(playerWood - lastResources.GetWood, 0f, Mathf.Infinity));
            lastResources.SetWood(playerWood);

            stoneCounterAndValue.Insert(0, Mathf.Clamp(playerStone - lastResources.GetStone, 0f, Mathf.Infinity));
            lastResources.SetStone(playerStone);
        }


        //// change last resources if it's differnt
        //if (!PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].CompareResources(lastResources))
        //{
        //ChangeResources(0, PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetFood - lastResources.GetFood,
        //    PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetWood - lastResources.GetWood,
        //    PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetStone - lastResources.GetStone);

        //lastResources.SetResources(PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID]);
        //}

        //if (lastPopulationCap != PlayerResourceManager.PopulationCap[identifier.GetPlayerID])
        //{
        //    ChangeResources(PlayerResourceManager.PopulationCap[identifier.GetPlayerID] - lastPopulationCap, 0, 0, 0);

        //    lastPopulationCap = PlayerResourceManager.PopulationCap[identifier.GetPlayerID];
        //}



        //for (int i = 0; i < 4; i++)
        //{
        //    fadeCounter[i] = Mathf.Clamp(fadeCounter[i] - Time.deltaTime, 0f, Mathf.Infinity);
        //    if (fadeCounter[i] >= 0)
        //        FadeColors(i);
        //}
    }

    //private void FadeColors (int index)
    //{
    //    Color textColor = changeText[index].color;
    //    changeText[index].color = new Color(textColor.r, textColor.g, textColor.b, Mathf.Lerp (0f, 1f, fadeCurve.Evaluate(fadeCounter[index]) / fadeTime));


    //    Color imageColor = changeImages[index].color;
    //    changeImages[index].color = new Color(imageColor.r, imageColor.g, imageColor.b, Mathf.Lerp(0f, 1f, fadeCurve.Evaluate (fadeCounter[index]) / fadeTime));
    //}

    public void ChangeResources(int populationChange, int food, int wood, int stone)
    {
        // turn on correct containers
        //TurnShitOn(populationChange, 0);
        TurnShitOn(food, 1);
        TurnShitOn(wood, 2);
        TurnShitOn(stone, 3);
    }

    private void TurnShitOn (int changeAmount, int index)
    {
        if (changeAmount != 0)
        {
            //fadeCounter[index] = fadeTime;

            //changeImages[index].color = new Color(1, 1, 1, 1);
            //changeText[index].transform.parent.gameObject.SetActive(true);

            changeText[index].text = (changeAmount > 0 ? "+" : "") + changeAmount.ToString();
            changeText[index].color = changeAmount > 0 ? addColor : subColor;
        }
    }

    private void DisplayResources ()
    {
        populationText.text = PlayerHolder.GetUnits(identifier.GetPlayerID).Count.ToString() + "/" +
            PlayerResourceManager.PopulationCap[identifier.GetPlayerID].ToString();

        foodText.text = PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetFood.ToString();

        woodText.text = PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetWood.ToString();

        stoneText.text = PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].GetStone.ToString();
    }
}
