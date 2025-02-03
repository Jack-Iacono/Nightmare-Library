using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class AttackNode : BehaviorTree.Node
{
    // Allows for updating values when enemy levels up
    public virtual void UpdateValues<T>(T obj) where T : class { }
}
