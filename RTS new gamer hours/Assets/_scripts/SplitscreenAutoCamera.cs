using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SplitscreenAutoCamera : MonoBehaviour
{
    public static SplitscreenAutoCamera instance;

    [SerializeField][Range(1, 4)] private int playersJoined = 4;
    [SerializeField] private GameObject[] playerParents;
    [SerializeField] private RectTransform vertLine;
    [SerializeField] private RectTransform horLine;

    private Camera[] playerCameras;
    private Rect[] playerCameraRects;

    private void Awake()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(transform.gameObject);
    }

    private void Start()
    {
        // if there is already an instance of this, take it's player count and destroy this
        if (instance != null)
        {
            this.playersJoined = instance.playersJoined;
            Destroy(instance.gameObject);
            instance = this;
        }
        else
            instance = this;

        // find cameras
        playerCameras = Camera.allCameras.Where(cam => cam.GetComponent<Identifier>()).OrderBy(cam => cam.GetComponent<Identifier>().GetPlayerID).ToArray();

        // find rects in cameras
        playerCameraRects = new Rect[4];
        for (int i = 0; i < playerCameras.Length; i++)
        {
            playerCameraRects[i] = playerCameras[i].rect;
        }

    }

    private void Update()
    {
        // set rect values
        SetRectValues();

        for (int i = 0; i < playerParents.Length; i++)
        {
            playerParents[i].SetActive(playersJoined > i);
        }
    }

    private void SetRectValues ()
    {
        vertLine.sizeDelta = new Vector2(20f, vertLine.sizeDelta.y);
        horLine.sizeDelta = new Vector2(horLine.sizeDelta.x, 20f);
        if (playersJoined == 1)
        {
            playerCameraRects[0].x = 0f;
            playerCameraRects[0].y = 0f;

            horLine.gameObject.SetActive(false);
            vertLine.gameObject.SetActive(false);
        }
        else if (playersJoined == 2)
        {
            playerCameraRects[0].x = -0.5f;
            playerCameraRects[0].y = 0;// .5f;

            playerCameraRects[1].x = 0.5f;
            playerCameraRects[1].y = 0;// -0.5f;

            horLine.gameObject.SetActive(false);
            vertLine.gameObject.SetActive(true);

            vertLine.localPosition = Vector3.zero;
            horLine.localPosition = Vector3.zero;
            vertLine.sizeDelta = new Vector2(vertLine.sizeDelta.x, Screen.height);
        }
        else if (playersJoined == 3)
        {
            playerCameraRects[0].x = -0.5f;
            playerCameraRects[0].y = 0.5f;

            playerCameraRects[1].x = 0.5f;
            playerCameraRects[1].y = 0.5f;

            playerCameraRects[2].x = 0f;
            playerCameraRects[2].y = -0.5f;

            vertLine.localPosition = new Vector3(0f, Screen.height * 0.25f, 0f);
            horLine.localPosition = Vector3.zero;
            horLine.gameObject.SetActive(true);
            vertLine.gameObject.SetActive(true);
            vertLine.sizeDelta = new Vector2(vertLine.sizeDelta.x, Screen.height / 2f);
            horLine.sizeDelta = new Vector2(Screen.width, horLine.sizeDelta.y);
        }
        else if (playersJoined == 4)
        {
            playerCameraRects[0].x = -0.5f;
            playerCameraRects[0].y = 0.5f;

            playerCameraRects[1].x = 0.5f;
            playerCameraRects[1].y = 0.5f;

            playerCameraRects[2].x = -0.5f;
            playerCameraRects[2].y = -0.5f;

            playerCameraRects[3].x = 0.5f;
            playerCameraRects[3].y = -0.5f;

            vertLine.localPosition = Vector3.zero;
            horLine.localPosition = Vector3.zero;
            horLine.gameObject.SetActive(true);
            vertLine.gameObject.SetActive(true);
            vertLine.sizeDelta = new Vector2(vertLine.sizeDelta.x, Screen.height);
            horLine.sizeDelta = new Vector2(Screen.width, horLine.sizeDelta.y);
        }


        // set camera rects
        for (int i = 0; i < playerCameraRects.Length; i++)
        {
            playerCameras[i].rect = playerCameraRects[i];
        }

    }
}
