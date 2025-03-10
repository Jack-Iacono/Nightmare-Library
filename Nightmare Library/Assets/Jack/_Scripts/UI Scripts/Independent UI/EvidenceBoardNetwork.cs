using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using static EvidenceBoardController;
using static EnemyPreset;

[RequireComponent(typeof(EvidenceBoardController))]
public class EvidenceBoardNetwork : NetworkBehaviour
{
    private EvidenceBoardController parent;

    private NetworkVariable<EvidenceDataS> evidenceData = new NetworkVariable<EvidenceDataS>();

    private void Awake()
    {
        if (!NetworkConnectionController.connectedToLobby)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }
        else
        {
            parent = GetComponent<EvidenceBoardController>();
            parent.OnEvidenceDataChange += OnEvidenceDataChange;

            if (IsServer)
                evidenceData.Value = new EvidenceDataS();
            else
                evidenceData.OnValueChanged += OnEvidenceDataValueChanged;
        }
    }

    private void OnEvidenceDataChange(int index, EvidenceData data)
    {
        if (IsServer)
        {
            // Adjusts the old value to hold the new values
            evidenceData.Value = EvidenceDataS.SetIndex(evidenceData.Value, index, data);
        }
        else
        {
            OnEvidenceDataChangeServerRpc(index, new bool[0]); 
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnEvidenceDataChangeServerRpc(int index, bool[] data)
    {
        //OnEvidenceDataChange(index, data);
    }
    private void OnEvidenceDataValueChanged(EvidenceDataS previousValue, EvidenceDataS newValue)
    {
        throw new NotImplementedException();
    }

    private class EvidenceDataS : INetworkSerializable
    {
        public bool[] data;

        public EvidenceDataS()
        {
            data = new bool[Enum.GetValues(typeof(EvidenceEnum)).Length * GameController.enemyCount];
        }
        public static EvidenceDataS SetIndex(EvidenceDataS eDataS, int index, EvidenceData eData)
        {
            // Replace the given index range
            for(int i = 0; i < eData.evidence.Length; i++)
            {
                eDataS.data[i + index * eData.evidence.Length] = eData.evidence[i];
            }

            return eDataS;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref data);
        }
    }
}
