using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomlyTurnOn : MonoBehaviour
{
    [SerializeField] private Vector2Int amountToTurnOn = new Vector2Int(1, 3);
    [SerializeField] private GameObject[] randomObjects;

    [SerializeField] private bool ifOffTurnBackOn = true;
    [SerializeField] private float turnBackOnTime = 15f;

    private float[] counters;
    private int[] randomIndexes;

    private void Awake()
    {
        int randAmt = Random.Range(amountToTurnOn.x, amountToTurnOn.y + 1);
        randomIndexes = new int[randAmt];
        counters = new float[randAmt];

        // initialize random indexes with -1
        for (int i = 0; i < randomIndexes.Length; i++)
        {
            randomIndexes[i] = -1;
        }

        // foreach random index, choose a number from 0 - randomObjects.len
        for (int i = 0; i < randomIndexes.Length; i++)
        {
            if (randomObjects.Length <= 1)
            {
                randomIndexes[0] = 0;
                break;
            }
            int randIndex;
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

        for (int i = 0; i < counters.Length; i++)
        {
            counters[i] = turnBackOnTime;
        }
    }

    private void Update()
    {
        if (!ifOffTurnBackOn)
            return;


        for (int i = 0; i < randomIndexes.Length; i++)
        {
            if (!randomObjects[randomIndexes[i]].activeInHierarchy)
            {
                counters[i] -= Time.deltaTime;

                if (counters[i] < 0)
                {
                    randomObjects[randomIndexes[i]].SetActive(true);
                    counters[i] = turnBackOnTime;
                }
            }
        }
    }
}
