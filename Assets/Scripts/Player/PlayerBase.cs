using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    public static PlayerBase M;
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

    [Header("Base Info")]
    public Building Main;
    public int UpgradeLevel;
    private int buildingCount;
    private HealthBar HB;

    [Header("Player Info")]
    public List<Character> Players;
    public Dictionary<string, int> Inventory;
    public int StorageCapacity;
    public int CombatLevel = 1;
    public int ToolLevel;

    public bool RaidOngoing = false;

    [Header("Player Actions")]
    public Interactable Target;
    public bool RTBCalled = false;

    [Header("Combat Info")]
    public Dictionary<int, Stats> Max = new Dictionary<int, Stats>();

    public class Stats
    {
        public float Health;
        public float Damage;
        public float AttackRange;
        public float AttackSpeed;
        public float HeadshotChance;

        public Stats(float hp, float dmg, float ar, float ats, float hc)
        {
            Health = hp;
            Damage = dmg;
            AttackRange = ar;
            AttackSpeed = ats;
            HeadshotChance = hc;
        }
    }

    void Start()
    {
        Inventory = new Dictionary<string, int>();
        StorageCapacity = 5000;
        AddItem("Rock", 1);
        Main = GetComponent<Building>();
        HB = GetComponentInChildren<HealthBar>();
        RTBCalled = false;

        UI.M.ButtonSetup();
        LoadCombatStatsCSV();
    }

    private void OnMouseOver()
    {
        if(Input.GetMouseButtonUp(0))
        {
            if(!Game.M.ZombiesSpawned)
            {
                UI.M.ToggleBaseUI(true);
            }
            else
            {
                Debug.Log("Players are defending the base");
            }
        }
    }

    private void Update()
    {
        HB.UpdateBar(Main.Health, 1000);
    }

    #region Base Info
    public KeyValuePair<string, int> UpgradeInfo(out float CurrentAmount, out bool CanUpgrade)
    {
        //Base cost + 1k per building
        int cost = 2500 + (1000 * buildingCount);

        CanUpgrade = false;
        CurrentAmount = 0;

        switch (UpgradeLevel)
        {
            case 0:
                CanUpgrade = Query("Stone") - cost >= 0;
                CurrentAmount = Query("Stone");
                return new KeyValuePair<string, int>("Stone", cost);
            case 1:
                CanUpgrade = Query("Metal") - cost >= 0;
                CurrentAmount = Query("Metal");
                return new KeyValuePair<string, int>("Metal", cost);
        }

        return new KeyValuePair<string, int>("", 0);
    }

    int Query(string type)
    {
        if (Inventory.ContainsKey(type))
            return Inventory[type];
        else
            return 0;
    }

    public void CheckHeal(out bool NeedHeal, out bool CanHeal, out int HealCost)
    {
        NeedHeal = false;
        int hurtCount = 0;

        foreach (Character player in Players)
        {
            if (player.Health < 100)
            {
                NeedHeal = true;
                hurtCount++;
            }
        }

        HealCost = hurtCount * 10;

        if (Query("Food") - HealCost > 0)
            CanHeal = true;
        else
            CanHeal = false;
    }

    #endregion

    #region Base Actions
    public void Defend()
    {
        UI.M.ToggleBaseUI(false);

        /*
        if (WindowFloor && !rtbcalled)
            RTB();
        else
        */

        if(RTBCalled)
        {
            foreach(Character player in Players)
            {
                player.StopAllCoroutines();
            }

            Deploy();
        }
        else
        {

            List<EnvironmentTile> used = new List<EnvironmentTile>();

            foreach (Character player in Players)
            {
                player.TargetBuilding = null;

                if (!Main.SpawnPoints.Contains(player.CurrentPosition))
                {
                    EnvironmentTile temp = Main.SpawnPoints[Random.Range(0, Main.SpawnPoints.Count)];

                    while (Taken(temp))
                    {
                        temp = Main.SpawnPoints[Random.Range(0, Main.SpawnPoints.Count)];
                    }

                    if (player.Busy)
                    {
                        player.PriorityTarget = temp;
                    }
                    else
                    {
                        player.GoTo(temp);
                        player.Busy = true;
                    }
                }
            }
        }
    }

    bool Taken(EnvironmentTile spawn)
    {
        bool taken = false;

        foreach(Character p in Players)
        {
            if (p.CurrentTarget != null && p.CurrentTarget == spawn)
                taken = true;
            else if (p.CurrentPosition == spawn)
                taken = true;
        }

        return taken;
    }

    public void RTB()
    {
        if(!RaidOngoing)
        {
            RTBCalled = true;

            foreach (Character player in Players)
            {
                player.TargetBuilding = Main;
                player.Garrisoned = true;

                if (!player.Busy)
                    player.GoTo(Main.DoorTile);
                else
                    player.PriorityTarget = Main.DoorTile;
            }
        }
        else
        {
            Debug.Log("Ongoing Raid, RTB Unavailable");
        }

        UI.M.ToggleBaseUI(false);
    }


    public void Heal()
    {
        foreach (Character player in Players)
        {
            player.Health = 100;
        }
    }

    public void Upgrade()
    {
        Debug.Log("Upgraded");
    }

    public void Deploy()
    {
        List<EnvironmentTile> used = new List<EnvironmentTile>();

        foreach (Character player in Players)
        {
            player.TargetBuilding = null;

            EnvironmentTile temp = Main.SpawnPoints[Random.Range(0, Main.SpawnPoints.Count)];

            if (!used.Contains(temp))
                used.Add(temp);
            else
            {
                while (used.Contains(temp))
                {
                    temp = Main.SpawnPoints[Random.Range(0, Main.SpawnPoints.Count)];
                }

                used.Add(temp);
            }

            player.CurrentPosition = temp;
            player.transform.position = temp.Position;
            player.Garrisoned = false;
        }

        UI.M.ToggleBaseUI(false);
        RTBCalled = false;
    }

    #endregion

    #region Player Actions
    public bool SendPlayer()
    {
        if (Closest(Target.Tile))
        {
            Closest(Target.Tile).Interact(Target);
            return true;
        }
        else
        {
            NoPlayerAvailable();
            return false;
        }
    }

    Character Closest(EnvironmentTile Target)
    {
        float lowest = float.MaxValue;
        Character closest = null;

        foreach (Character player in M.Players)
        {
            if(!player.Garrisoned && !player.Busy)
            {
                float distance = Environment.M.Heuristic(player.CurrentPosition, Target);
                if (distance < lowest)
                {
                    lowest = distance;
                    closest = player;
                }
            }
        }

        return closest;
    }

    void NoPlayerAvailable()
    {
        Debug.Log("No Player Available");
    }
    #endregion

    #region Inventory
    public void AddItem(string item, int Count)
    {
        if (Inventory.ContainsKey(item))
            Inventory[item] += Count;
        else
            Inventory.Add(item, Count);
    }

    public void RemoveItem(string item, int Count)
    {
        if (Inventory.ContainsKey(item))
        {
            if (Inventory[item] - Count > 0)
                Inventory[item] -= Count;
            else
                Inventory.Remove(item);
        }
    }
    #endregion

    #region Combat Info

    void LoadCombatStatsCSV()
    {
        TextAsset data = Resources.Load<TextAsset>("combatstats");

        string[] lines = data.text.Split('\n');

        for (int i = 1; i < lines.Length - 1; i++)
        {
            string[] cell = lines[i].Split(',');
            Max.Add(int.Parse(cell[0]), new Stats(float.Parse(cell[1]), float.Parse(cell[2]), float.Parse(cell[3]), float.Parse(cell[4]), float.Parse(cell[5])));
        }
    }

    #endregion
}
