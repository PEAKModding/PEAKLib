using BepInEx;
using Photon.Pun;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PEAKLib.Items;

/// <summary>
/// Partial class of TestsPlugin used to spawn the Monolith at game start.
/// </summary>
public partial class TestsPlugin : BaseUnityPlugin
{
    GameObject? MonolithPrefab;
    Vector3 MonolithPosition = new Vector3(-0.8f, 4.1f, -368.4f);

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Match match = new Regex(@"^Level_(\d+)$").Match(scene.name);
        if (match.Success &&
            int.TryParse(match.Groups[1].Value, out int level) &&
            level >= 0 && level <= 13 &&
            PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Log.LogInfo("Game Start detected, spawning Monolith");
            GameObject go = Instantiate(MonolithPrefab)!;
            go.transform.position = MonolithPosition;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene change events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
