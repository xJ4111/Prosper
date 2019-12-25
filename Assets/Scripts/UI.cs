using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas Hud;

    [Header("Interaction UI")]
    [SerializeField] private GameObject Interact;
    [SerializeField] private TextMeshProUGUI InteractText;
    [SerializeField] private TextMeshProUGUI ButtonText;


    public void ToggleMenu(bool show)
    {
        Menu.enabled = show;
        Hud.enabled = !show;
    }

    public void ToggleInteract(string Info, string Action)
    {
        Interact.SetActive(true);
        InteractText.text = Info;
        ButtonText.text = Action;
    }

    public void ToggleInteract(bool toggle)
    {
        Interact.SetActive(false);
    }
}
