using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    public static PauseController Instance;

    [SerializeField]
    private GameObject screen;

    private KeyCode keyPause = KeyCode.Q;
    private CursorLockMode lockModeStore = CursorLockMode.Locked;
    public static bool gamePaused = false;

    public delegate void OnGamePausedDelegate(bool paused);
    public static event OnGamePausedDelegate OnGamePaused;

    private void Awake()
    {
        if(Instance != null)
            Destroy(Instance);
        Instance = this;

        gamePaused = false;

        screen.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(keyPause))
        {
            gamePaused = !gamePaused;
            screen.SetActive(gamePaused);

            if (gamePaused)
            {
                lockModeStore = Cursor.lockState;
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                Cursor.lockState = lockModeStore;
            }

            OnGamePaused?.Invoke(gamePaused);
        }
    }

    public void LeaveGame()
    {
        LobbyController.instance.LeaveLobby();
    }
}
