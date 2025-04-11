using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicLightController : MonoBehaviour, IElectronic
{
    private bool isInterfering = false;
    public event IElectronic.OnInterfereDelegate OnInterfere;

    [SerializeField]
    private Light attachedLight;

    private int minLightFlickerCount = 3;
    private int maxLightFlickerCount = 7;

    private float minLightFlickerDuration = 0.2f;
    private float maxLightFlickerDuration = 1;

    private void Awake()
    {
        IElectronic.instances.Add(gameObject, this);
    }

    public void ElectronicInterfere()
    {
        if (!isInterfering)
            StartCoroutine(FlickerLightCoroutine());
        OnInterfere?.Invoke();
    }

    IEnumerator FlickerLightCoroutine()
    {
        isInterfering = true;

        int flickerAmount = Random.Range(minLightFlickerCount, maxLightFlickerCount);
        for (int i = 0; i < flickerAmount; i++)
        {
            attachedLight.enabled = false;
            yield return new WaitForSeconds(UnityEngine.Random.Range(minLightFlickerDuration, maxLightFlickerDuration));
            attachedLight.enabled = true;
            yield return new WaitForSeconds(UnityEngine.Random.Range(minLightFlickerDuration, maxLightFlickerDuration));
        }

        isInterfering = false;
    }

    private void OnDestroy()
    {
        IElectronic.instances.Remove(gameObject);
    }
}
