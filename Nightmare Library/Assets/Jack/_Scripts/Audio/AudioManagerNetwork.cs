using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AudioManager))]
public class AudioManagerNetwork : NetworkBehaviour
{
    private AudioManager parent;

    private void Awake()
    {
        // Check if the network is running, if not, remove this component
        if (NetworkConnectionController.CheckNetworkConnected(this))
        {
            parent = GetComponent<AudioManager>();
            AudioManager.OnSoundPlay += OnSoundPlay;
        }
    }

    private void OnSoundPlay(AudioData sound, Vector3 pos)
    {
        Vector2 data;

        // If an AudioData was provided, get the index for the AudioData, if not, send an invalid index
        if (sound != null)
            data = AudioManager.audioReference[sound];
        else
            data = new Vector2(-1, -1);

        // Typical Server/Client interaction stuff
        if (IsServer)
            OnSoundPlayClientRpc((int)data.x, (int)data.y, pos, NetworkManager.LocalClientId);
        else
            OnSoundPlayServerRpc((int)data.x, (int)data.y, pos, NetworkManager.LocalClientId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnSoundPlayServerRpc(int i, int j, Vector3 pos, ulong sender)
    {
        // Convert the index into an audio Data
        AudioData data = AudioManager.GetAudioData(i, j);
        AudioManager.PlaySoundAtPointOffline(data, pos);

        // Tell clients to play that sound
        OnSoundPlayClientRpc(i, j, pos,sender);
    }
    [ClientRpc]
    private void OnSoundPlayClientRpc(int i, int j, Vector3 pos, ulong sender)
    {
        // If the client is not the one who sent the initial message, decode the indexes into an AudioData
        if (sender != NetworkManager.LocalClientId)
        {
            AudioData data = AudioManager.GetAudioData(i, j);
            AudioManager.PlaySoundAtPointOffline(data, pos);
        }
    }
}
