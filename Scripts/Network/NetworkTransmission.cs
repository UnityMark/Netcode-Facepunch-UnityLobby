using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class NetworkTransmission : NetworkBehaviour
{
    public static NetworkTransmission instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    //Сначала получает сервер а потом отсылает сервер всем клиентам

    [ServerRpc(RequireOwnership = false)]
    public void AddMeToDictionaryPlayerServerRPC(ulong steamId, string steamName, ulong clientId)
    {
        MyInfo.instance.AddPlayerToDictionary(steamId, steamName, clientId);
        MyInfo.instance.UpdateClientsDictionary();
    }

    [ClientRpc]
    public void UpdateClientsPlayerInfromationClientRpc(ulong steamId, string steamName, ulong clientId, bool isReady)
    {
        MyInfo.instance.AddPlayerToDictionaryClient(steamId, steamName, clientId, isReady);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveMeToDictionaryPlayerServerRPC(ulong steamId)
    {
        RemoveMeToDictionaryPlayerClientRPC(steamId);
    }

    [ClientRpc]
    public void RemoveMeToDictionaryPlayerClientRPC(ulong steamId)
    {
        MyInfo.instance.RemovePlayerFromDictionary(steamId);
        Debug.Log("removing client");
    }

    [ServerRpc(RequireOwnership = false)]
    public void IsTheClientReadyServerRPC(bool ready, ulong clientId)
    {
        AClientMightBeReadyClientRPC(ready, clientId);
    }

    [ClientRpc]
    public void AClientMightBeReadyClientRPC(bool ready, ulong clientId)
    {
        foreach (KeyValuePair<ulong, GameObject> player in MyInfo.instance.PlayerInformation)
        {
            if (player.Key == clientId)
            {
                player.Value.GetComponent<PlayerInfo>().IsReady = ready;
                player.Value.GetComponent<PlayerInfo>().ReadyImage.SetActive(ready);
                if (NetworkManager.Singleton.IsHost)
                {
                    Debug.Log(MenuManager.instance.CheckIfPlayersAreReady());
                }
            }
        }
    }

    //Check button start when our member leave
    [ServerRpc(RequireOwnership = false)]
    public void CheckStartButtonServerRPC() 
    {
        MenuManager.instance.CheckIfPlayersAreReady();
    }

}
