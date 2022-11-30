using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeLayerController : MonoBehaviour
{
    /// <summary>
    /// returns -1 if they playerID is not a player
    /// </summary>
    public static int GetLayer (int playerID)
    {
        if (playerID == 0)
        {
            return LayerMask.NameToLayer("P1 CAM");
        }
        else if (playerID == 1)
        {
            return LayerMask.NameToLayer("P2 CAM");
        }
        else if (playerID == 2)
        {
            return LayerMask.NameToLayer("P3 CAM");
        }
        else if (playerID == 3)
        {
            return LayerMask.NameToLayer("P4 CAM");
        }
        else
        {
            return -1;
        }
    }
}
