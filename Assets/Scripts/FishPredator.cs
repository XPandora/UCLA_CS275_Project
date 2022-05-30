using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishPredator : FishBase {
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
        if (H > settings.r){
            return intention.eat;
        }else{
            return intention.wander;
        }
    }
}