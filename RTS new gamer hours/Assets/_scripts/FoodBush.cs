using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodBush : MonoBehaviour
{
    [SerializeField] private ResourceNode node;
    [SerializeField] private GameObject[] food;

    private bool foodEnabled = true;
    public bool GetFoodEnabled => node.GetHasResources;

    private void Update()
    {
        if (node.GetHasResources && !foodEnabled)
            EnableFood();
    }

    private void EnableFood ()
    {
        foodEnabled = true;
        for (int i = 0; i < food.Length; i++)
        {
            food[i].SetActive(true);
        }
    }

    // used in tree shake onHit
    public void HitBush ()
    {
        if (!foodEnabled)
            return;

        foodEnabled = false;
        for (int i = 0; i < food.Length; i++)
        {
            food[i].SetActive(false);
        }
    }
}
