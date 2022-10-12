using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 10f;

    private void Update()
    {
        transform.RotateAround(transform.parent.position, transform.right, rotateSpeed * Time.deltaTime);
    }
}
