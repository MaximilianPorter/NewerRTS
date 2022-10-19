using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SplitscreenAutoCamera : MonoBehaviour
{
    public static SplitscreenAutoCamera instance;

    [SerializeField] private int[] playersJoinedOrder = new int[4] { -1, -1, -1, -1 };
    [SerializeField] private GameObject[] playerParents;
    [SerializeField] private RectTransform vertLine;
    [SerializeField] private RectTransform horLine;

    private Camera[] playerCameras;
    private Camera[] playerUiCameras;
    private Rect[] playerCameraRects;

    private int debugSecondPlayerIndex = 0;

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
            this.playersJoinedOrder = instance.playersJoinedOrder;
            Destroy(instance.gameObject);
            instance = this;
        }
        else
            instance = this;


        // find cameras
        playerCameras = Camera.allCameras.Where(cam => cam.GetComponent<Identifier>()).OrderBy(cam => cam.GetComponent<Identifier>().GetPlayerID).ToArray();

        playerUiCameras = new Camera[playerCameras.Length];

        // find rects in cameras
        playerCameraRects = new Rect[4];
        for (int i = 0; i < playerCameras.Length; i++)
        {
            playerUiCameras[i] = playerCameras[i].transform.GetChild(0).GetComponent<Camera>();
            playerCameraRects[i] = playerCameras[i].rect;
        }

    }

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            // if a player clicks "A" and they're not joined yet
            if (PlayerInput.GetPlayers[i].GetButtonDown (PlayerInput.GetInputSelect) && !playersJoinedOrder.Contains(i))
            {
                // assign the first -1 to the character index
                for (int j = 0; j < playersJoinedOrder.Length; j++)
                {
                    if (playersJoinedOrder[j] == -1)
                    {
                        playersJoinedOrder[j] = i;
                        break;
                    }
                }
            }
        }

        // set rect values
        SetRectValues();

        for (int i = 0; i < playerParents.Length; i++)
        {
            playerParents[i].SetActive(playersJoinedOrder.Contains(i));
        }
    }

    private void SetRectValues ()
    {
        vertLine.sizeDelta = new Vector2(20f, vertLine.sizeDelta.y);
        horLine.sizeDelta = new Vector2(horLine.sizeDelta.x, 20f);

        int playersJoinedCount = playersJoinedOrder.Count(player => player >= 0);

        if (playersJoinedCount == 1)
        {
            playerCameraRects[playersJoinedOrder[0]].x = 0f;
            playerCameraRects[playersJoinedOrder[0]].y = 0f;

            horLine.gameObject.SetActive(false);
            vertLine.gameObject.SetActive(false);
        }
        else if (playersJoinedCount == 2)
        {
            playerCameraRects[playersJoinedOrder[0]].x = -0.5f;
            playerCameraRects[playersJoinedOrder[0]].y = 0;// .5f;

            playerCameraRects[playersJoinedOrder[1]].x = 0.5f;
            playerCameraRects[playersJoinedOrder[1]].y = 0;// -0.5f;

            horLine.gameObject.SetActive(false);
            vertLine.gameObject.SetActive(true);

            vertLine.localPosition = Vector3.zero;
            horLine.localPosition = Vector3.zero;
            vertLine.sizeDelta = new Vector2(vertLine.sizeDelta.x, Screen.height);
        }
        else if (playersJoinedCount == 3)
        {
            playerCameraRects[playersJoinedOrder[0]].x = -0.5f;
            playerCameraRects[playersJoinedOrder[0]].y = 0.5f;

            playerCameraRects[playersJoinedOrder[1]].x = 0.5f;
            playerCameraRects[playersJoinedOrder[1]].y = 0.5f;

            playerCameraRects[playersJoinedOrder[2]].x = 0f;
            playerCameraRects[playersJoinedOrder[2]].y = -0.5f;

            vertLine.localPosition = new Vector3(0f, Screen.height * 0.25f, 0f);
            horLine.localPosition = Vector3.zero;
            horLine.gameObject.SetActive(true);
            vertLine.gameObject.SetActive(true);
            vertLine.sizeDelta = new Vector2(vertLine.sizeDelta.x, Screen.height / 2f);
            horLine.sizeDelta = new Vector2(Screen.width, horLine.sizeDelta.y);
        }
        else if (playersJoinedCount == 4)
        {
            playerCameraRects[playersJoinedOrder[0]].x = -0.5f;
            playerCameraRects[playersJoinedOrder[0]].y = 0.5f;

            playerCameraRects[playersJoinedOrder[1]].x = 0.5f;
            playerCameraRects[playersJoinedOrder[1]].y = 0.5f;

            playerCameraRects[playersJoinedOrder[2]].x = -0.5f;
            playerCameraRects[playersJoinedOrder[2]].y = -0.5f;

            playerCameraRects[playersJoinedOrder[3]].x = 0.5f;
            playerCameraRects[playersJoinedOrder[3]].y = -0.5f;

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

            playerUiCameras[i].rect = playerCameraRects[i];
        }

    }
}
