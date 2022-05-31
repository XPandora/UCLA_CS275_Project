﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalPerceptionManager : MonoBehaviour {

    const int threadGroupSize = 1024;

    public FishSettings settings;
    public ComputeShader compute;
    FishBase[] fishes;

    void Start()
    {
        FishPrey[] fish_preys = FindObjectsOfType<FishPrey>();
        foreach (FishPrey b in fish_preys) {
            b.Initialize(settings, null);
        }

        FishPredator[] fish_predators = FindObjectsOfType<FishPredator>();
        foreach (FishPredator b in fish_predators) {
            b.Initialize(settings, null);
        }

        FishPacifists[] fish_pacifists = FindObjectsOfType<FishPacifists>();
        foreach (FishPacifists b in fish_pacifists) {
            b.Initialize(settings, null);
        }

        fishes = new FishBase[fish_preys.Length + fish_predators.Length + fish_pacifists.Length];
        for (int i = 0; i < fishes.Length; i++) {
            if (i < fish_preys.Length) {
                fishes[i] = fish_preys[i];
            }
            else if (i < fish_preys.Length + fish_predators.Length) {
                fishes[i] = fish_predators[i - fish_preys.Length];
            }
            else {
                fishes[i] = fish_pacifists[i - fish_preys.Length - fish_predators.Length];
            }
        }
    }

    void Update()
    {
        if (fishes != null) {

            int numFishes = fishes.Length;
            var fishDat = new FishDataBuffer[numFishes];

            // copy previous information from fish
            for (int i = 0; i < fishes.Length; i++) {
                fishDat[i].position = fishes[i].position;
                fishDat[i].direction = fishes[i].forward;
                fishDat[i].type = (int) fishes[i].type;
                if (fishes[i].type == FishType.predator)
                    fishDat[i].threat = 0;
                else
                    fishDat[i].threat = 1;
            }
            // copied data to compute shader buffer
            var fishDatBuffer = new ComputeBuffer(numFishes, FishDataBuffer.Size);
            fishDatBuffer.SetData(fishDat);
            compute.SetBuffer(0, "fishDatBuffer", fishDatBuffer);
            compute.SetInt("numFishes", fishes.Length);
            compute.SetFloat("viewRadius", settings.perceptionRadius);
            compute.SetFloat("avoidRadius", settings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt(numFishes / (float) threadGroupSize);
            compute.Dispatch(0, threadGroups, 1, 1);
            // get buffer data (updated information)
            fishDatBuffer.GetData(fishDat);
            // push the information to fish
            for (int i = 0; i < fishes.Length; i++) {
                fishes[i].avgFlockHeading = fishDat[i].flockHeading;
                fishes[i].centreOfFlockmates = fishDat[i].flockCentre;
                fishes[i].avgAvoidanceHeading = fishDat[i].avoidanceHeading;
                fishes[i].numPerceivedFlockmates = fishDat[i].numFlockmates;

                fishes[i].F = fishDat[i].totFear;
                fishes[i].Fmax = fishDat[i].maxFear;

                fishes[i].nearestPredatorPos = fishDat[i].nearestPredatorPos;
            }

            fishDatBuffer.Release();
        }
    }

    public struct FishDataBuffer {
        public int type;
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public float totFear;
        public float maxFear;
        public float threat; // 0 for prey 1 for predator

        // eat
        public Vector3 nearestPredatorPos;
        public int eatenPreyNumber;
        // mate
        public Vector3 desiredMatePos;
        public intention desiredMateIntention;
        public Vector3 desiredMatesMatePos; // compare with self pos

        public static int Size // NOTE when you changed the buffer content also change the size
        {
            get
            {
                return sizeof(int) + sizeof(float) * 3 * 5 + sizeof(int) + sizeof(float) * 3 + 3 * sizeof(float) * 3 + sizeof(int) + sizeof(intention);
            }
        }
    }
}