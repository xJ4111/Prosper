﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    [Header("Stats")]
    public float Health;

    [Header("Position")]
    public EnvironmentTile Centre;
    public Vector3Int Dimensions;

    public GameObject DoorFront;
    public EnvironmentTile DoorTile;
    public List<EnvironmentTile> SpawnPoints = new List<EnvironmentTile>();
    public List<EnvironmentTile> AttackTiles = new List<EnvironmentTile>();

    public GameObject DefendPointsParent;
    [HideInInspector] public Transform[] DefendPoints;

    // Start is called before the first frame update
    void Start()
    {
        Centre = GetComponent<EnvironmentTile>();
        if(DoorFront)
            DoorTile = Environment.M.ClosestTile(DoorFront.transform.position);

        if (DefendPointsParent)
        {
            DefendPoints = DefendPointsParent.GetComponentsInChildren<Transform>();
        }
    }

    public void ExitBuilding()
    {
        List<EnvironmentTile> used = new List<EnvironmentTile>();

        foreach (Character player in PlayerBase.M.Players)
        {
            player.TargetBuilding = null;

            EnvironmentTile temp = SpawnPoints[Random.Range(0, SpawnPoints.Count - 1)];

            if (!used.Contains(temp))
                used.Add(temp);
            else
            {
                while (used.Contains(temp))
                {
                    temp = SpawnPoints[Random.Range(0, SpawnPoints.Count - 1)];
                }

                used.Add(temp);
            }

            player.CurrentPosition = temp;
            player.transform.position = temp.Position;
            player.TargetBuilding = null;
            player.Busy = false;
            player.Garrisoned = false;
        }
    }
}
