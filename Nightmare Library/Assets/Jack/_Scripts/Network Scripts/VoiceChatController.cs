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

    public static async void JoinChannel(string voiceChannel, ChatType chatType = ChatType.GROUP)
    {
        if (currentVoiceChannel != null && VivoxService.Instance.ActiveChannels.Keys.Contains(currentVoiceChannel))
            await LeaveChannel();

        currentVoiceChannel = voiceChannel;

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

    }
    public static async Task LeaveChannel()
    {
        if(VivoxService.Instance.ActiveChannels.Keys.Contains(currentVoiceChannel))
            await VivoxService.Instance.LeaveChannelAsync(currentVoiceChannel);
    }

    public static void UpdatePlayerPosition(GameObject player)
    {
        VivoxService.Instance.Set3DPosition(player, currentVoiceChannel);
    }
}
