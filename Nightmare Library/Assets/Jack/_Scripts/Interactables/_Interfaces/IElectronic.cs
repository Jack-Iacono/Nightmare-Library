using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IElectronic
{
    // Used for easy referencing
    public static Dictionary<GameObject, IElectronic> instances = new Dictionary<GameObject, IElectronic>();
    public void ElectronicInterfere();

    public delegate void OnInterfereDelegate(bool fromNetwork = false);
    public event OnInterfereDelegate OnInterfere;
}
