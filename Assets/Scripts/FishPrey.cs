using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishPrey : FishBase {

    public void Initialize(FishSettings settings, Transform target)
    {
        this.target = target;
        this.settings = settings;
        this.type = FishType.prey;
        
        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }

    intention GenerateIntentioBasedOnHabit()
    {
        if (H > settings.r)
        {
            return intention.eat;
        }
        else
        {
            // assume preys always like schooling
            return intention.school;
        }
    }
}