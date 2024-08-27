using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class ev_Flicker : Evidence
{
    

    protected float baseInteractChance = 0.05f;
    protected float currentInteractChance;

    protected float interactChanceTime = 0.5f;
    protected float interactChanceTimer = 0;

    protected float interactCooldownTime = 5f;
    protected float interactCooldownTimer = 0f;

    protected bool interactReady = false;

    protected float interactRange = 3;
    protected LayerMask interactLayers = 1 << 7;

    public ev_Flicker(Enemy owner) : base(owner)
    {
        interactChanceTimer = interactChanceTime;
        interactCooldownTimer = interactCooldownTime;
        currentInteractChance = baseInteractChance;
        interactReady = false;
    }

    public override void UpdateProcess(float dt)
    {
        // Check if the enemy is allowed to interact again via cooldown
        if (interactCooldownTimer > 0)
            interactCooldownTimer -= dt;
        else
        {
            if (interactReady)
            {
                Interact();
            }
            else
            {
                CheckInteractReady(dt);
            }
        }
    }

    protected void Interact()
    {
        Debug.Log("Flicker");

        owner.FlickerLights();

        interactReady = false;
        interactCooldownTimer = interactCooldownTime;
    }

    private void CheckInteractReady(float dt)
    {
        // Check if the timer is running or if it has ended
        if (interactChanceTimer > 0)
            interactChanceTimer -= dt;
        else
        {
            // Check for random chance
            if (Random.Range(0, 1f) < currentInteractChance)
            {
                // Chance succeeded and now the enemy will interact and reset it's odds back to base
                currentInteractChance = baseInteractChance;
                interactReady = true;
            }
            else
            {
                // Chance failed and now the enemy will have a greater chance to interact at the next attempt
                currentInteractChance *= 1.1f;
            }

            // Reset the timer to try again next time it is needed
            interactChanceTimer = interactChanceTime;
        }
    }
}
