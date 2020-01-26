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

    public float SecondToGametimeRatio;

    [Header("Location Loot")]
    public Dictionary<string, List<Item>> LootTable = new Dictionary<string, List<Item>>();

    public class Item
    {
        public string Name;
        public int DropCount;
        public int DropChance;

        public Item(string name, int count, int chance)
        {
            Name = name;
            DropCount = count;
            DropChance = chance;
        }
    }

    [Header("Zombie Wave Parameters")]
    public int WaveCount = 5;
    public float ZombieMultiplier = 2.5f;

    [Header("Game Loop")]
    public Light Sun;
    [HideInInspector] public bool Play;
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

        LocationLootTableCSV();
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

        StartCoroutine(SpawnPlayers());

        GameStart();
    }

    IEnumerator SpawnPlayers()
    {
        yield return new WaitForEndOfFrame();

        List<EnvironmentTile> used = new List<EnvironmentTile>();
        foreach (Character player in PlayerBase.M.Players)
        {
            EnvironmentTile temp = PlayerBase.M.Main.SpawnPoints[Random.Range(0, PlayerBase.M.Main.SpawnPoints.Count)];

            if (!used.Contains(temp))
                used.Add(temp);
            else
            {
                while (used.Contains(temp))
                {
                    temp = PlayerBase.M.Main.SpawnPoints[Random.Range(0, PlayerBase.M.Main.SpawnPoints.Count)];
                }

                used.Add(temp);
            }

            player.transform.position = temp.Position;
            player.transform.rotation = Quaternion.identity;
            player.CurrentPosition = temp;
        }
    }

    void LocationLootTableCSV()
    {
        TextAsset data = Resources.Load<TextAsset>("LocationLootTable");

        string[] lines = data.text.Split('\n');

        for (int i = 1; i < lines.Length - 1; i++)
        {
            string[] cell = lines[i].Split(',');
            LootTable.Add(cell[0], new List<Item>());
            for(int j = 1; j <= 5; j++)
            {
                string[] item = cell[j].Split('|');
                LootTable[cell[0]].Add(new Item(item[0], int.Parse(item[1]), int.Parse(item[2])));
            }
        }
    }

    #endregion

    #region Game Loop

    void GameStart()
    {
        Sun.intensity = 1;
        Play = true;
        roundStartTime = Time.time;
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
        Sun.transform.rotation = Quaternion.Euler(new Vector3(time, -30, 0));
    }

    void SpawnZombies()
    {
        PlayerBase.M.Defend();

        ZombiesSpawned = true;
        Zombies.M.Spawn();
    }

    void EndRound()
    {
        UI.M.Tooltip("Wave" + WaveCount + " Cleared");
        roundStarted = false;
        ZombiesSpawned = false;

        PlayerBase.M.Repair();
    }

    void NightOver()
    {
        NightCount++;
        PlayerBase.M.Deploy();

        roundStartTime = Time.time;
        CurrentWave = 0;

        foreach (Character p in PlayerBase.M.Players)
            p.Busy = false;

        foreach(Building l in Environment.M.AllBuildings)
        {
            if(l.GetComponent<Location>())
            {
                l.GetComponent<Location>().raided = false;
            }
        }

        if(PlayerBase.M.Built[0])
        {
            PlayerBase.M.AddItem("Food", 100);
        }
    }

    #endregion
    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
