using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static void ChangeScene (int sceneIndex)
    {
        if (sceneIndex == SceneManager.GetActiveScene().buildIndex)
            return;

        for (int i = 0; i < 4; i++)
        {
            if (PlayerHolder.instance != null)
            {
                PlayerHolder.GetCompletedResearch(i).Clear();
                PlayerHolder.SetCurrentResearch(i, null);
            }
        }

        SceneManager.LoadScene(sceneIndex);
    }
}
