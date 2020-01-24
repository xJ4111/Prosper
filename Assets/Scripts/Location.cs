using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Location : Building
{
    [Header("Location Information")]
    public string Name;
    public bool Guarded;
    public int EnemyLevel;
    public string LootType;
    public float LootTime;

    [Header("Raid Information")]
    [SerializeField] private Canvas Timer;
    [SerializeField] private TextMeshProUGUI TimerText;
    [SerializeField] private Image TimerBar;
    private bool raiding;
    private float startTime;

    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log(LocationInfo());
            InititateRaid();
        }
    }

    private void Update()
    {
        if(raiding)
        {
            RaidStatus();
        }
    }

    string LocationInfo()
    {
        string info = "";

        info += "Loot Type: " + LootType + "\n";

        if (Guarded)
            info += "Guarded by Level " + EnemyLevel + " Enemies. " + PlayerBase.M.CombatLevel / EnemyLevel + " Success Chance \n";
        else
            info += "Location Unguarded. 100% Success Chance \n";

        info += "Loot Time: " + LootTime + "\n";

        return info;
    }

    void InititateRaid()
    {
        raiding = true;

        foreach (Character player in PlayerBase.M.Players)
        {
            player.TargetBuilding = this;

            if (!player.Busy)
            {
                player.GoTo(DoorTile);
                player.Busy = true;
            }
            else
            {
                player.PriorityTarget = DoorTile;
                player.Busy = true;
            }
        }
    }

    void RaidStatus()
    {
        if (startTime == 0)
            startTime = Time.time;

        Quaternion rot = Quaternion.LookRotation(CameraMovement.M.Cam.gameObject.transform.position - Timer.transform.position);
        Timer.transform.rotation = rot * Quaternion.Euler(0, 180, 0);

        float temp = (startTime + LootTime) - Time.time;

        TimerText.text = temp.ToString("F0") + "s";
        TimerBar.rectTransform.sizeDelta = new Vector2(Timer.GetComponent<RectTransform>().sizeDelta.x * (temp / LootTime), TimerBar.rectTransform.sizeDelta.y);

        if(temp <= 0)
        {
            Timer.gameObject.SetActive(false);
            
            if(Guarded)
            {
                if (Random.Range(0, 100) < PlayerBase.M.CombatLevel / EnemyLevel)
                {
                    Debug.Log("Raid Successful");
                    Loot();
                    Damage(true);
                }
                else
                {
                    Debug.Log("Raid Failed");
                    Damage(false);
                }
            }
            else
            {
                Loot();
            }

            ExitBuilding();
            raiding = false;
        }
    }

    void Loot()
    {
        PlayerBase.M.AddItem("Metal", 1000);
    }

    void Damage(bool raidWon)
    {
        if (raidWon)
        {
            foreach(Character player in PlayerBase.M.Players)
            {
                player.Health -= 25;
            }
        }
        else
        {
            foreach (Character player in PlayerBase.M.Players)
            {
                player.Health -= 50;
            }
        }
    }

}
