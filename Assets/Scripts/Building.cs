using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    [Header("Position")]
    public EnvironmentTile Centre;
    public Vector3Int Dimensions;

    public GameObject DoorFront;
    public EnvironmentTile DoorTile;
    public List<EnvironmentTile> SpawnPoints = new List<EnvironmentTile>();

    // Start is called before the first frame update
    void Start()
    {
        Centre = GetComponent<EnvironmentTile>();
        DoorTile = Environment.M.ClosestTile(DoorFront.transform.position);
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
        }
    }
}
