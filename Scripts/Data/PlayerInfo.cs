using UnityEngine;
using TMPro;
using NaughtyAttributes;

public class PlayerInfo : MonoBehaviour
{
    [BoxGroup("Player Parameters")]
    public string SteanName;
    [BoxGroup("Player Parameters")]
    public ulong SteamID;

    [BoxGroup("UI Parameters")]
    public bool IsReady;
    [BoxGroup("UI Parameters")]
    public GameObject ReadyImage;
    [BoxGroup("UI Parameters"), SerializeField]
    private TMP_Text _labelSteamName;

    private void Start()
    {
        ReadyImage.SetActive(IsReady);
        _labelSteamName.text = SteanName;
    }

}
