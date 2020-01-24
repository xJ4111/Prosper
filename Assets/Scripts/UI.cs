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

    [Header("Interaction UI")]
    [SerializeField] private GameObject Interact;
    [SerializeField] private TextMeshProUGUI InteractText;

    [Header("Base UI")]
    [SerializeField] public GameObject BaseUIPanel;
    [SerializeField] private GameObject BaseActionPanel;
    [SerializeField] private GameObject RTBButton;

    private List<Button> buttons;
    private List<TextMeshProUGUI> texts;
    private List<TextMeshProUGUI> costs;

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

    public void ToggleInteract(string Info)
    {
        BaseUIPanel.SetActive(false);
        Interact.SetActive(true);
        InteractText.text = Info;
    }

    public void ToggleInteract(bool toggle)
    {
        Interact.SetActive(false);
    }

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
        buttons[1].onClick.AddListener(() => PlayerBase.M.Upgrade());
        //buttons[2].onClick.AddListener(() => PlayerBase.M.AddBuilding());
        //buttons[3].onClick.AddListener(PlayerBase.M.RemoveBuilding());
        buttons[4].onClick.AddListener(() => PlayerBase.M.Deploy());
        buttons[5].onClick.AddListener(() => PlayerBase.M.RTB());
    }

    void BaseActionUpdate()
    {
        HealButton();
        UpgradeButton();
    }

    void HealButton()
    {
        bool NeedHeal;
        bool CanHeal;
        int HealCost;

        PlayerBase.M.CheckHeal(out NeedHeal, out CanHeal, out HealCost);

        if (NeedHeal)
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
    
    void UpgradeButton()
    {
        if (PlayerBase.M.UpgradeLevel == 2)
        {
            texts[1].text = "Base Fully Upgraded";
            costs[1].text = "";
            buttons[1].enabled = false;
        }
        else
        {
            float CurrentAmount;
            bool CanUpgrade;
            KeyValuePair<string, int> cost = PlayerBase.M.UpgradeInfo(out CurrentAmount, out CanUpgrade);

            if(CanUpgrade)
            {
                buttons[1].enabled = true;
                texts[1].text = "Upgrade";
                costs[1].text = cost.Key + " x" + (cost.Value / 1000) + "K/";
            }
            else if(!CanUpgrade)
            {
                buttons[1].enabled = false;
                texts[1].text = "Upgrade";
                costs[1].text = cost.Key + " " + (CurrentAmount / 1000) + "K/" + (cost.Value / 1000) + "K";
            }

        }
    }
}
