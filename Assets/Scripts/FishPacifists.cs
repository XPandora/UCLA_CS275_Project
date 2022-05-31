using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 1. different sex
// 2.

public class FishPacifists : FishBase {
    public void Initialize(FishSettings settings, Transform target)
    {
        this.target = target;
        this.settings = settings;
        this.type = FishType.pacifist;
        this.size = 1.0f;
        // this.sex = FishSex.NA;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }

    void UpdateMentalStates()
    {
        H = 0;
        L = Math.Min(settings.libidoRate * deltaTL * (1 - H), 1);
        F = 0;
    }

    intention GenerateIntentioBasedOnHabit()
    {
        if (H > settings.r) {
            return intention.eat;
        }
        else {
            return intention.mate;
        }
    }
}