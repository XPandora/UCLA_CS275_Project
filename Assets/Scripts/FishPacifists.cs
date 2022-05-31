using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishPacifists : FishBase {
    public void Initialize(FishSettings settings, Transform target)
    {
        this.target = target;
        this.settings = settings;
        this.type = FishType.pacifist;
        
        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }

    void UpdateMentalStates()
    {
        H = Math.Min(1 - foodConsumed * (1 - settings.digestionRate * deltaTH) / settings.appetite, 1);
        L = Math.Min(settings.libidoRate * deltaTL * (1 - H), 1);
        F = 0;
    }

    intention GenerateIntentioBasedOnHabit()
    {
        if (H > settings.r){
            return intention.eat;
        }else{
            return intention.mate;
        }
    }
}