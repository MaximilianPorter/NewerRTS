using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ErrorMessageHandler : MonoBehaviour
{
    private static ErrorMessage[] errorMessages;
    public static void SetErrorMessage(int playerID, string message) => errorMessages[playerID].SetErrorMessage(message);


    private void Start()
    {
        errorMessages = FindObjectsOfType<ErrorMessage>(true).OrderBy(messageObject => messageObject.GetComponent<Identifier>().GetPlayerID).ToArray();
    }
}
