using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class AuthenticationController : MonoBehaviour
{
    private static List<MonoBehaviour> observers = new List<MonoBehaviour>();

    private static AuthenticationController instance;
    public static PlayerInfo playerInfo { get; private set; }

    public delegate void SignInEvent(bool status);
    public static event SignInEvent OnSignInStatusChanged;

    public delegate void ProcessActiveDelegate(bool inProgress);
    public static event ProcessActiveDelegate OnProcessActive;

    public static bool signedIn { get; private set; } = false;

    public async static Task Authenticate()
    {
        // Retrieve the initialization options
        var options = new InitializationOptions();

        // Handle intitialization for parrel sync (this is for testing only)
        #if UNITY_EDITOR
        options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
        #endif

        await UnityServices.InitializeAsync(options);
    }

    public async static Task<bool> SignInAnonymously()
    {
        try
        {
            // Let other scripts know that this process is active
            OnProcessActive?.Invoke(true);

            // Authenticate, it literally says it right there
            await Authenticate();

            // If the player is not signed in, sign them in
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                // This may be changed later to use Steam
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                // Set the player info to the signed in player
                playerInfo = AuthenticationService.Instance.PlayerInfo;

                // Login to the Vivox voice controller service
                await VivoxService.Instance.InitializeAsync();
                await VoiceChatController.Login();
            }
            // If the player is already signed in, don't sign them in
            else
            {
                Debug.Log("Already Signed In");

                // Set the player info to the currently signed in player
                playerInfo = AuthenticationService.Instance.PlayerInfo;
            }

            // Alert that the player is now signed in
            signedIn = true;
            OnSignInStatusChanged?.Invoke(true);

            // Tell other scripts that this process is finished
            OnProcessActive?.Invoke(false);

            return true;
        }
        catch(Exception e)
        {
            Debug.Log("Failed Sign In: " + e.Message);
        }

        // Tell other scripts that this process is finished
        OnProcessActive?.Invoke(false);

        return false;
    }
    public static bool SignOut()
    {
        try
        {
            // If signed in, sign out
            if (!AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut();

            // Remove the signed in player Info
            playerInfo = null;

            signedIn = false;
            OnSignInStatusChanged?.Invoke(false);

            // Log the user out of the voice chat service
            VoiceChatController.Logout();

            return true;
        }
        catch (Exception e)
        {
            Debug.Log("Failed Sign Out: " + e.Message);
        }

        return false;
    }

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
    private void OnApplicationQuit()
    {
        // Signs the player out if the application is quit
        SignOut();
    }

    
}
