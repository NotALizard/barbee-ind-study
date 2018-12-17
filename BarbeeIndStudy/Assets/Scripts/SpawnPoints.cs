using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoints : MonoBehaviour {

    public Transform[] spawnpoints;

	public Vector3 GetRandomSpawn()
    {
        return spawnpoints[Random.Range(0, spawnpoints.Length)].position;
    }
}
