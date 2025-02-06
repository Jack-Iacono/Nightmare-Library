using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;

public static class VoiceChatController
{

    private static string currentVoiceChannel = string.Empty;
    public enum ChatType { ECHO, GROUP, POSITIONAL };

    private static bool hasJoinedChannel = false;

    public static async Task Login()
    {
        LoginOptions options = new LoginOptions();
        options.DisplayName = "Temp User";
        options.EnableTTS = true;
        await VivoxService.Instance.LoginAsync(options);
    }
    public static async void Logout()
    {
        await VivoxService.Instance.LogoutAsync();
    }

    public static async void JoinChannel(string vcIdentifier, ChatType chatType = ChatType.GROUP)
    {
        // Ensures that no values can be changed while not in a channel
        hasJoinedChannel = false;

        if (currentVoiceChannel != null && VivoxService.Instance.ActiveChannels.Keys.Contains(currentVoiceChannel))
            await LeaveChannel();

        // Combines the current room with the join code
        currentVoiceChannel = NetworkConnectionController.joinCode + vcIdentifier;
        Debug.Log("Joining " + chatType.ToString() + " Channel: " + currentVoiceChannel);

        switch (chatType)
        {
            case ChatType.ECHO:
                // Used to hear own voice when in chat, testing only
                await VivoxService.Instance.JoinEchoChannelAsync(currentVoiceChannel, ChatCapability.AudioOnly);
                break;
            case ChatType.GROUP:
                // Used to hear all other users in game
                await VivoxService.Instance.JoinGroupChannelAsync(currentVoiceChannel, ChatCapability.AudioOnly);
                break;
            case ChatType.POSITIONAL:
                // Used to have positional audio? Not sure yet, will investigate
                Channel3DProperties prop = new Channel3DProperties();
                await VivoxService.Instance.JoinPositionalChannelAsync(currentVoiceChannel, ChatCapability.AudioOnly, prop);
                break;
        }

        // Allow channel related actions to occur
        hasJoinedChannel = true;
    }
    public static async Task LeaveChannel()
    {
        if(VivoxService.Instance.ActiveChannels.Keys.Contains(currentVoiceChannel))
            await VivoxService.Instance.LeaveChannelAsync(currentVoiceChannel);

        Debug.Log("Leaving Voice Channel: " + currentVoiceChannel.ToString());
    }

    public static void UpdatePlayerPosition(GameObject player)
    {
        if(hasJoinedChannel)
            VivoxService.Instance.Set3DPosition(player, currentVoiceChannel);
    }
}
