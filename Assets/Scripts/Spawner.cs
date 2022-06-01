using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public enum GizmoType { Never,
        SelectedOnly,
        Always }

    public FishPrey prey_prefab;
    public FishPredator predator_prefab; 
    public FishPacifists pacifist_prefab;
    public float spawnRadius = 10;
    public int preySpawnCount = 10;
    public int predatorSpawnCount = 0;
    public int pacifistSpawnCount = 0;
    public Color colour;
    public GizmoType showSpawnRegion;

    void Awake()
    {
        // Spawn different kinds of fish
        for (int i = 0; i < preySpawnCount; i++) {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            FishPrey fish = Instantiate(prey_prefab);
            fish.transform.position = pos;
            fish.transform.forward = Random.insideUnitSphere;
<<<<<<< Updated upstream

            fish.SetColour(colour);
=======
>>>>>>> Stashed changes
        }

        for (int i = 0; i < predatorSpawnCount; i++) {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            FishPredator fish = Instantiate(predator_prefab);
            fish.transform.position = pos;
            fish.transform.forward = Random.insideUnitSphere;

        }

        for (int i = 0; i < pacifistSpawnCount; i++) {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            FishPacifists fish = Instantiate(pacifist_prefab);
            fish.transform.position = pos;
            fish.transform.forward = Random.insideUnitSphere;

            fish.SetColour(colour);
        }
    }

    private void OnDrawGizmos()
    {
        if (showSpawnRegion == GizmoType.Always) {
            DrawGizmos();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (showSpawnRegion == GizmoType.SelectedOnly) {
            DrawGizmos();
        }
    }

    void DrawGizmos()
    {
        Gizmos.color = new Color(colour.r, colour.g, colour.b, 0.3f);
        Gizmos.DrawSphere(transform.position, spawnRadius);
    }
}