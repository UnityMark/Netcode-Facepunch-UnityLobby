using NaughtyAttributes;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MyInfo : MonoBehaviour
{
    public static MyInfo instance { get; private set; } = null;

    [BoxGroup("Network Parameters"), SerializeField]
    private ulong _clientId;
    [BoxGroup("Network Parameters"), SerializeField]
    private ulong _steamId;
    [BoxGroup("Network Parameters"), SerializeField]
    private string _steamName;
    [BoxGroup("Network Parameters"), SerializeField]
    private bool _isHost;

    public Dictionary<ulong, GameObject> PlayerInformation = new Dictionary<ulong, GameObject>();

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

    public void AddPlayerToDictionary(ulong steamId, string steamName, ulong clientId)
    {
        if (!PlayerInformation.ContainsKey(clientId))
        {
            PlayerInfo pi = Instantiate(MenuManager.instance.GetPlayerCardPrefab(), MenuManager.instance.GetPlayerFieldBox().transform).GetComponent<PlayerInfo>();
            pi.SteamID = steamId;
            pi.SteanName = steamName;
            PlayerInformation.Add(clientId, pi.gameObject);
        }
    }

    public void AddPlayerToDictionaryClient(ulong steamId, string steamName, ulong clientId, bool isReady)
    {
        if (!PlayerInformation.ContainsKey(clientId))
        {
            PlayerInfo pi = Instantiate(MenuManager.instance.GetPlayerCardPrefab(), MenuManager.instance.GetPlayerFieldBox().transform).GetComponent<PlayerInfo>();
            pi.SteamID = steamId;
            pi.SteanName = steamName;
            pi.IsReady = isReady;
            PlayerInformation.Add(clientId, pi.gameObject);
        }
    }

    public void UpdateClientsDictionary()
    {
        foreach (KeyValuePair<ulong, GameObject> player in MyInfo.instance.PlayerInformation)
        {
            ulong steamId = player.Value.GetComponent<PlayerInfo>().SteamID;
            string steamName = player.Value.GetComponent<PlayerInfo>().SteanName;
            ulong clientId = player.Key;

            player.Value.GetComponent<PlayerInfo>().IsReady = player.Value.GetComponent<PlayerInfo>().IsReady;

            NetworkTransmission.instance.UpdateClientsPlayerInfromationClientRpc(steamId, steamName, clientId, player.Value.GetComponent<PlayerInfo>().IsReady);
            NetworkTransmission.instance.CheckStartButtonServerRPC(); //!!!!!!!!!!!!!!!!!!
        }
    }

    public void RemovePlayerFromDictionary(ulong steamId)
    {
        GameObject value = null; // Задаём спецально значения для проверки смениться ли у нас значения
        ulong key = 100; // Задаём спецально значения для проверки смениться ли у нас значения
        foreach (KeyValuePair<ulong, GameObject> player in PlayerInformation)
        {
            if(player.Value.GetComponent<PlayerInfo>().SteamID == steamId)
            {
                value = player.Value; // Тут меняем значения
                key = player.Key; // Тут меняем значения
            }
        }
        if(key != 100) // Если поменялось удаляем
        {
            PlayerInformation.Remove(key); 
        }
        if (value != null) // Если поменялось удаляем
        {
            Destroy(value);
        }
    }

    #region Function With Variable
    public void SetValues(ulong clientId, ulong steamId, string steamName, bool isHost)
    {
        _clientId = clientId;
        _steamId = steamId;
        _steamName = steamName;
        _isHost = isHost;
    }
    public void ClearValue()
    {
        _clientId = 0;
        _steamId = 0;
        _steamName = "";
        _isHost = false;
    } 
    public ulong GetClientId() => _clientId;
    public ulong GetSteamId() => _steamId;
    public string GetSteamName() => _steamName;

    [ContextMenu("Check Players")]
    public void CheckPlayers()
    {
        foreach(KeyValuePair<ulong, GameObject> player in PlayerInformation)
        {
            Debug.Log($"Player: {player.Value.name}");
        }
    }
    #endregion
}
