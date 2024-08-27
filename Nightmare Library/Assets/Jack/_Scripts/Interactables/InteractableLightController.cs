using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableLightController : MonoBehaviour
{
    public Light attachedLight;

    int LightFlickerAvg = 5;
    int lightFlickerDev = 2;

    float lightFlickerDurationAvg = 0.2f;
    float lightFlickerDurationDev = 0.19f;

    float lightFlickerCooldownAvg = 1f;
    float lightFlickerCooldownDev = 0.9f;

    private bool isFlickering = false;

    public void FlickerLight()
    {
        if(!isFlickering)
            StartCoroutine(FlickerLightCoroutine());
    }
    IEnumerator FlickerLightCoroutine()
    {
        isFlickering = true;

        int flickerAmount = Random.Range(LightFlickerAvg - lightFlickerDev, LightFlickerAvg + lightFlickerDev);
        for(int i = 0; i < flickerAmount; i++)
        {
            attachedLight.enabled = false;
            yield return new WaitForSeconds(Random.Range(lightFlickerDurationAvg - lightFlickerDurationDev, lightFlickerDurationAvg + lightFlickerDurationDev));
            attachedLight.enabled = true;
            yield return new WaitForSeconds(Random.Range(lightFlickerCooldownAvg - lightFlickerCooldownDev, lightFlickerCooldownAvg + lightFlickerCooldownDev));
        }

        isFlickering = false;
    }
}
