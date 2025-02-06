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
        var options = new InitializationOptions();

        #if UNITY_EDITOR
        options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
        #endif

        await UnityServices.InitializeAsync(options);
    }

    public async static Task<bool> SignInAnonymously()
    {
        try
        {
            OnProcessActive?.Invoke(true);

            await Authenticate();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                playerInfo = AuthenticationService.Instance.PlayerInfo;

                await VivoxService.Instance.InitializeAsync();
                await VoiceChatController.Login();
            }
            else
            {
                Debug.Log("Already Signed In");
                playerInfo = AuthenticationService.Instance.PlayerInfo;
            }

            signedIn = true;
            OnSignInStatusChanged?.Invoke(true);

            OnProcessActive?.Invoke(false);

            return true;
        }
        catch(Exception e)
        {
            Debug.Log("Failed Sign In: " + e.Message);
        }

        OnProcessActive?.Invoke(false);

        return false;
    }
    public static bool SignOut()
    {
        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut();

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

    private async void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        DontDestroyOnLoad(this);

        // Change this later to link to steam
        await SignInAnonymously();
    }
    private void OnApplicationQuit()
    {
        // Signs the player out if the application is quit
        SignOut();
    }

    
}
