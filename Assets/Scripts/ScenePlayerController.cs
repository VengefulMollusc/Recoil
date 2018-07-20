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
            playerSettings.playerGameObject.SetActive(isActivePlayer);
            playerSettings.playerCamera.rect = isActivePlayer ? viewportSettings[playerCount - 1].viewports[i] : Rect.zero;
        }

        terrainGenerator.SetViewers(viewers);
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
public class PlayerInputSettings
{
    [Header("Axes")]
    public string xMovAxis = "Horizontal";
    public string yMovAxis = "Vertical";
    public string xCamAxis = "LookX";
    public string yCamAxis = "LookY";

    public bool useWeaponAxes;
    public string mainWeaponAxis = "WeaponMain";
    public string secondaryWeaponAxis = "WeaponSecondary";

    [Header("Buttons")]
    public string boostButton = "Jump";

    public string mainWeaponButton = "Fire1";
    public string secondaryWeaponButton = "Fire2";
}

[System.Serializable]
public class ViewportSettings
{
    public List<Rect> viewports;
}
