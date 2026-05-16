using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrapper : MonoBehaviour
{
    [Header("Initialization Settings")]
    [Tooltip("The name of the initial scene/room to load asynchronously upon game startup.")]
    public string startingRoomName = "MainRoom";

    private void Start()
    {
        Scene startScene = SceneManager.GetSceneByName(startingRoomName);

        if (!startScene.isLoaded)
        {
            SceneManager.LoadSceneAsync(startingRoomName, LoadSceneMode.Additive);
        }
    }
}