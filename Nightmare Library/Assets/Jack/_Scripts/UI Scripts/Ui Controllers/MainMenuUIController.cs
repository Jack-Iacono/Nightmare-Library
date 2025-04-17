using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuUIController : UIController
{
    // This will include unique functionality for the main menu UI system
    public GameObject interactionBlocker;
    public TMP_Text blockerText;

    protected override void Awake()
    {
        base.Awake();

        NetworkConnectionController.OnProcessActive += OnNetworkProcessActive;
        AuthenticationController.OnProcessActive += OnAuthenticationProcessActive;

        interactionBlocker.SetActive(false);
    }

    protected override void Start()
    {
        base.Start();

        Cursor.lockState = CursorLockMode.None;

        if (NetworkConnectionController.IsRunning)
            ChangeToScreen(1);
        else
            ChangeToScreen(0);
    }

    /// <summary>
    /// Enables an interaction blocker when a network process is occuring
    /// </summary>
    /// <param name="b">Is the process running or not</param>
    public void OnNetworkProcessActive(bool b)
    {
        interactionBlocker.SetActive(b);
        blockerText.text = "Connecting to Server";
    }

    /// <summary>
    /// Enables an interaction blocker when an authenticator process is occuring
    /// </summary>
    /// <param name="b">Is the process running or not</param>
    public void OnAuthenticationProcessActive(bool b)
    {
        interactionBlocker.SetActive(b);
        blockerText.text = "Signing In";
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        NetworkConnectionController.OnProcessActive -= OnNetworkProcessActive;
        AuthenticationController.OnProcessActive -= OnAuthenticationProcessActive;
    }
}
