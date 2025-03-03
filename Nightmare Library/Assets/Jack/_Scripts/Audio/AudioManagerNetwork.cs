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
        if (!NetworkConnectionController.connectedToLobby)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }
        else
        {
            parent = GetComponent<AudioManager>();
            AudioManager.OnSoundPlay += OnSoundPlay;
        }
    }

    private void OnSoundPlay(AudioData sound, Vector3 pos)
    {
        Vector2 data;
        if (sound != null)
            data = AudioManager.audioReference[sound];
        else
            data = new Vector2(-1, -1);

        if (IsServer)
            OnSoundPlayClientRpc((int)data.x, (int)data.y, pos, NetworkManager.LocalClientId);
        else
            OnSoundPlayServerRpc((int)data.x, (int)data.y, pos, NetworkManager.LocalClientId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnSoundPlayServerRpc(int i, int j, Vector3 pos, ulong sender)
    {
        AudioData data = AudioManager.GetAudioData(i, j);
        AudioManager.PlaySoundOffline(data, pos);

        OnSoundPlayClientRpc(i, j, pos,sender);
    }
    [ClientRpc]
    private void OnSoundPlayClientRpc(int i, int j, Vector3 pos, ulong sender)
    {
        if (sender != NetworkManager.LocalClientId)
        {
            AudioData data = AudioManager.GetAudioData(i, j);
            AudioManager.PlaySoundOffline(data, pos);
        }
    }
}
