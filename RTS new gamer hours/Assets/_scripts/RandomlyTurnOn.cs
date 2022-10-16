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

    private void Start()
    {
        int randAmt = Random.Range(amountToTurnOn.x, amountToTurnOn.y);
        randomIndexes = new int[randAmt];
        counters = new float[randAmt];
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
