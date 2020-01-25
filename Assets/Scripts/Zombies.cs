using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombies : MonoBehaviour
{
    public static Zombies M;
    private void Awake()
    {
        if (M == null)
        {
            M = this;
        }
        else if (M != this)
        {
            Destroy(this);
        }
    }

    [Header("Prefabs")]
    public GameObject DefaultZombie;
    public GameObject[] ZombiePrefabs;

    [Header("Spawn Paramaters")]
    public int ZombieCount;

    [Header("Zombie Information")]
    public List<Zombie> AllZombies;
    public float MSX;//Movement Speed Multiplier

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            Spawn();
    }

    public IEnumerator Spawn()
    {
        for (int i = 0; i < ZombieCount; i++)
        {
            EnvironmentTile spawnPos = Environment.M.EdgePieces[Random.Range(0, Environment.M.EdgePieces.Count - 1)];

            while (!spawnPos.IsAccessible)
            {
                spawnPos = Environment.M.EdgePieces[Random.Range(0, Environment.M.EdgePieces.Count - 1)];
            }

            GameObject spawned = Instantiate(DefaultZombie, spawnPos.Position, spawnPos.transform.rotation, transform);
            AllZombies.Add(spawned.GetComponent<Zombie>());
            spawned.GetComponent<Character>().CurrentPosition = spawnPos;
        }

        return null;
    }
}
