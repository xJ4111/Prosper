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

    public EnvironmentTile BaseTile;

    [Header("Player Info")]
    public List<Character> Players;
    public Dictionary<string, int> Inventory;
    public int ToolLevel;

    [Header("Player Actions")]
    public Interactable Target;

    void Start()
    {
        BaseTile = GetComponent<EnvironmentTile>();
        Inventory = new Dictionary<string, int>();
        AddItem("Rock", 1);
    }

    #region Player Information

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
            if(!player.Busy)
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
