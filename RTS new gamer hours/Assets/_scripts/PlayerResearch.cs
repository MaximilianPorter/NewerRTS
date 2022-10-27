using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof (Identifier))]
public class PlayerResearch : MonoBehaviour
{
    [SerializeField] private Transform researchLayoutGroup;

    private Identifier identifier;
    private ResearchUi[] researchIcons;
    private ResearchUi GetCurrentResearch => PlayerHolder.GetCurrentResearch(identifier.GetPlayerID); // from PlayerHolder.cs

    private void Start()
    {
        identifier = GetComponent<Identifier>();
        researchIcons = researchLayoutGroup.GetComponentsInChildren<ResearchUi>();

        // turn off all the research icons
        for (int i = 0; i < researchIcons.Length; i++)
        {
            researchIcons[i].gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (GetCurrentResearch && GetCurrentResearch.GetIsFinished)
        {
            if (!PlayerHolder.GetCompletedResearch(identifier.GetPlayerID).Contains(GetCurrentResearch.GetStats.researchType)) // it shouldn't contain it, but this is just for precausion
                CompleteResearch();
        }
    }

    public void SetCurrentResearch (BuyIconUI researchButtonClicked)
    {
        //if (currentResearch.GetStats.researchType == researchType && !currentResearch.GetIsFinished) // clicked the same button, cancel research
        //    CancelResearch();
        //else 

        if (GetCurrentResearch != null) // we're researching something else
            return;

        ResearchUi matchingResearch = researchIcons.FirstOrDefault(icon => icon.GetStats.researchType == researchButtonClicked.GetButtonType);

        // turn on the correct icon and start researching
        matchingResearch.gameObject.SetActive(true);
        matchingResearch.transform.SetAsFirstSibling();
        PlayerHolder.SetCurrentResearch(identifier.GetPlayerID, matchingResearch);
        GetCurrentResearch.StartResearch();
    }

    private void CancelResearch()
    {
        GetCurrentResearch.StopResearch();
        GetCurrentResearch.gameObject.SetActive(false);
        PlayerHolder.SetCurrentResearch(identifier.GetPlayerID, null);
    }

    private void CompleteResearch ()
    {
        PlayerHolder.AddCompletedResearch(identifier.GetPlayerID, GetCurrentResearch.GetStats.researchType);
        PlayerHolder.SetCurrentResearch(identifier.GetPlayerID, null);
    }
}
