using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RenderBoundsLines : MonoBehaviour
{
    [SerializeField] private Transform home;
    [SerializeField] private LineRenderer lineRenderer;
    //[SerializeField] private SphereCollider col;
    [SerializeField] private float radius = 10f;
    [SerializeField] private LayerMask mask;
    RaycastHit[] hits;
    Collider[] colliders;


    void Start()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        transform.gameObject.SetActive(true);


    }

    private void FixedUpdate()
    {
        //hits = Physics.SphereCastAll(transform.position, radius, Vector3.one, 0f, mask);
        //Physics.OverlapSphereNonAlloc(transform.position, radius, colliders);
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, radius);
        //if (!Application.isPlaying)
        //    return;

        //foreach (RaycastHit hit in hits)
        //{
        //    Gizmos.DrawSphere(hit.point, 1f);

        //}
    }
}
