using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI : MonoBehaviour
{
    public static UI M;
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

    [Header("Main Menu")]
    [SerializeField] private GameObject Menu;
    [SerializeField] private GameObject HUD;

    [Header("Round Info")]
    [SerializeField] private GameObject RoundInfoPanel;
    [SerializeField] private TextMeshProUGUI RoundTitleText;
    [SerializeField] private TextMeshProUGUI RoundInfoText;

    [Header("Interaction UI")]
    [SerializeField] private GameObject Interact;
    [SerializeField] private TextMeshProUGUI InteractText;

    [Header("Base UI")]
    public GameObject BaseUIPanel;
    [SerializeField] private GameObject BaseActionPanel;
    [SerializeField] private GameObject RTBButton;

    private List<Button> buttons;
    private List<TextMeshProUGUI> texts;
    private List<TextMeshProUGUI> costs;

    [Header("Raid UI")]
    public GameObject RaidUIPanel;
    [SerializeField] private TextMeshProUGUI RaidTitle;
    [SerializeField] private TextMeshProUGUI RaidInfo;
    [SerializeField] private Button RaidButton;

    private void Start()
    {
        buttons = new List<Button>();
        texts = new List<TextMeshProUGUI>();
        costs = new List<TextMeshProUGUI>();

        foreach(Button temp in BaseActionPanel.GetComponentsInChildren<Button>())
        {
            buttons.Add(temp);
            TextMeshProUGUI[] array = temp.GetComponentsInChildren<TextMeshProUGUI>();
            texts.Add(array[0]);
            if(array.Length > 1)
                costs.Add(temp.GetComponentsInChildren<TextMeshProUGUI>()[1]);
        }

        BaseUIPanel.SetActive(false);
    }

    public void ToggleMenu(bool show)
    {
        Menu.SetActive(show);
        HUD.SetActive(!show);
    }

    #region Round Info
    public void UpdateRoundInfo(float timeToRoundStart)
    {
        if(timeToRoundStart >= 0)
        {
            RoundTitleText.text = "Day " + Game.M.NightCount;
            RoundInfoText.text = "Sunset Imminent | Time Remaining: " + GameTime(timeToRoundStart, true);
        }
        else if (timeToRoundStart < 0)
        {
            RoundTitleText.text = "Night " + Game.M.NightCount + " | Wave " + Game.M.CurrentWave + "/" + Game.M.WaveCount;
            RoundInfoText.text = "Zombies Incoming | " + Zombies.M.AllZombies.Count + "/" + (int)(Game.M.RoundCount * Game.M.ZombieMultiplier);
        }
    }

    #endregion

    #region Interaction

    public void ToggleInteract(string Info)
    {
        BaseUIPanel.SetActive(false);
        Interact.SetActive(true);
        InteractText.text = Info;
    }

    public void ToggleInteract()
    {
        Interact.SetActive(false);
    }

    #endregion

    #region Base UI
    public void ToggleBaseUI(bool toggle)
    {
        Interact.SetActive(false);
        BaseUIPanel.SetActive(toggle);

        if (PlayerBase.M.RTBCalled)
        {
            RTBButton.SetActive(false);

            BaseActionPanel.SetActive(true);
            BaseActionUpdate();
        }
        else
        {
            RTBButton.SetActive(true);
            BaseActionPanel.SetActive(false);
        }
    }

    public void ButtonSetup()
    {
        Button[] buttons = BaseUIPanel.GetComponentsInChildren<Button>();

        buttons[0].onClick.AddListener(() => PlayerBase.M.Heal());
        buttons[1].onClick.AddListener(() => PlayerBase.M.Repair());
        buttons[2].onClick.AddListener(() => PlayerBase.M.AddBuilding());
        buttons[3].onClick.AddListener(() => PlayerBase.M.Research());
        buttons[4].onClick.AddListener(() => PlayerBase.M.Deploy());
        buttons[5].onClick.AddListener(() => PlayerBase.M.RTB());
    }

    void BaseActionUpdate()
    {
        HealButton();
        Repair();
    }

    void HealButton()
    {
        bool NeedHeal;
        bool CanHeal;
        int HealCost;

        PlayerBase.M.CheckHeal(out NeedHeal, out CanHeal, out HealCost);

        if (!NeedHeal)
        {
            buttons[0].enabled = false;
            texts[0].text = "All Players Healthy";
            costs[0].text = "";
        }
        else
        {
            if (CanHeal)
            {
                buttons[0].enabled = true;
                texts[0].text = "Heal";
                costs[0].text = "Food x" + HealCost.ToString();
            }
            else
            {
                buttons[0].enabled = false;
                texts[0].text = "Need Food To Heal";
                costs[0].text = "";
            }

        }
    }
    
    void Repair()
    {
        if(PlayerBase.M.Main.Health == PlayerBase.M.MaxHealth())
        {
            buttons[1].enabled = false;
            texts[1].text = "No Repairs Needed";
            costs[1].text = "";
        }
        else
        {
            if (PlayerBase.M.Inventory["Metal"] - PlayerBase.M.RepairCost() < 0)
            {
                buttons[1].enabled = false;
                texts[1].text = "Can't Afford Repairs";
                costs[1].text = "Need " + PlayerBase.M.RepairCost() + " Metal";
            }
            else
            {
                buttons[1].enabled = true;
                texts[1].text = "Repair Base";
                costs[1].text = "Metal x" + PlayerBase.M.RepairCost().ToString();
            }
        }

    }

    #endregion

    #region Raid UI
    public void ToggleRaidUI(Location target)
    {
        RaidUIPanel.SetActive(true);

        RaidTitle.text = target.Name;
        RaidInfo.text = GetRaidInfo(target);

        RaidButton.onClick.RemoveAllListeners();
        RaidButton.onClick.AddListener(() => target.InititateRaid());
    }

    public void ToggleRaidUI()
    {
        RaidUIPanel.SetActive(false);
    }

    string GetRaidInfo(Location target)
    {
        string info = "";

        info += "Loot Type: " + target.LootType + "\n";

        if (target.Guarded)
            info += "Guarded (Level " + target.EnemyLevel + " Enemies) " + target.SuccessChance() + "%" + " Success\n";
        else
            info += "Location Unguarded. 100% Success\n";

        info += "Loot Time: " + GameTime(target.LootTime, true) + "\n";

        return info;
    }
    #endregion

    public string GameTime(float time, bool full)
    {
        int hours = (int)(time / Game.M.SecondToGametimeRatio);
        float mins = 60 * ((time / Game.M.SecondToGametimeRatio) % 1);

        if(hours > 0)
        {
            if (full)
                return hours + " Hours " + mins.ToString("F0") + " Mins";
            else
            {
                return hours + "." + mins.ToString("F0") + "hrs";
            }
        }
        else
        {
            if (full)
                return mins.ToString("F0") + " Mins";
            else
            {
                return mins.ToString("F0") + "m";
            }
        }


    }
}
