using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishPredator : FishBase {
    // YADI NOTE: I am not familiar with the derive in C sharp, so may be you need to add more stuff here i.e. construction overload?
    public void Initialize(FishSettings settings, Transform target)
    {
        this.target = target;
        this.settings = settings;
        this.type = FishType.predator;
        
        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }

    intention GenerateIntentioBasedOnHabit()
    {
        // TODO overload
        return intention.wander;
    }
}