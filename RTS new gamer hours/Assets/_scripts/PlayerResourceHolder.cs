using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof (Identifier))]
public class PlayerResourceHolder : MonoBehaviour
{
    [SerializeField] private TMP_Text populationText;
    [SerializeField] private TMP_Text foodText;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text stoneText;

    private Identifier identifier;

    private void Start()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Update()
    {
        populationText.text = PlayerHolder.GetUnits(identifier.GetPlayerID).Count.ToString() + "/" +
            PlayerResourceManager.PopulationCap[identifier.GetPlayerID].ToString();

        foodText.text = PlayerResourceManager.Food[identifier.GetPlayerID].ToString();

        woodText.text = PlayerResourceManager.Wood[identifier.GetPlayerID].ToString();

        stoneText.text = PlayerResourceManager.Stone[identifier.GetPlayerID].ToString();

    }
}
