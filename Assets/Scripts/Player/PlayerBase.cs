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
    private HealthBar HB;

    [Header("Building Info")]
    public int buildingCount;
    public bool[] Built = { false, false, false };
    public float BaseCost = 250;
    public float PerBuildingCost = 100;
    public float BaseHealth = 1000;
    public float PerBuildingHealth = 250;

    [Header("Player Info")]
    public List<Character> Players;
    public Dictionary<string, int> Inventory = new Dictionary<string, int>();
    public Dictionary<string, int> ResearchCosts = new Dictionary<string, int>();
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
        public float SnapTime;

        public Stats(float hp, float dmg, float ar, float ats, float hc, float st)
        {
            Health = hp;
            Damage = dmg;
            AttackRange = ar;
            AttackSpeed = ats;
            HeadshotChance = hc;
            SnapTime = st;
        }
    }

    void Start()
    {
        StorageCapacity = 1000;
        Main = GetComponent<Building>();
        Main.Health = BaseHealth;
        HB = GetComponentInChildren<HealthBar>();
        RTBCalled = false;

        UI.M.ButtonSetup();
        LoadCombatStatsCSV();
        LoadResearchCosts();

        AddItem("Metal", 1000);
        AddItem("Scrap", 1000);
        AddItem("Food", 1000);

        /*
        AddItem("Wooden Armour", 1);
        AddItem("Crossbow", 1);
        AddItem("Leather Armour", 1);
        AddItem("M1911", 1);
        AddItem("Kevlar Armour", 1);
        AddItem("MP5", 1);
        AddItem("Metal Plate Armour", 1);
        AddItem("M4A1", 1);
        */

        for(int i = 0; i < Players.Count; i++)
        {
            Players[i].name = "Player " + (i + 1);
        }
    }

    private void OnMouseOver()
    {
        if(Input.GetMouseButtonUp(0))
        {
            if(!UI.M.BaseUIPanel.activeSelf && !Game.M.ZombiesSpawned)
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
        HB.UpdateBar(Main.Health, MaxHealth());
    }

    #region Base Actions
    public void Defend()
    {
        UI.M.ToggleBaseUI(false);

        foreach (Character player in Players)
        {
            player.TargetBuilding = Main;

            if (player.Busy)
            {
                player.PriorityTarget = Main.DoorTile;
            }
            else
            {
                player.GoTo(Main.DoorTile);
            }
        }
    }

    public void RTB()
    {
        if (!RaidOngoing)
        {
            RTBCalled = true;

            foreach (Character player in Players)
            {
                player.TargetBuilding = Main;

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
        bool need = false, can = false;
        int cost;

        CheckHeal(out need, out can, out cost);

        if(need && can)
        {
            foreach (Character player in Players)
            {
                player.Health = Stat("hp");
            }

            Inventory["Food"] -= cost;
        }
    }
    public void Repair()
    {
        if(Main.Health != MaxHealth() && Inventory["Metal"] - RepairCost() >= 0)
        {
            Inventory["Metal"] -= RepairCost();
            Main.Health = MaxHealth();
        }
    }
    public void BuildFarm()
    {
        Debug.Log("Farm Built");
    }
    public void BuildWorkshop()
    {
        Debug.Log("Workshop Built");
    }
    public void BuildRadioStation()
    {
        Debug.Log("Radio Station Built");
    }

    public void Deploy()
    {
        Main.ExitBuilding();
        UI.M.ToggleBaseUI(false);
        RTBCalled = false;
    }

    #endregion

    #region Base Info
    public int RepairCost()
    {
        int cost = (int)(BaseCost + (PerBuildingCost * buildingCount));
        return (int)(cost * (Main.Health / MaxHealth()));
    }

    public int MaxHealth()
    {
        return (int)(BaseHealth + (PerBuildingHealth * buildingCount));
    }

    public void CheckHeal(out bool NeedHeal, out bool CanHeal, out int HealCost)
    {
        NeedHeal = false;
        int hurtCount = 0;

        foreach (Character player in Players)
        {
            if (player.Health < Stat("hp"))
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

    public void EnterBase(Character player)
    {
        Vector3 temp = Main.DefendPoints[Random.Range(0, Main.DefendPoints.Length)].position;

        while(Taken(temp))
        {
            temp = Main.DefendPoints[Random.Range(0, Main.DefendPoints.Length)].position;
        }

        player.transform.position = temp;
        player.Garrisoned = true;
    }

    bool Taken(Vector3 spawn)
    {
        bool taken = false;

        foreach (Character p in Players)
        {
            if (Vector3.Distance(p.transform.position, spawn) < 0.005)
                taken = true;
        }

        return taken;
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

    public int Query(string type)
    {
        if (Inventory.ContainsKey(type))
            return Inventory[type];
        else
            return 0;
    }

    void LoadResearchCosts()
    {
        TextAsset data = Resources.Load<TextAsset>("researchcosts");

        string[] lines = data.text.Split('\n');

        for (int i = 1; i < lines.Length - 1; i++)
        {
            string[] cell = lines[i].Split(',');
            ResearchCosts.Add(cell[0], int.Parse(cell[1]));
        }

        foreach(KeyValuePair<string, int> costs in ResearchCosts)
        {
            Debug.Log(costs.Key + " " + costs.Value);
        }
    }

    public bool CheckResearch(string item)
    {
        if(Query(item) != 0)
        {
            if(Query("Scrap") > ResearchCosts[item])
            {
                return true;
            }
        }

        return false;
    }

    public void Research(string item)
    {
        Inventory["Scrap"] -= ResearchCosts[item];
        RemoveItem(item, 1);
        AddItem(item + " BP", 1);

        UpdateCombatLevel();
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
            Max.Add(int.Parse(cell[0]), new Stats(float.Parse(cell[1]), float.Parse(cell[2]), float.Parse(cell[3]), float.Parse(cell[4]), float.Parse(cell[5]), float.Parse(cell[6])));
        }
    }

    public float Stat(string stat)
    {
        switch (stat)
        {
            case "hp":
                return Max[CombatLevel].Health;
            case "dmg":
                return Max[CombatLevel].Damage;
            case "ar":
                return Max[CombatLevel].AttackRange;
            case "as":
                return Max[CombatLevel].AttackSpeed;
            case "hc":
                return Max[CombatLevel].HeadshotChance;
            case "st":
                return Max[CombatLevel].SnapTime;
        }

        Debug.LogError("Stat not found");
        return 0;
    }

    void UpdateCombatLevel()
    {
        int oldLevel = CombatLevel;

        if(Query("Crossbow BP") > 0 && Query("Wooden Armour BP") > 0)
        {
            CombatLevel = 2;
        }

        if (Query("M1911 BP") > 0 && Query("Leather Armour BP") > 0)
        {
            CombatLevel = 3;
        }

        if (Query("MP5 BP") > 0 && Query("Kevlar Armour BP") > 0)
        {
            CombatLevel = 4;
        }

        if (Query("M4A1 BP") > 0 && Query("Metal Plate Armour BP") > 0)
        {
            CombatLevel = 5;
        }

        if(oldLevel != CombatLevel)
        {
            foreach(Character player in Players)
            {
                player.Health = Max[CombatLevel].Health;
            }
        }
    }

    #endregion
}
