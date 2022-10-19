using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void ChangeScene (int sceneIndex)
    {
        if (sceneIndex == SceneManager.GetActiveScene().buildIndex)
            return;

        SceneManager.LoadScene(sceneIndex);
    }
}
