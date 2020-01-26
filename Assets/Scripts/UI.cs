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
    [SerializeField] private Button RTBButton;

    private List<Button> buttons;
    private List<TextMeshProUGUI> texts;
    private List<TextMeshProUGUI> costs;

    [Header("Raid UI")]
    public GameObject RaidUIPanel;
    [SerializeField] private TextMeshProUGUI RaidTitle;
    [SerializeField] private TextMeshProUGUI RaidInfo;
    [SerializeField] private Button RaidButton;

    [Header("Research UI")]
    public GameObject ResearchPanel;
    public class Slot
    {
        public Image Item;
        public TextMeshProUGUI Recipe;
        public Button Research;

        public Slot(Image i, TextMeshProUGUI t, Button b)
        {
            Item = i;
            Recipe = t;
            Research = b;
        }
    }
    [SerializeField] private Button[] ResearchButtons;

    List<Slot> Slots = new List<Slot>();

    [Header("Build UI")]
    public GameObject BuildPanel;
    public Button[] BuildButtons;

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
        SetupResearchPanel();
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
            RTBButton.gameObject.SetActive(false);

            ToggleActionPanel(true);
            BaseActionUpdate();
        }
        else
        {
            RTBButton.gameObject.SetActive(true);
            ToggleActionPanel(false);
        }
    }

    void ToggleActionPanel(bool toggle)
    {
        BaseActionPanel.SetActive(toggle);
    }

    public void ButtonSetup()
    {
        Button[] buttons = BaseActionPanel.GetComponentsInChildren<Button>();

        buttons[0].onClick.AddListener(() => PlayerBase.M.Heal());
        buttons[0].onClick.AddListener(() => BaseActionUpdate());

        buttons[1].onClick.AddListener(() => PlayerBase.M.Repair());
        buttons[1].onClick.AddListener(() => BaseActionUpdate());

        buttons[2].onClick.AddListener(() => ToggleBuildPanel(true));
        buttons[2].onClick.AddListener(() => ToggleActionPanel(false));

        buttons[3].onClick.AddListener(() => ToggleResearchPanel(true));
        buttons[3].onClick.AddListener(() => ToggleActionPanel(false));

        buttons[4].onClick.AddListener(() => PlayerBase.M.Deploy());
        buttons[4].onClick.AddListener(() => BaseActionUpdate());

        RTBButton.onClick.AddListener(() => PlayerBase.M.RTB());
    }

    void BaseActionUpdate()
    {
        HealButton();
        Repair();
        Research();
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

    void Research()
    {
        if(PlayerBase.M.CombatLevel == 5)
        {
            texts[3].text = "Max Combat Level";
            buttons[3].enabled = false;
        }
    }

    #endregion

    #region Raid UI
    public void ToggleRaidUI(Location target)
    {
        RaidUIPanel.SetActive(true);

        RaidTitle.text = target.Name;
        RaidInfo.text = GetRaidInfo(target);

        if(target.raided)
        {
            RaidButton.GetComponentInChildren<TextMeshProUGUI>().text = "Raided";
            RaidButton.enabled = false;
        }
        else
        {
            RaidButton.enabled = true;
            RaidButton.onClick.RemoveAllListeners();
            RaidButton.onClick.AddListener(() => target.InititateRaid());
        }

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

    #region Research
    void ToggleResearchPanel(bool toggle)
    {
        UpdateResearchPanel();
        ResearchPanel.SetActive(toggle);
    }

    void SetupResearchPanel()
    {
        List<Image> images = new List<Image>();
        List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();

        foreach (Image i in ResearchPanel.GetComponentsInChildren<Image>())
        {
            if (!i.GetComponent<Button>())
                images.Add(i);
        }

        foreach (TextMeshProUGUI t in ResearchPanel.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (t.name.Contains("Recipe"))
                texts.Add(t);
        }

        for (int i = 0; i < 2; i++)
        {
            Slots.Add(new Slot(images[i], texts[i], ResearchButtons[i]));
        }

        ResearchButtons[2].onClick.RemoveAllListeners();
        ResearchButtons[2].onClick.AddListener(() => ToggleResearchPanel(false));
        ResearchButtons[2].onClick.AddListener(() => ToggleBaseUI(true));
    }

    void UpdateResearchPanel()
    {
        switch (PlayerBase.M.CombatLevel)
        {
            case 1:
                ResearchSlot("Crossbow", Slots[0]);
                ResearchSlot("Wooden Armour", Slots[1]);
                break;
            case 2:
                ResearchSlot("M1911", Slots[0]);
                ResearchSlot("Leather Armour", Slots[1]);
                break;
            case 3:
                ResearchSlot("MP5", Slots[0]);
                ResearchSlot("Kevlar Armour", Slots[1]);
                break;
            case 4:
                ResearchSlot("M4A1", Slots[0]);
                ResearchSlot("Metal Plate Armour", Slots[1]);
                break;
            case 5:
                ResearchSlot("M4A1", Slots[0]);
                ResearchSlot("Metal Plate Armour", Slots[1]);
                break;
        }

    }

    void ResearchSlot(string item, Slot slot)
    {
        //Slots[0].Item = null;

        if (PlayerBase.M.Query(item + " BP") > 0)
        {
            slot.Recipe.text = item + " Already Researched";
            slot.Research.enabled = false;
        }
        else
        {
            slot.Recipe.text = item + " (Cost: " + PlayerBase.M.ResearchCosts[item] + ")";

            if (PlayerBase.M.CheckResearch(item))
            {
                slot.Research.enabled = true;
                slot.Research.onClick.RemoveAllListeners();
                slot.Research.onClick.AddListener(() => PlayerBase.M.Research(item));
                slot.Research.onClick.AddListener(() => UpdateResearchPanel());
            }
            else
            {
                slot.Research.enabled = false;
            }
        }

    }

    #endregion

    #region Building
    void ToggleBuildPanel(bool toggle)
    {
        BuildPanel.SetActive(toggle);
        UpdateBuildPanel();
    }

    void UpdateBuildPanel()
    {
        if(!PlayerBase.M.Built[0] && PlayerBase.M.Query("Food") >= 150)
        {
            BuildButtons[0].enabled = true;
            BuildButtons[0].onClick.RemoveAllListeners();
            BuildButtons[0].onClick.AddListener(() => PlayerBase.M.BuildFarm());
        }
        else if (PlayerBase.M.Built[0])
        {
            BuildButtons[0].enabled = false;
            BuildButtons[0].GetComponent<TextMeshProUGUI>().text = "Built";
        }
        else
        {
            BuildButtons[1].enabled = false;
        }

        if (!PlayerBase.M.Built[1] && PlayerBase.M.Query("Metal") >= 250 && PlayerBase.M.Query("Scrap") >= 125)
        {
            BuildButtons[1].enabled = true;
            BuildButtons[1].onClick.RemoveAllListeners();
            BuildButtons[1].onClick.AddListener(() => PlayerBase.M.BuildWorkshop());
        }
        else if (PlayerBase.M.Built[2])
        {
            BuildButtons[1].enabled = false;
            BuildButtons[1].GetComponent<TextMeshProUGUI>().text = "Built";
        }
        else
        {
            BuildButtons[1].enabled = false;
        }

        if (!PlayerBase.M.Built[2] && PlayerBase.M.Query("Food") > 150)
        {
            BuildButtons[2].enabled = true;
            BuildButtons[2].onClick.RemoveAllListeners();
            BuildButtons[2].onClick.AddListener(() => PlayerBase.M.BuildRadioStation());
        }
        else if(PlayerBase.M.Built[2])
        {
            BuildButtons[2].enabled = false;
            BuildButtons[2].GetComponent<TextMeshProUGUI>().text = "Built";
        }
        else
        {
            BuildButtons[2].enabled = false;
        }

        BuildButtons[3].onClick.RemoveAllListeners();
        BuildButtons[3].onClick.AddListener(() => ToggleBaseUI(false));
        BuildButtons[3].onClick.AddListener(() => ToggleBaseUI(true));
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
