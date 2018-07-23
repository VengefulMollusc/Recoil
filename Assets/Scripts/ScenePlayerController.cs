using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenePlayerController : MonoBehaviour
{
    public TerrainGenerator terrainGenerator;
    [Header("Players")]
    [Range(1, 4)]
    public int playerCount;
    public List<PlayerSettings> playerSettingsList;
    public List<ViewportSettings> viewportSettings;

    private static ScenePlayerController instance;

    public static ScenePlayerController Instance()
    {
        if (instance == null)
            instance = FindObjectOfType<ScenePlayerController>();

        return instance;
    }

    public static int GetPlayerCount()
    {
        return Instance().playerCount;
    }

    void Awake()
    {
        SetupPlayers();
    }

    void SetupPlayers()
    {
        List<Transform> viewers = new List<Transform>();

        for (int i = 0; i < 4; i++)
        {
            PlayerSettings playerSettings = playerSettingsList[i];
            if (playerSettings.playerGameObject == null)
            {
                Debug.Log("Player " + (i + 1) + " not set up");
                continue;
            }

            bool isActivePlayer = i < playerCount;
            if (isActivePlayer)
                viewers.Add(playerSettings.playerGameObject.transform);

            playerSettings.playerGameObject.GetComponent<InputController>().SetInputSettings(playerSettings.inputSettings);
            //playerSettings.playerGameObject.SetActive(isActivePlayer);
            playerSettings.playerGameObject.SetActive(false);
            playerSettings.playerCamera.rect = isActivePlayer ? viewportSettings[playerCount - 1].viewports[i] : Rect.zero;
        }

        terrainGenerator.SetViewers(viewers);
        terrainGenerator.OnTerrainLoaded += OnTerrainLoaded;
    }

    void OnTerrainLoaded()
    {
        for (int i = 0; i < playerCount; i++)
        {
            playerSettingsList[i].playerGameObject.SetActive(true);
        }
        Debug.Log("Players Enabled");
    }

    void OnValidate()
    {
        if (playerSettingsList == null)
        {
            playerSettingsList = new List<PlayerSettings>(playerCount);
        }

        if (playerSettingsList.Count > 4)
        {
            playerSettingsList.RemoveRange(4, playerSettingsList.Count - 4);
        }

        if (playerSettingsList.Count < playerCount)
        {
            int addCount = playerCount - playerSettingsList.Count;
            for (int i = 0; i < addCount; i++)
            {
                playerSettingsList.Add(new PlayerSettings());
            }
        }
    }
}

[System.Serializable]
public class PlayerSettings
{
    public GameObject playerGameObject;
    public Camera playerCamera;
    public PlayerInputSettings inputSettings;
}

[System.Serializable]
public class ViewportSettings
{
    public List<Rect> viewports;
}
