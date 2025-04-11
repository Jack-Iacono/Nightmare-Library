using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AudioSourceController))]
public class AudioSourceNetwork : NetworkBehaviour
{
    private AudioSourceController parent;
    private void Awake()
    {
        if (!NetworkConnectionController.connectedToLobby)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }
        else
        {
            parent = GetComponent<AudioSourceController>();
            parent.OnPlay += OnPlay;
        }
    }

    private void OnPlay(AudioData sound)
    {
        Vector2 data;
        if (sound != null)
            data = AudioManager.audioReference[sound];
        else
            data = new Vector2(-1, -1);

        if (IsServer)
            OnPlayClientRpc((int)data.x, (int)data.y, NetworkManager.LocalClientId);
        else
            OnPlayServerRpc((int)data.x, (int)data.y, NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnPlayServerRpc(int i, int j, ulong sender)
    {
        if (sender != NetworkManager.LocalClientId)
        {
            // check for presence of audio source
            if (i != -1)
            {
                AudioData data = AudioManager.GetAudioData(i, j);
                parent.Play(data, true);
            }
            else
                parent.Play(true);
        }
        OnPlayClientRpc(i, j, sender);
    }
    [ClientRpc]
    private void OnPlayClientRpc(int i, int j, ulong sender)
    {
        if (sender != NetworkManager.LocalClientId)
        {
            // check for presence of audio source
            if (i != -1)
            {
                AudioData data = AudioManager.GetAudioData(i, j);
                parent.Play(data, true);
            }
            else
                parent.Play(true);
        }
    }
}
