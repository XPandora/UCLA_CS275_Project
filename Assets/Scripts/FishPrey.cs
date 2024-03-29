﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FishPrey : FishBase {

    public void Initialize(FishSettings settings, Transform target)
    {
        this.target = target;
        this.settings = settings;
        this.type = FishType.prey;
        this.size = 0.5f;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }

    public override void UpdateMentalStates()
    {
        H = 0;
        L = 0;
        F = Math.Min(F, 1);
    }

    public override intention GenerateIntentioBasedOnHabit()
    {
        if (H > settings.r) {
            return intention.eat;
        }
        else {
            return intention.school;
        }
    }
}