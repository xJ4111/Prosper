using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [HideInInspector] public EnvironmentTile Tile;
    [HideInInspector] public Character TargetingPlayer;

    [SerializeField] private string Category;
    [SerializeField] private string Type;
    [SerializeField] private int Amount;

    private float interactTime;
    private float fraction;

    private void Start()
    {
        foreach(EnvironmentTile connection in GetComponent<EnvironmentTile>().Connections)
        {
            if (connection.IsAccessible && Vector3.Distance(GetComponent<EnvironmentTile>().Position, connection.Position) == 10)
                Tile = connection;
        }
    }

    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(0))
        {
            PlayerBase.M.Target = this;
            UI.M.ToggleInteract(DisplayInfo(), Category);
        }
    }

    public string DisplayInfo()
    {
        Calculate();
        return Type + " x" + (Amount * fraction) + "\n" + "Harvest Time: " + interactTime;
    }

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
        yield return new WaitForSeconds(interactTime);
        PlayerBase.M.AddItem(Type, Amount);
        Environment.M.Replace(GetComponent<EnvironmentTile>());
        TargetingPlayer.Busy = false;
    }
}
