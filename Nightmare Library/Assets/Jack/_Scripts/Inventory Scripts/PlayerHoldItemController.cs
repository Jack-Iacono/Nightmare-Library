using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHoldItemController : MonoBehaviour
{
    [SerializeField]
    private MeshFilter filter;
    [SerializeField]
    private MeshRenderer meshRend;

    private void Awake()
    {
        SetVisible(false);
    }

    public void SetVisible(bool b)
    {
        gameObject.SetActive(b);
    }
    public void SetMesh(MeshFilter filter, Material material)
    {
        this.filter.mesh = filter.mesh;
        meshRend.material = material;

        this.filter.transform.localScale = filter.transform.localScale;
    }
}
