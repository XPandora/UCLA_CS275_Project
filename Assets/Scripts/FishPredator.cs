using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FishPredator : FishBase {
    public void Initialize(FishSettings settings, Transform target)
    {
        this.target = target;
        this.settings = settings;
        this.type = FishType.predator;
        this.size = 2.0f;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }

    void UpdateMentalStates()
    {
        H = Math.Min(1 - foodConsumed * (1 - settings.digestionRate * deltaTH) / settings.appetite, 1);
        L = 0;
        F = 0;
    }

    intention GenerateIntentioBasedOnHabit()
    {
        if (H > settings.r) {
            return intention.eat;
        }
        else {
            return intention.wander;
        }
    }
}