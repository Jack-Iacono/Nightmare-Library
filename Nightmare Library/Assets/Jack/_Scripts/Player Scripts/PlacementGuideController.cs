using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementGuideController : MonoBehaviour
{
    [NonSerialized]
    public Transform trans;

    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private MeshRenderer meshRend;

    [SerializeField]
    private Material clearPlaceMaterial;
    [SerializeField]
    private Material blockedPlaceMaterial;

    // Start is called before the first frame update
    void Start()
    {
        trans = transform;

        gameObject.SetActive(false);
    }

    public void SetMeshFilter(MeshFilter meshFilter)
    {
        Debug.Log("Setting Mesh Filter");
        this.meshFilter.mesh = meshFilter.mesh;
        this.meshFilter.transform.localScale = meshFilter.transform.localScale;
    }

    /// <summary>
    /// Changes the material of this object to reflect the placement availability
    /// </summary>
    /// <param name="clear">Whether the placement is clear or not</param>
    public void SetMaterial(bool clear)
    {
        if(clear)
            meshRend.material = clearPlaceMaterial;
        else
            meshRend.material = blockedPlaceMaterial;
    }
    public void SetTransform(Transform newTransform)
    {
        trans.position = newTransform.position;
        trans.rotation = newTransform.rotation;
    }

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
