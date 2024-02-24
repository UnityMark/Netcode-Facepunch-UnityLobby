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
    [BoxGroup("CoopMenu"), Label("Parent"), SerializeField] private GameObject _playerFieldBox;
    [BoxGroup("CoopMenu"), Label("Prefab"), SerializeField] private GameObject _playerCardPrefab;
    [BoxGroup("CoopMenu"), Label("Btn ready"), SerializeField] private GameObject _btnReady;
    [BoxGroup("CoopMenu"), Label("Btn not ready"), SerializeField] private GameObject _btnNotReady;
    [BoxGroup("CoopMenu"), Label("Btn start"), SerializeField] private GameObject _btnStart;

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
        _mainMenu.SetActive(false);
        _coopMenu.SetActive(true);
        _btnNotReady.SetActive(false);
        _btnReady.SetActive(true);
        _btnStart.SetActive(false);
    }

    public void Disconnected()
    {
        MyInfo.instance.PlayerInformation.Clear();
        GameObject[] playerCards = GameObject.FindGameObjectsWithTag("PlayerCard");
        foreach (GameObject card in playerCards)
        {
            Destroy(card);
        }

        _mainMenu.SetActive(true);
        _coopMenu.SetActive(false);

        MyInfo.instance.ClearValue();
    }

    public void ReadyButton(bool ready)
    {
        NetworkTransmission.instance.IsTheClientReadyServerRPC(ready, MyInfo.instance.GetClientId());
    }
    

    public bool CheckIfPlayersAreReady()
    {
        bool ready = false;

        foreach (KeyValuePair<ulong,GameObject> player in MyInfo.instance.PlayerInformation)
        {
            if (!player.Value.GetComponent<PlayerInfo>().IsReady)
            {
                _btnStart.SetActive(false);
                return false;
            }
            else
            {
                _btnStart.SetActive(true);
                ready = true;
            }
        }

        return ready;
    }

    public void Quit() 
    {
        Application.Quit();
    }

    #region Request Function For Get or Set
    public void SetLobbyId(string lobbyId) => _labelLobbyId.text = lobbyId;
    public string GetInputField() => _inputField.text; 
    public GameObject GetPlayerFieldBox() => _playerFieldBox;
    public GameObject GetPlayerCardPrefab() => _playerCardPrefab;
    #endregion
}
