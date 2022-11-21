using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuDescriptionArea : MonoBehaviour
{
    [SerializeField] private HorizontalLayoutGroup descriptionArea;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private HorizontalLayoutGroup detailedDescriptionArea;
    [SerializeField] private TMP_Text detailedDescriptionText;
    [SerializeField] private PlayerBuilding playerBuilding;


    private void Update()
    {

        if (playerBuilding.GetHasMenuOpen)
        {
            if (playerBuilding.GetSelectedUiButton != null)
            {
                descriptionText.text = "<b>" + playerBuilding.GetSelectedUiButton.GetButtonName.ToUpper() + "</b> \n" +
                    playerBuilding.GetSelectedUiButton.GetButtonDescription;

                // more descriptive details about unit stats
                if (playerBuilding.GetSelectedUiButton.GetUnitStats)
                {
                    detailedDescriptionArea.enabled = false;
                    detailedDescriptionArea.enabled = true;
                    detailedDescriptionArea.gameObject.SetActive(true);
                    detailedDescriptionText.text = 
                        StatDescription ("HEALTH", playerBuilding.GetSelectedUiButton.GetUnitStats.health.ToString()) + "\n" +
                        StatDescription("ARMOR", playerBuilding.GetSelectedUiButton.GetUnitStats.armor.ToString()) + "\n" +
                        StatDescription("DAMAGE", playerBuilding.GetSelectedUiButton.GetUnitStats.damage.ToString()) + "\n" +
                        StatDescription("ATTACK TIME", playerBuilding.GetSelectedUiButton.GetUnitStats.timeBetweenAttacks.ToString()) + "\n" +
                        StatDescription("LOOK RANGE", playerBuilding.GetSelectedUiButton.GetUnitStats.lookRange.ToString()) + "\n" +
                        StatDescription("ATTACK RANGE", playerBuilding.GetSelectedUiButton.GetUnitStats.attackRange.ToString());
                }
                else
                {
                    detailedDescriptionArea.gameObject.SetActive(false);
                }
            }

            // turned off and turned on because the ContentSizeFitter doesn't update enough
            descriptionArea.enabled = false;
            descriptionArea.enabled = true;

            descriptionArea.gameObject.SetActive(true);
        }
        else
        {
            descriptionArea.gameObject.SetActive(false);
            detailedDescriptionArea.gameObject.SetActive(false);
        }

    }

    private string StatDescription (string header, string stat)
    {
        return header + ": <b>" + stat + "</b>";
    }
}
