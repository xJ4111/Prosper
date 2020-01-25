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


    public int WaveCount = 5;
    public float ZombieMultiplier = 2.5f;

    [Header("Game Loop")]
    public bool Play;
    public Light Sun;
    [HideInInspector] public int NightCount;
    [HideInInspector] public int RoundCount;
    [HideInInspector] public int CurrentWave;
    [SerializeField] private float RoundLength;
    private float roundStartTime;
    private bool roundStarted;
    [HideInInspector] public bool ZombiesSpawned = false;

    void Start()
    {
        //ShowMenu(true);
        NightCount = 1;
        WaveCount = 5;
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
                EnvironmentTile temp = Environment.M.StartPos[Random.Range(0, Environment.M.StartPos.Count)];

                if (!used.Contains(temp))
                    used.Add(temp);
                else
                {
                    while(used.Contains(temp))
                    {
                        temp = Environment.M.StartPos[Random.Range(0, Environment.M.StartPos.Count)];
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
        float roundTime = (roundStartTime + RoundLength) - Time.time;
        DayNightCycle(roundTime);


        if (roundTime <= 0)
        {
            if(CurrentWave <= WaveCount)
            {
                if (!roundStarted)
                {
                    NewRound();
                }
                else
                {
                    if (!ZombiesSpawned)
                    {
                        SpawnZombies();
                    }
                    else if (ZombiesSpawned && Zombies.M.AllZombies.Count == 0)
                    {
                        EndRound();
                    }
                }
            }
            else
            {
                NightOver();
            }

        }

        UI.M.UpdateRoundInfo(roundTime);
    }

    void NewRound()
    {
        if(Play)
        {
            RoundCount++;
            CurrentWave++;
            roundStarted = true;
            ZombiesSpawned = false;

            Zombies.M.ZombieCount = (int)(RoundCount * ZombieMultiplier);
        }
    }

    void DayNightCycle(float time)
    {
       
    }

    void SpawnZombies()
    {
        PlayerBase.M.Defend();

        ZombiesSpawned = true;
        Zombies.M.Spawn();
    }

    void EndRound()
    {
        Debug.Log("Round Over");
        roundStarted = false;
        ZombiesSpawned = false;
    }

    void NightOver()
    {
        NightCount++;

        roundStartTime = Time.time;
        CurrentWave = 0;

        foreach (Character p in PlayerBase.M.Players)
            p.Busy = false;
    }

    #endregion
    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
