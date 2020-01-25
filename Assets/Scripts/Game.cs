using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    public static Game M;
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

    [Header("Initialisation")]
    [SerializeField] private Character Character;
    [SerializeField] private int CharacterCount;
    [SerializeField] private Transform CharacterStart;
    private Character mCharacter;

    [Header("Game Loop")]
    public bool Play;
    public Light Sun;
    [HideInInspector] public int RoundCount;
    [SerializeField] private float RoundLength;
    public float RoundZombieMultiplier = 2.5f;
    private float roundStartTime;
    private bool roundStarted;
    [HideInInspector] public bool ZombiesSpawned;

    void Start()
    {
        //ShowMenu(true);
    }

    private void Update()
    {
        GameLoop();
    }

    #region Game Initialisation
    public void ShowMenu(bool show)
    {
        if (show)
        {
            Environment.M.CleanUpWorld();
        }
        else
        {
            Destroy(mCharacter);
            List<EnvironmentTile> used = new List<EnvironmentTile>();
            foreach(Character player in PlayerBase.M.Players)
            {
                EnvironmentTile temp = Environment.M.StartPos[Random.Range(0, Environment.M.StartPos.Count - 1)];

                if (!used.Contains(temp))
                    used.Add(temp);
                else
                {
                    while(used.Contains(temp))
                    {
                        temp = Environment.M.StartPos[Random.Range(0, Environment.M.StartPos.Count - 1)];
                    }

                    used.Add(temp);
                }

                player.transform.position = temp.Position;
                player.transform.rotation = Quaternion.identity;
                player.CurrentPosition = temp;
            }
        }

        UI.M.ToggleMenu(show);
    }

    public void Generate()
    {
        Environment.M.GenerateWorld();

        for (int i = 0; i < CharacterCount; i++)
        {
            PlayerBase.M.Players.Add(Instantiate(Character, CharacterStart));
        }

        GameStart();
    }
    #endregion

    #region Game Loop

    void GameStart()
    {
        Sun.intensity = 1;
        Play = true;
    }

    void GameLoop()
    {
        if(!roundStarted)
        {
            NewRound();
        }
        else
        {
            float roundTime = (roundStartTime + RoundLength) - Time.time;
            DayNightCycle(roundTime);

            if(roundTime <= 0)
            {
                if(!ZombiesSpawned)
                {
                    SpawnZombies();
                }
                else if(ZombiesSpawned && Zombies.M.AllZombies.Count == 0)
                {
                    EndRound();
                }
            }

            UI.M.UpdateRoundInfo(roundTime);
        }
    }

    void NewRound()
    {
        if(Play)
        {
            RoundCount++;
            roundStartTime = Time.time;
            roundStarted = true;
            ZombiesSpawned = false;
        }
    }

    void DayNightCycle(float time)
    {
        Debug.Log(time);
    }

    void SpawnZombies()
    {
        Zombies.M.ZombieCount = (int)(RoundCount * RoundZombieMultiplier);
        Zombies.M.Spawn();
        ZombiesSpawned = true;
    }

    void EndRound()
    {
        Debug.Log("Round Over");
        roundStarted = false;
        ZombiesSpawned = false;
    }

    #endregion
    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
