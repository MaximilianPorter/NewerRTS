using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Identifier))]
public class ErrorMessage : MonoBehaviour
{
    private float errorOnScreenTimer = 3f;
    private float errorOnScreenCounter = 0f;
    private TMP_Text errorText;

    private void Awake()
    {
        errorText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        // turn on/off text
        errorText.enabled = errorOnScreenCounter > 0f;

        // count down
        errorOnScreenCounter = Mathf.Clamp(errorOnScreenCounter - Time.deltaTime, 0f, errorOnScreenTimer);

        // fade alpha
        errorText.color = new Color(errorText.color.r, errorText.color.g, errorText.color.b, Mathf.Lerp(0f, 1f, errorOnScreenCounter / errorOnScreenTimer));
    }

    /// <summary>
    /// throws the error message (.ToUpper()) up on the players screen for 3 seconds
    /// </summary>
    /// <param name="message"></param>
    public void SetErrorMessage (string message)
    {
        errorOnScreenCounter = errorOnScreenTimer;

        string prefix = "<font=\"SourceSansPro-BlackItalic ERROR MESSAGE\"><mark=#BE1818>";
        string suffix = "</mark>";

        errorText.text = prefix + message.ToUpper() + suffix;
    }
}
