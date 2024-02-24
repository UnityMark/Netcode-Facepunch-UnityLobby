using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSettings : NetworkBehaviour
{
    public static SceneSettings instance;

    [Header("Scene Parameters")]
    [SerializeField]
    private List<string> listScene = new List<string>();
    public IEnumerable<string> sceneActive => listScene;

    public void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        listScene.Add("Main Menu");
    }

    public void ClientAddstring(string nameScene)
    {
        listScene.Add(nameScene);
    }

    public void StartLevel(string nameScene)
    {
        if (IsServer && !string.IsNullOrEmpty(nameScene))
        {
            var status = NetworkManager.Singleton.SceneManager.LoadScene(nameScene, LoadSceneMode.Additive);
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to load {nameScene} " +
                      $"with a {nameof(SceneEventProgressStatus)}: {status}");
            }
            Debug.Log("Count: " + listScene.Count);
            NetworkTransmission.instance.AddSceneListClientRPC(nameScene);
        }
    }
    

    public void DisconnectToMenu()
    {
        if (IsServer && !string.IsNullOrEmpty("Main Menu"))
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Main Menu"));
            SceneManager.UnloadSceneAsync(GetLastActiveScene());
            //NetworkManager.Singleton.SceneManager.UnloadScene(GetLastActiveScene());
            listScene.Remove(GetLastActiveScene().name);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Main Menu"));
            SceneManager.UnloadSceneAsync(GetLastActiveScene());
            //NetworkManager.Singleton.SceneManager.UnloadScene(GetLastActiveScene());
            listScene.Remove(GetLastActiveScene().name);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

    }

    public void DisconectForcedMenu()
    {
        if (IsServer && !string.IsNullOrEmpty("Main Menu"))
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Main Menu"));
            foreach (string item in sceneActive)
            {
                if (item != "Main Menu")
                {
                    NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetSceneByName(item));
                    //SceneManager.UnloadSceneAsync(item);
                    listScene.Remove(item); // we can change to people.RemoveRange(1, GetCountActiveScene() + 1) after Foreach;
                }
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    #region Function Get()
    public int GetCountActiveScene() => sceneActive.Count() - 1;
    public Scene GetLastActiveScene() => SceneManager.GetSceneByName(listScene[GetCountActiveScene()]);
    public Scene GetActiveScene() => SceneManager.GetActiveScene();
    public string GetActiveSceneName() => SceneManager.GetActiveScene().name;
    public int GetActiveSceneID() => SceneManager.GetActiveScene().buildIndex;
    public bool GetInLevel() => "Main Menu" == GetLastActiveScene().name; 
    #endregion

}
