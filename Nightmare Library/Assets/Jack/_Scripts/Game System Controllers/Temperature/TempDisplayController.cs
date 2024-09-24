using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TempDisplayController : MonoBehaviour
{
    private static List<TempDisplayController> controllers = new List<TempDisplayController>();
    public TMP_Text displayText;

    public MeshRenderer stateRenderer;
    public Material raiseMaterial;
    public Material lowerMaterial;
    public Material holdMaterial;

    private void Start()
    {
        controllers.Add(this);
        if (controllers[0] == this)
        {
            TempController.OnTempChanged += OnTempChanged;
            TempController.OnTempStateChanged += OnTempStateChanged;
        }
            
    }

    private void OnTempChanged(int temp)
    {
        displayText.text = temp.ToString();
    }
    private void OnTempStateChanged(int state)
    {
        switch (state)
        {
            case 0:
                stateRenderer.material = holdMaterial;
                break;
            case 1:
                stateRenderer.material = raiseMaterial;
                break;
            case 2:
                stateRenderer.material = lowerMaterial;
                break;
        }
    }

    private void OnDestroy()
    {
        controllers.Clear();
    }
}
