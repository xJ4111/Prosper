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
    [SerializeField] private List<Game.Item> loot;

    [Header("Raid Information")]
    [SerializeField] private Canvas Timer;
    [SerializeField] private TextMeshProUGUI TimerText;
    [SerializeField] private Image TimerBar;
    private bool raiding;
    [HideInInspector] public bool raided;
    private float startTime;

    private void Start()
    {
        loot = Game.M.LootTable[Name];
        DoorTile = Environment.M.ClosestTile(DoorFront.transform.position);
    }

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

        if (DefendPointsParent)
                DefendPointsParent.SetActive(!raided);
    }

    public void InititateRaid()
    {
        if(PlayerBase.M.RaidOngoing)
        {
            UI.M.Tooltip("Already Raiding Elsewhere");
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
                    }
                }
            }
            else
            {
                UI.M.Tooltip("No Players Available For Raid");
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

        TimerText.text = UI.M.GameTime(temp, false);
        TimerBar.rectTransform.sizeDelta = new Vector2(Timer.GetComponent<RectTransform>().sizeDelta.x * (temp / LootTime), TimerBar.rectTransform.sizeDelta.y);

        if(temp <= 0)
        {
            Timer.gameObject.SetActive(false);
            
            if(Guarded)
            {
                if (Random.Range(0, 100) < SuccessChance())
                {
                    UI.M.Tooltip("Raid Successful");
                    Loot();
                    Damage();
                }
                else
                {
                    UI.M.Tooltip("Raid Failed");
                    Damage();
                }
            }
            else
            {
                Loot();
            }

            ExitBuilding();
            raiding = false;
            raided = true;
            PlayerBase.M.RaidOngoing = false;
            startTime = 0;
        }
    }

    void Loot()
    {
        foreach(Game.Item item in loot)
        {
            if(Random.Range(0,100) < item.DropChance)
                PlayerBase.M.AddItem(item.Name, item.DropCount);
        }
    }

    void Damage()
    {

        foreach (Character player in PlayerBase.M.Players)
        {
            player.Health -= Random.Range((100 - SuccessChance()) - 10, (100 - SuccessChance()) + 10);
        }
    }

    public float SuccessChance()
    {
        if (EnemyLevel == PlayerBase.M.CombatLevel)
            return 75f;
        else if(EnemyLevel > PlayerBase.M.CombatLevel)
        {
            if(EnemyLevel - PlayerBase.M.CombatLevel == 2)
                return 25f;
            if (EnemyLevel - PlayerBase.M.CombatLevel == 1)
                return 50f;
        }
        else
        {
            return 100f;
        }

        return 100f;
    }

}
