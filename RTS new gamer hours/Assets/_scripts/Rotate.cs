using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private bool aroundParent = true;
    [SerializeField] private bool unscaledTime = false;

    private void Update()
    {
        Vector3 pos = aroundParent ? transform.parent.position : transform.position;
        transform.RotateAround(pos, aroundParent ? transform.parent.right : transform.forward, rotateSpeed * (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime));
    }
}
