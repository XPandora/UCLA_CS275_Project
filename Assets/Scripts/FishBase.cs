using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum intention { wander,
    avoid,
    eat,
    mate,
    escape,
    school,
    leave }

public class FishBase : MonoBehaviour {

    FishSettings settings;

    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    Vector3 velocity;
    float H, L, F, Fmax;
    float deltaTH, deltaTL, foodConsumed;
    List<intention> memories;
    intention It; // current intention

    // To update:
    Vector3 acceleration;
    [HideInInspector]
    public Vector3 avgFlockHeading;
    [HideInInspector]
    public Vector3 avgAvoidanceHeading;
    [HideInInspector]
    public Vector3 centreOfFlockmates;
    [HideInInspector]
    public int numPerceivedFlockmates;

    // Cached
    Material material;
    Transform cachedTransform;
    Transform target;

    void Awake()
    {
        material = transform.GetComponentInChildren<MeshRenderer>().material;
        cachedTransform = transform;
    }

    public void Initialize(FishSettings settings, Transform target)
    {
        this.target = target;
        this.settings = settings;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }

    public void SetColour(Color col)
    {
        if (material != null) {
            material.color = col;
        }
    }

    public void Update()
    {
        // mental states update
        UpdateMentalStates();
        // decision tree for choosing an intention
        IntentionGenerator();
        // filtered the obtained information based on the intention
        FilterInfoByFocusser();
        // choose a certain behavior sequence TODO
        Vector3 acceleration = Vector3.zero;
        switch (It) {
        case intention.wander:
            acceleration = DefaultBoidWander();
            break;
        // TODO cases for every other Its and the corresponding behavior wrappers
        // case intention.avoid:
        //     acceleration = DefaultBoidWander();
        //     break;
        // case intention.eat: // NOTE: remember to reset the food consumed, and the deltaTH
        //     acceleration = DefaultBoidWander();
        //     break;
        // case intention.mate: // NOTE: remember to reset the and the deltaTL
        //     acceleration = DefaultBoidWander();
        //     break;
        // case intention.escape:
        //     acceleration = DefaultBoidWander();
        //     break;
        // case intention.school:
        //     acceleration = DefaultBoidWander();
        //     break;
        // case intention.leave:
        //     acceleration = DefaultBoidWander();
        //     break;
        default:
            acceleration = DefaultBoidWander();
            break;
        }
        // DONE cases for every other Its and the corresponding behavior wrappers
        // accumulate deltaTs
        deltaTH += 1;
        deltaTL += 1;

        // move
        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp(speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;
    }

    void UpdateMentalStates()
    {
        H = Math.Min(1 - foodConsumed * (1 - settings.digestionRate * deltaTH) / settings.appetite, 1);
        L = Math.Min(settings.libidoRate * deltaTL * (1 - H), 1);
        F = Math.Min(F, 1);
    }

    void IntentionGenerator()
    {
        // NOTE look at the figure 5.
        intention ItMinus = It; // backup It at last step
        bool avoid = true; // TODO refine the avoid check here
        if (avoid) {
            It = intention.avoid;
            if (ItMinus != intention.avoid) {
                memories.Add(ItMinus);
            }
        }
        else {
            if (F > settings.f0) {
                if (Fmax > settings.f1) {
                    It = intention.escape;
                }
                else {
                    It = intention.school;
                }
            }
            else {
                if (memories.Count == 0) {
                    It = GenerateIntentioBasedOnHabit();
                }
                else {
                    intention Is = memories.RemoveAt(0); // TODO should pop 0 or pop last? what happens to a fish if it recovers from a danger
                    if (Is == intention.eat || Is == intention.mate) {
                        It = Is;
                    }
                    else {
                        It = GenerateIntentioBasedOnHabit();
                    }
                }
            }
        }
    }

    intention GenerateIntentioBasedOnHabit()
    {
        // NOTE be overloaded in different fishes
        return intention.wander;
    }

    void FilterInfoByFocusser()
    {
        // TODO
    }

    // Helper functions to construct behaviour sequences
    Vector3 Steer()
    {
        Vector3 acceleration = Vector3.zero;
        if (target != null) {
            Vector3 offsetToTarget = (target.position - position);
            acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
        }
        return acceleration;
    }

    Vector3 Flock()
    {
        Vector3 acceleration = Vector3.zero;

        if (numPerceivedFlockmates != 0) {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);

            var alignmentForce = SteerTowards(avgFlockHeading) * settings.alignWeight;
            var cohesionForce = SteerTowards(offsetToFlockmatesCentre) * settings.cohesionWeight;
            var seperationForce = SteerTowards(avgAvoidanceHeading) * settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        return acceleration;
    }

    Vector3 CollisiionAvoid()
    {
        Vector3 acceleration = Vector3.zero;

        if (IsHeadingForCollision()) {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        return acceleration;
    }

    // Wrappers for behaviour sequences
    Vector3 DefaultBoidWander()
    {
        return Steer() + Flock() + CollisiionAvoid();
    }
    // TODO add more wrappers

    // Other helpers
    bool IsHeadingForCollision()
    {
        RaycastHit hit;
        if (Physics.SphereCast(position, settings.boundsRadius, forward, out hit,
                settings.collisionAvoidDst,
                settings.obstacleMask)) {
            return true;
        }
        else {
        }
        return false;
    }

    Vector3 ObstacleRays()
    {
        Vector3[] rayDirections = PerceptionHelper.directions;

        for (int i = 0; i < rayDirections.Length; i++) {
            Vector3 dir = cachedTransform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(position, dir);
            if (!Physics.SphereCast(ray, settings.boundsRadius,
                    settings.collisionAvoidDst,
                    settings.obstacleMask)) {
                return dir;
            }
        }

        return forward;
    }

    Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, settings.maxSteerForce);
    }
}