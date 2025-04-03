using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(BasicLightController))]
public class BasicLightNetwork : NetworkBehaviour
{
    BasicLightController parent;

    private void Awake()
    {
        parent = GetComponent<BasicLightController>();

        if (NetworkConnectionController.HasAuthority)
            parent.OnInterfere += OnParentInterfere;
    }

    private void OnParentInterfere(bool fromNetwork)
    {
        if(!fromNetwork)
            OnParentInterfereClientRpc();
    }
    [ClientRpc]
    private void OnParentInterfereClientRpc()
    {
        if (!NetworkConnectionController.HasAuthority)
            parent.ElectronicInterfere();
    }
}
