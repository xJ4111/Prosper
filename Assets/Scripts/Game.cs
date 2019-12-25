using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    public static Game M;
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

    [Header("Initialisation")]
    [SerializeField] private Character Character;
    [SerializeField] private Transform CharacterStart;
    private Character mCharacter;
    void Start()
    {
        mCharacter = Instantiate(Character, transform);
        PlayerBase.M.Players.Add(mCharacter);
        ShowMenu(true);
    }

    #region Game Initialisation
    public void ShowMenu(bool show)
    {
        if (show)
        {
            mCharacter.transform.position = CharacterStart.position;
            mCharacter.transform.rotation = CharacterStart.rotation;
            Environment.M.CleanUpWorld();
        }
        else
        {
            mCharacter.transform.position = Environment.M.Start.Position;
            mCharacter.transform.rotation = Quaternion.identity;
            mCharacter.CurrentPosition = Environment.M.Start;
        }

        UI.M.ToggleMenu(show);
    }

    public void Generate()
    {
        Environment.M.GenerateWorld();
    }
    #endregion

    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
