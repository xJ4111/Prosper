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
    public int EnemyLevel = 0;
    public string LootType;
    public float LootTime;
    [SerializeField] private string item;
    [SerializeField] private int count;

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
            UI.M.ToggleRaidUI(this);
        }
    }

    private void Update()
    {
        if(raiding)
        {
            RaidStatus();
        }
    }

    public void InititateRaid()
    {
        if(PlayerBase.M.RaidOngoing)
        {
            Debug.Log("Already Raiding Elsewhere");
            UI.M.ToggleRaidUI();
        }
        else
        {
            UI.M.ToggleRaidUI();

            if (!PlayerBase.M.RTBCalled)
            {
                raiding = true;
                PlayerBase.M.RaidOngoing = true;

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
            else
            {
                Debug.Log("No Players Available For Raid");
            }
        }
    }

    void RaidStatus()
    {
        if (startTime == 0)
            startTime = Time.time;

        if (!Timer.gameObject.activeSelf)
            Timer.gameObject.SetActive(true);

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
                if (Random.Range(0, 100) < (PlayerBase.M.CombatLevel / EnemyLevel) * 100)
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
            PlayerBase.M.RaidOngoing = false;
            startTime = 0;
        }
    }

    void Loot()
    {
        PlayerBase.M.AddItem(item, count);
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
