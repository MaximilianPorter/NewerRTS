using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomlyTurnOn : MonoBehaviour
{
    [SerializeField] private int amountToTurnOn = 1;
    [SerializeField] private GameObject[] randomObjects;

    private int[] randomIndexes;

    private void Start()
    {
        randomIndexes = new int[amountToTurnOn];
        for (int i = 0; i < randomIndexes.Length; i++)
        {
            int randIndex = Random.Range(0, randomObjects.Length);
            do
            {
                randIndex = Random.Range(0, randomObjects.Length);

            } while (randomIndexes.Contains(randIndex));
            randomIndexes[i] = randIndex;
        }

        for (int i = 0; i < randomObjects.Length; i++)
        {
            if (randomIndexes.Contains(i))
                randomObjects[i].SetActive(true);
            else
                randomObjects[i].SetActive(false);
        }
    }
}
