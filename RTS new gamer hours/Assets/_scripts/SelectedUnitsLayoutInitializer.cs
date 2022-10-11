using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedUnitsLayoutInitializer : MonoBehaviour
{
    [SerializeField] private Transform queuedUpUnitsLayout;

    private void Awake()
    {
        QueuedUpUnitUi[] children = queuedUpUnitsLayout.GetComponentsInChildren<QueuedUpUnitUi>();
        for (int i = 0; i < children.Length; i++)
        {
            GameObject duplicateChildInstance = Instantiate(children[i].gameObject, transform);
        }
    }
}
