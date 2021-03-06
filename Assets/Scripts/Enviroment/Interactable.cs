﻿using System.Collections;
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
        SetTimerUI();
        Tile = GetComponent<EnvironmentTile>();
    }

    void Update()
    {
        DisplayTimer();
    }

    private void OnMouseOver()
    {
        if (!UI.M.BaseUIPanel.activeSelf && !Interacted)
        {
            UI.M.ToggleInteract(GetInfo());
            PlayerBase.M.Target = this;

            if (Input.GetMouseButtonUp(0) && PlayerBase.M.SendPlayer())
            {
                Interacted = true;
                UI.M.ToggleInteract();
            }
        }
    }

    private void OnMouseExit()
    {   
        UI.M.ToggleInteract();
    }

    #region Setup
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
        return Type + " x" + (Amount * fraction) + "\n" + "Harvest Time: " + UI.M.GameTime(interactTime, true);
    }

    void DisplayTimer()
    {
        Timer.gameObject.SetActive(Interacted);

        if (Interacted)
        {
            Quaternion rot = Quaternion.LookRotation(CameraMovement.M.Cam.gameObject.transform.position - Timer.transform.position);
            Timer.transform.rotation = rot * Quaternion.Euler(0, 180, 0);

            float temp = (startTime + interactTime) - Time.time;

            TimerText.text = UI.M.GameTime(temp, false);
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


        Environment.M.Clear(Tile);
        TargetingPlayer.Busy = false;
        Destroy(gameObject);
    }

    public EnvironmentTile TargetTile(Vector3 position)
    {
        EnvironmentTile Closest = null;
        float dist = float.MaxValue;

        foreach (EnvironmentTile t in Tile.Connections)
        {
            if (t.IsAccessible)
            {
                if (Vector3.Distance(t.Position, position) < dist)
                {
                    Closest = t;
                    dist = Vector3.Distance(t.Position, position);
                }
            }
        }

        if (Closest != null)
            return Closest;
        else
            return null;
    }
    #endregion
}
