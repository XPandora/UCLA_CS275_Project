using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishPrey : FishBase {

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