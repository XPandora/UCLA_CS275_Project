using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FishSettings : ScriptableObject {
    // Settings
    public float minSpeed = 2;
    public float maxSpeed = 5;
    public float perceptionRadius = 15f;
    public float flockRadius = 2.5f;
    public float avoidanceRadius = 1;
    public float maxSteerForce = 3;

    public float alignWeight = 1;
    public float cohesionWeight = 1;
    public float seperateWeight = 1;

    public float targetWeight = 1;
    public float steerEatRange = 100;
    public float canEatRange = 2;

    [Header("Collisions")]
    public LayerMask obstacleMask;
    public float boundsRadius = .54f;
    public float avoidCollisionWeight = 10;
    public float collisionAvoidDst = 5;

    [Header("Fish States")]
    public float digestionRate
        = 0.00067f;
    public float libidoRate = 0.0025f;
    // NOTE the above 2 can be tuned for different kinds of fish
    public float appetite = 1.0f; // TODO: needs to tune as in the paper, this const is missing
    public float dangerDist = 100f; // Distance to trigge a full (100%) fear

    public float f0 = .5f; // TODO: needs to tune
    public float f1 = .75f; // TODO: needs to tune f1 > f0
    public float r = .45f; // TODO: needs to tune (0 < r < 0.5); threshold of eating/mating

    [Header("Mating Consts")]
    public float looping_dist
        = 3f;
    public float touching_dist = 1.5f;
    public float roundingWeight = 1;
}