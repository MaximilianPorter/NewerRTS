using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSurrenderController : MonoBehaviour
{
    [SerializeField] private Image surrenderFill;

    private Identifier identifier;
    private float surrenderCounter = 0f;
    private float timeToSurrender = 5f;

    private void Start()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Update()
    {
        if (!SurrenderController.Instance)
        {
            surrenderFill.gameObject.SetActive(false);
            return;
        }

        if (SurrenderController.Instance.GetPlayerSurrendered (identifier.GetPlayerID))
        {
            surrenderFill.gameObject.SetActive(false);
            return;
        }

        surrenderFill.gameObject.SetActive(surrenderCounter > 2);
        surrenderFill.fillAmount = surrenderCounter / timeToSurrender;

        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButton(PlayerInput.GetInputBack))
        {
            surrenderCounter += Time.deltaTime;

        }
        else
            surrenderCounter = 0f;

        if (surrenderCounter > timeToSurrender && SurrenderController.Instance.GetPlayerSurrendered(identifier.GetPlayerID) == false)
        {
            SurrenderController.Instance.SetPlayerSurrendered(identifier.GetPlayerID);
        }
    }
}
