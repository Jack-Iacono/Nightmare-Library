using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AudioSourceController))]
public class AudioSourceNetwork : NetworkBehaviour
{
    AudioSourceController parent;
    private bool pool = false;

    private void Awake()
    {
        if (!NetworkConnectionController.IsRunning)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }
        else
        {
            parent = GetComponent<AudioSourceController>();
            parent.OnPlay += OnPlay;

            AudioManager.OnPoolObjects += OnPoolObject;
        }
    }

    // receives this call when spawned on the network
    public void OnPoolSpawn()
    {
        if (IsServer)
            OnObjectSpawnClientRpc(NetworkManager.LocalClientId);
    }
    [ClientRpc]
    protected void OnObjectSpawnClientRpc(ulong sender)
    {
        if(sender != NetworkManager.LocalClientId)
        {
            pool = true;
        }
    }

    private void OnPoolObject()
    {
        if (pool)
            parent.Pool();
    }

    protected void OnPlay(AudioData sound = null, bool move = false)
    {
        Vector2 data;
        if (sound != null)
            data = AudioManager.audioReference[sound];
        else
            data = new Vector2(-1,-1);
        Vector3 movePos = !move ? Vector3.negativeInfinity : transform.position;

        if (IsServer)
            OnPlayerClientRpc((int)data.x, (int)data.y, movePos, NetworkManager.LocalClientId);
        else
            OnPlayerServerRpc((int)data.x, (int)data.y, movePos, NetworkManager.LocalClientId);
    }
    [ServerRpc]
    private void OnPlayerServerRpc(int i, int j, Vector3 pos, ulong sender)
    {
        OnPlayerClientRpc(i, j, pos, sender);
    }
    [ClientRpc]
    private void OnPlayerClientRpc(int i, int j, Vector3 pos, ulong sender)
    {
        if (sender != NetworkManager.LocalClientId)
        {
            // check for presence of audio source
            if (i != -1)
            {
                AudioData data = AudioManager.GetAudioData(i, j);
                if (pos != Vector3.negativeInfinity)
                    parent.PlaySound(data, pos, true);
                else
                    parent.PlaySound(data, false);
            }
            else
                parent.PlaySound(true);
        }
    }
}
