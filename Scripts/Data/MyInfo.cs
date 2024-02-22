using NaughtyAttributes;
using Unity.Netcode;
using UnityEngine;

public class MyInfo : NetworkBehaviour
{
    public static MyInfo instance { get; private set; } = null;

    [BoxGroup("Network Parameters"), SerializeField]
    private ulong _clientId;
    [BoxGroup("Network Parameters"), SerializeField]
    private ulong _steamId;
    [BoxGroup("Network Parameters"), SerializeField]
    private string _steamName;

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

    public void SetValues(ulong clientId, ulong steamId, string steamName)
    {
        _clientId = clientId;
        _steamId = steamId;
        _steamName = steamName;
    }
}
