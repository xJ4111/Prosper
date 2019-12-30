using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Interactable : MonoBehaviour
{
    [HideInInspector] public EnvironmentTile Tile;
    [HideInInspector] public Character TargetingPlayer;

    [SerializeField] private string Type;
    [SerializeField] private int Amount;

    public bool Interacted = false;
    private float interactTime;
    private float fraction;

    public GameObject TimerCanvas;

    private Canvas Timer;
    private TextMeshProUGUI TimerText;
    private Image TimerBar;
    float startTime;

    void Start()
    {
        SetInteractTile();
        SetTimerUI();
    }

    void Update()
    {
        DisplayTimer();
    }

    private void OnMouseOver()
    {
        if (!Interacted)
        {
            UI.M.ToggleInteract(GetInfo());
            if(Input.GetMouseButtonUp(0))
            {
                Interacted = true;
                PlayerBase.M.Target = this;
                PlayerBase.M.SendPlayer();
                UI.M.ToggleInteract(false);
            }
        }
    }

    private void OnMouseExit()
    {   
        UI.M.ToggleInteract(false);
    }

    #region Setup
    void SetInteractTile()
    {
        foreach (EnvironmentTile connection in GetComponent<EnvironmentTile>().Connections)
        {
            if (connection.IsAccessible && Vector3.Distance(GetComponent<EnvironmentTile>().Position, connection.Position) == 10)
                Tile = connection;
        }
    }

    void SetTimerUI()
    {
        GameObject temp = Instantiate(TimerCanvas, gameObject.transform);
        temp.transform.localPosition = new Vector3(5, 25, 5);

        Timer = GetComponentInChildren<Canvas>();
        TimerText = Timer.GetComponentInChildren<TextMeshProUGUI>();
        TimerBar = Timer.GetComponentInChildren<Image>();
    }
    #endregion

    #region UI
    public string GetInfo()
    {
        Calculate();
        return Type + " x" + (Amount * fraction) + "\n" + "Harvest Time: " + interactTime;
    }

    void DisplayTimer()
    {
        Timer.gameObject.SetActive(Interacted);

        if (Interacted)
        {
            Quaternion rot = Quaternion.LookRotation(CameraMovement.M.Cam.gameObject.transform.position - Timer.transform.position);
            Timer.transform.rotation = rot * Quaternion.Euler(0, 180, 0);

            float temp = (startTime + interactTime) - Time.time;

            TimerText.text = temp.ToString("F0") + "s";
            TimerBar.rectTransform.sizeDelta = new Vector2(Timer.GetComponent<RectTransform>().sizeDelta.x * (temp / interactTime), TimerBar.rectTransform.sizeDelta.y);
        }
    }
    #endregion

    #region Interaction
    public void Calculate()
    {
        switch (PlayerBase.M.ToolLevel)
        {
            case 0:
                interactTime = 10;
                fraction = 0.5f;
                break;
            case 1:
                interactTime = 5;
                fraction = 0.75f;
                break;
            case 2:
                interactTime = 5;
                fraction = 1f;
                break;
            case 3:
                interactTime = 2.5f;
                fraction = 1f;
                break;
        }
    }

    public IEnumerator Harvest()
    {
        TargetingPlayer.Busy = true;
        startTime = Time.time;
        yield return new WaitForSeconds(interactTime);
        if (PlayerBase.M.Inventory.ContainsKey(Type) && PlayerBase.M.Inventory[Type] + Amount > PlayerBase.M.StorageCapacity)
            PlayerBase.M.Inventory[Type] = PlayerBase.M.StorageCapacity;
        else
            PlayerBase.M.AddItem(Type, Amount);


        Environment.M.Replace(GetComponent<EnvironmentTile>());
        TargetingPlayer.Busy = false;
    }
    #endregion
}
