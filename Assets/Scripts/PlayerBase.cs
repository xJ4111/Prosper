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
    public EnvironmentTile BaseTile;
    public GameObject DoorFront;
    public EnvironmentTile DoorTile;

    public bool AllPlayersAtBase;
    public int PlayersAtBase = 0;
    public int UpgradeLevel;
    private int buildingCount;

    [Header("Player Info")]
    public List<Character> Players;
    public Dictionary<string, int> Inventory;
    public int StorageCapacity;
    public int CombatLevel;
    public int ToolLevel;


    [Header("Player Actions")]
    public Interactable Target;
    public bool RTBCalled;

    void Start()
    {
        BaseTile = GetComponent<EnvironmentTile>();
        Inventory = new Dictionary<string, int>();
        StorageCapacity = 5000;
        AddItem("Rock", 1);

        AllPlayersAtBase = false;
    }

    private void OnMouseOver()
    {
        if(Input.GetMouseButtonUp(0))
        {
            UI.M.ToggleBaseUI(true);
        }
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

    public void RTB()
    {
        RTBCalled = true;

        foreach (Character player in Players)
        {
            if(!player.AtBase)
            {
                if (!player.Busy)
                    player.GoTo(DoorTile);
                else
                    player.PriorityTarget = DoorTile;
            }
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
            EnvironmentTile temp = Environment.M.StartPos[Random.Range(0, Environment.M.StartPos.Count - 1)];

            if (!used.Contains(temp))
                used.Add(temp);
            else
            {
                while (used.Contains(temp))
                {
                    temp = Environment.M.StartPos[Random.Range(0, Environment.M.StartPos.Count - 1)];
                }

                used.Add(temp);
            }

            player.CurrentPosition = temp;
            player.transform.position = temp.Position;
            player.AtBase = false;
        }

        UI.M.ToggleBaseUI(false);
        RTBCalled = false;
    }

    #endregion

    #region Player Actions
    public void SendPlayer()
    {
        if(Closest(Target.Tile))
        {
            Closest(Target.Tile).Interact(Target);
        }
        else
        {
            NoPlayerAvailable();
        }
    }

    Character Closest(EnvironmentTile Target)
    {
        float lowest = float.MaxValue;
        Character closest = null;

        foreach (Character player in M.Players)
        {
            if(!player.AtBase && !player.Busy)
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
}
