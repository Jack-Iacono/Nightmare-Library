using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Vivox;
using UnityEngine;

public static class VoiceChatController
{
    private static string currentVoiceChannel = string.Empty;
    public enum ChatType { ECHO, GROUP, POSITIONAL };

    private static bool hasJoinedChannel = false;
    private static ChatType currentChatType = ChatType.GROUP;

    public static string playerId
    {
        get => VivoxService.Instance.SignedInPlayerId;
    }
    private static List<string> mutedPlayers = new List<string>();

    public static async Task Login()
    {
        LoginOptions options = new LoginOptions();
        options.DisplayName = "Temp User";
        options.PlayerId = AuthenticationController.playerInfo.Id;
        await VivoxService.Instance.LoginAsync(options);

        VivoxService.Instance.ParticipantAddedToChannel += ParticipantAddedToChannel;
        VivoxService.Instance.ParticipantRemovedFromChannel += ParticipantRemovedFromChannel;
    }

    public static async void Logout()
    {
        if(VivoxService.Instance.IsLoggedIn)
            await VivoxService.Instance.LogoutAsync();
    }

    private static void ParticipantAddedToChannel(VivoxParticipant participant)
    {
        //Debug.Log($"Participant {participant.PlayerId} Added");

        if (mutedPlayers.Contains(participant.PlayerId))
        {
            participant.MutePlayerLocally();
        }
        else
            participant.UnmutePlayerLocally();
    }
    private static void ParticipantRemovedFromChannel(VivoxParticipant participant)
    {
        //Debug.Log($"Participant {participant.PlayerId} Removed");
    }

    public static async void JoinChannel(string vcIdentifier, ChatType chatType = ChatType.GROUP)
    {
        // Ensures that no values can be changed while not in a channel
        hasJoinedChannel = false;
        currentChatType = chatType;

        if (currentVoiceChannel != null && VivoxService.Instance.ActiveChannels.Keys.Contains(currentVoiceChannel))
            await LeaveChannel();

        // Combines the current room with the join code
        currentVoiceChannel = NetworkConnectionController.joinCode + vcIdentifier;

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
                Channel3DProperties prop = new Channel3DProperties(15, 10, 0.5f, AudioFadeModel.LinearByDistance);
                await VivoxService.Instance.JoinPositionalChannelAsync(currentVoiceChannel, ChatCapability.AudioOnly, prop);
                break;
        }

        // Allow channel related actions to occur
        hasJoinedChannel = true;
    }
    public static async Task LeaveChannel()
    {
        if(VivoxService.Instance != null && VivoxService.Instance.ActiveChannels.Keys.Contains(currentVoiceChannel))
            await VivoxService.Instance.LeaveChannelAsync(currentVoiceChannel);
    }

    public static void UpdatePlayerPosition(GameObject player)
    {
        if(currentChatType == ChatType.POSITIONAL && hasJoinedChannel)
            VivoxService.Instance.Set3DPosition(player, currentVoiceChannel);
    }
    public static void MutePlayer(ulong networkID, bool mute)
    {
        string playerId = LobbyController.playerList.Value.GetPlayerInfo(networkID).id;

        if (mute && !mutedPlayers.Contains(playerId))
        {
            mutedPlayers.Add(playerId);
        }
        else if(mutedPlayers.Contains(playerId))
        {
            mutedPlayers.Remove(playerId);
        }

        // Check to see if this particular participant in within the channel already
        ReadOnlyCollection<VivoxParticipant> participants = VivoxService.Instance.ActiveChannels[currentVoiceChannel];
        foreach (VivoxParticipant participant in participants)
        {
            if (mutedPlayers.Contains(participant.PlayerId))
            {
                participant.MutePlayerLocally();
            }
            else
                participant.UnmutePlayerLocally();
        }
    }
    public static void UnMuteAll()
    {
        mutedPlayers.Clear();

        // Check to see if this particular participant in within the channel already
        ReadOnlyCollection<VivoxParticipant> participants = VivoxService.Instance.ActiveChannels[currentVoiceChannel];
        foreach (VivoxParticipant participant in participants)
        {
            participant.UnmutePlayerLocally();
        }
    }
}
