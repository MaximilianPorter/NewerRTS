using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuDescriptionArea : MonoBehaviour
{
    [SerializeField] private HorizontalLayoutGroup descriptionArea;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private PlayerBuilding playerBuilding;


    private void Update()
    {

        if (playerBuilding.GetHasMenuOpen)
        {
            if (playerBuilding.GetSelectedUiButton != null)
            {
                descriptionText.text = "<b>" + playerBuilding.GetSelectedUiButton.GetButtonName.ToUpper() + ": </b> \n" +
                    playerBuilding.GetSelectedUiButton.GetButtonDescription;
            }

            // turned off and turned on because the ContentSizeFitter doesn't update enough
            descriptionArea.enabled = false;
            descriptionArea.enabled = true;

            descriptionArea.gameObject.SetActive(true);
        }
        else
        {
            descriptionArea.gameObject.SetActive(false);
        }

    }
}
