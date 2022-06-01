using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum intention { wander,
    avoid,
    eat,
    mate,
    escape,
    school,
    leave }

public enum FishType {
    prey = 0,
    predator = 1,
    pacifist = 2
}

public enum FishSex {
    MALE = 1,
    FEMALE = 2
}

public class FishBase : MonoBehaviour {

    public FishSettings settings;
    [HideInInspector]
    public FishType type
        = FishType.prey;
    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    public Vector3 velocity;
    public float H, L, F, Fmax;
    public float deltaTH, deltaTL, foodConsumed;
    public List<intention> memories;
    public intention It; // current intention
    public float size;
    public FishSex sex;

    // eat
    public Vector3 nearestPredatorPos;
    public int eatenPreyNumber;
    public float nearestPredatorDistance;
    // mate
    public int correct_like;
    public int desiredMateID;
    public Vector3 desiredMatePos;
    public Vector3 desiredMateDir;
    public intention desiredMateIntention;

    // To update:
    public Vector3 acceleration;
    [HideInInspector]
    public Vector3 avgFlockHeading;
    [HideInInspector]
    public Vector3 avgAvoidanceHeading;
    [HideInInspector]
    public Vector3 centreOfFlockmates;
    [HideInInspector]
    public int numPerceivedFlockmates;

    // Cached
    public Material material;
    public Transform cachedTransform;
    public Transform target;

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

        // Tim Should we use a countdown timer to generate intention ?
        // In Update() Method, it will generate intention in each frame. OOM?
        IntentionGenerator();

        // filtered the obtained information based on the intention
        FilterInfoByFocusser();
        // choose a certain behavior sequence
        Vector3 acceleration = Vector3.zero;
        switch (It) {
        case intention.wander:
            acceleration = DefaultBoidWander();
            break;
        // TODO cases for every other Its and the corresponding behavior wrappers
        case intention.avoid:
            acceleration = CollisiionAvoid();
            break;
        // case intention.eat: // NOTE: remember to reset the food consumed, and the deltaTH
        //     acceleration = DefaultBoidWander();
        //     break;
        //case intention.mate: // NOTE: remember to reset the and the deltaTL
        //    Debug.Log(It);
        //    acceleration = Mating(desiredMatePos);
        //    break;
        case intention.escape:
            // TODO : modify focusser_pos when focusser is completed
            Vector3 focusser_pos = nearestPredatorPos;
            acceleration = Escape(focusser_pos);

            // now judge if prey is being caught by a predator
            // TODO consider when to judge and destroy this prey
            nearestPredatorDistance = Vector3.Distance(position, nearestPredatorPos);
            //Debug.Log(nearestPredatorDistance);
            if (nearestPredatorDistance < settings.steerEatRange && type == FishType.prey) {
                Debug.Log("in Drag");
                acceleration = DragByPredator(nearestPredatorPos);
            }

            if (nearestPredatorDistance < settings.canEatRange && type == FishType.prey) {
                Debug.Log("in destory");
                Destroy(gameObject);
            }
            break;
        case intention.school:
            acceleration = Flock();
            break;
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

        // TODO consider how to set predator's foodConsumed = ?, deltaTH = 0

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

    public virtual void UpdateMentalStates()
    {
        H = Math.Min(1 - foodConsumed * (1 - settings.digestionRate * deltaTH) / size, 1);
        L = Math.Min(settings.libidoRate * deltaTL * (1 - H), 1);
        F = Math.Min(F, 1);
    }

    void IntentionGenerator()
    {
        intention ItMinus = It; // backup It at last step
        bool avoid = IsHeadingForCollision();
        if (avoid) {
            It = intention.avoid;
            if (ItMinus != intention.avoid) {
                if (memories.Count == 0)
                    memories.Add(ItMinus);
                else if (ItMinus != memories[memories.Count - 1])
                    memories.Add(ItMinus);
            }
        }
        else {
            if (F > settings.f0) {
                if (Fmax > settings.f1) {
                    It = intention.escape;
                    // Debug.Log(type);
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
                    intention Is = memories[memories.Count - 1];
                    memories.RemoveAt(memories.Count - 1);
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

    public virtual intention GenerateIntentioBasedOnHabit()
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

    Vector3 Looping(Vector3 focusser_pos)
    {
        Vector3 acceleration = Vector3.zero;
        Vector3 offset = focusser_pos - position;
        acceleration += SteerTowards(desiredMateDir) * settings.alignWeight;
        acceleration += SteerTowards(offset) * settings.cohesionWeight;
        acceleration += Vector3.Cross(offset, velocity).normalized * settings.roundingWeight;
        return acceleration;
    }

    // Wrappers for behaviour sequences
    Vector3 DefaultBoidWander()
    {
        return Steer() + Flock() + CollisiionAvoid();
    }

    Vector3 Escape(Vector3 focusser_pos)
    {
        return SteerTowards(position - focusser_pos) * 10f;
    }

    Vector3 DragByPredator(Vector3 predator_pos)
    {
        return SteerTowards(predator_pos - position) * 100f;
    }

    Vector3 MaleMating(Vector3 focusser_pos)
    {
        // desired female determined by dist
        // once find a target, remember it (for recovery from avoid), this remember is cancled until mating is finished
        Vector3 acceleration = Vector3.zero;
        Vector3 offset = focusser_pos - position;
        float dist = offset.magnitude;
        if (dist > settings.looping_dist) {
            // case 1: far away from female, chase (use steer)
            acceleration = SteerTowards(offset);
        }
        else if (dist > settings.touching_dist) {
            // case 2: middle range from female 1. if female target not mating or target is not self->looping and wait 2. else approach
            // Tim Line316 correct_like is int can not && with bool
            // Not sure is it correct to be correct_like == 0
            if (correct_like == 0 && (desiredMateIntention == intention.mate)) {
                acceleration = SteerTowards(offset);
            }
            else {
                acceleration = Looping(focusser_pos);
            }
        }
        else {
            // if very close, consider it as successful mating; reset timmer and L
            acceleration = SteerTowards(offset);
            deltaTL /= 2;
        }
        return acceleration;
    }

    Vector3 FemaleMating(Vector3 focusser_pos)
    {
        // desired male determined by size
        // all cases: approach the desired male (but slower than male)
        // if very close, consider it as successful mating; reset timmer and L
        Vector3 acceleration = Vector3.zero;
        Vector3 offset = focusser_pos - position;
        float dist = offset.magnitude;
        acceleration = SteerTowards(offset);
        if (dist < settings.touching_dist) {
            // if very close, consider it as successful mating; reset timmer and L
            deltaTL /= 2;
        }
        return acceleration;
    }

    Vector3 Leaving(Vector3 focusser_pos)
    {
        // triggered when mating is finished
        // use opposite direction to its current mating target as steer
        deltaTL = 0;
        return SteerTowards(position - focusser_pos) * 10f;
    }

    Vector3 Mating(Vector3 focusser_pos)
    {
        if (L > settings.r) {
            if (sex == FishSex.MALE) {
                return MaleMating(focusser_pos);
            }
            else {
                return FemaleMating(focusser_pos);
            }
        }
        else {
            return Leaving(focusser_pos);
        }
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