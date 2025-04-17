using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using static EvidenceBoardController;
using static EnemyPreset;
using Unity.Collections.LowLevel.Unsafe;

[RequireComponent(typeof(EvidenceBoardController))]
public class EvidenceBoardNetwork : NetworkBehaviour
{
    private EvidenceBoardController parent;

    private NetworkVariable<EvidenceBoardData> evidenceData = new NetworkVariable<EvidenceBoardData>();

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
        }
    }
    public override void OnNetworkSpawn()
    {
        if (NetworkManager.IsServer)
            evidenceData.Value = new EvidenceBoardData();
        else
            evidenceData.OnValueChanged += OnEvidenceDataValueChanged;
    }

    private void OnEvidenceDataChange(int index, EvidenceData data)
    {
        if (IsServer)
        {
            // Adjusts the old value to hold the new values
            evidenceData.Value = new EvidenceBoardData(evidenceData.Value, index, data.evidence);
        }
        else
        {
            OnEvidenceDataChangeServerRpc(index, data.evidence); 
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnEvidenceDataChangeServerRpc(int index, bool[] data)
    {
        evidenceData.Value = new EvidenceBoardData(evidenceData.Value, index, data);
        parent.SetEvidenceData(evidenceData.Value.data);
    }
    private void OnEvidenceDataValueChanged(EvidenceBoardData previousValue, EvidenceBoardData newValue)
    {
        parent.SetEvidenceData(newValue.data);
    }

    private class EvidenceBoardData : INetworkSerializable
    {
        // Array containing entire evidence list
        public bool[] data;

        public EvidenceBoardData()
        {
            data = new bool[EvidenceTypeCount * GameController.startingEnemyCount];
        }
        public EvidenceBoardData(EvidenceBoardData eDataS, int index, bool[] eData)
        {
            data = new bool[EvidenceTypeCount * GameController.startingEnemyCount];

            // Replace the given index range
            for (int i = 0; i < data.Length; i++)
            {
                if(Mathf.FloorToInt(i / EvidenceTypeCount) == index)
                    data[i] = eData[i % EvidenceTypeCount];
                else
                    data[i] = eDataS.data[i];
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref data);
        }
    }
}
