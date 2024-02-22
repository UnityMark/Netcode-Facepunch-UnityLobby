using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance { get; private set; } = null;

    [BoxGroup("MainMenu"), Label("GameObject"), SerializeField] private GameObject _mainMenu;
    [BoxGroup("MainMenu"), Label("InputField"), SerializeField] private TMP_InputField _inputField;

    [BoxGroup("CoopMenu"), Label("GameObject"), SerializeField] private GameObject _coopMenu;
    [BoxGroup("CoopMenu"), Label("Label"), SerializeField] private TMP_Text _labelLobbyId;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Connected()
    {

    }

    public void Disconnected()
    {

    }

    public void Quit() 
    {
        Application.Quit();
    }

    #region Request Function For Get or Set
    public void SetLobbyId(string lobbyId) => _labelLobbyId.text = lobbyId;
    public string GetInputField() => _inputField.text; 
    #endregion
}
