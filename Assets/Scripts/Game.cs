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
    [SerializeField] private int CharacterCount;
    [SerializeField] private Transform CharacterStart;
    private Character mCharacter;
    void Start()
    {
        mCharacter = Instantiate(Character, CharacterStart);

        for (int i = 0; i < CharacterCount; i++)
        {
            PlayerBase.M.Players.Add(Instantiate(Character, CharacterStart));
        }
        ShowMenu(true);
    }

    #region Game Initialisation
    public void ShowMenu(bool show)
    {
        if (show)
        {
            Environment.M.CleanUpWorld();
        }
        else
        {
            Destroy(mCharacter);
            List<EnvironmentTile> used = new List<EnvironmentTile>();
            foreach(Character player in PlayerBase.M.Players)
            {
                EnvironmentTile temp = Environment.M.StartPos[Random.Range(0, Environment.M.StartPos.Count - 1)];

                if (!used.Contains(temp))
                    used.Add(temp);
                else
                {
                    while(used.Contains(temp))
                    {
                        temp = Environment.M.StartPos[Random.Range(0, Environment.M.StartPos.Count - 1)];
                    }

                    used.Add(temp);
                }

                player.transform.position = temp.Position;
                player.transform.rotation = Quaternion.identity;
                player.CurrentPosition = temp;
            }
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
