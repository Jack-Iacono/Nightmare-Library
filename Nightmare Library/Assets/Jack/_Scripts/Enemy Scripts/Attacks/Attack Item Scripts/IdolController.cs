using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdolController : Interactable
{
    private TaskSpawnIdols idolSpawner;

    public void Initialize(TaskSpawnIdols idolSpawner)
    {
        this.idolSpawner = idolSpawner;
    }

    public override void Click()
    {
        RemoveIdol();
    }

    private void RemoveIdol()
    {
        idolSpawner.RemoveIdol();
    }
}
