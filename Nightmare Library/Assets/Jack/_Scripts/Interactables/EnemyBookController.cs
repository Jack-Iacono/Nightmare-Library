using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBookController : MonoBehaviour, IClickable
{
    [SerializeField]
    public EnemyPreset assignedEnemy;
    
    public static Dictionary<PlayerController, EnemyPreset> appliedBooks = new Dictionary<PlayerController, EnemyPreset>();

    public delegate void OnAppliedBookChangedDelegate(PlayerController controller, EnemyPreset preset);
    public event OnAppliedBookChangedDelegate OnAppliedBookChanged;

    public event IClickable.OnClickDelegate OnClick;

    private void Awake()
    {
        IClickable.instances.Add(gameObject, this);
        foreach(PlayerController cont in PlayerController.playerInstances.Values)
        {
            appliedBooks.Add(cont, null);
        }
    }

    public void Click()
    {
        if (NetworkConnectionController.HasAuthority)
            ChangeAppliedBook(PlayerController.mainPlayerInstance, assignedEnemy);
        else
            OnAppliedBookChanged?.Invoke(PlayerController.mainPlayerInstance, assignedEnemy);

        OnClick?.Invoke(this);
    }
    public static void ChangeAppliedBook(PlayerController player, EnemyPreset preset)
    {
        appliedBooks[player] = preset;
    }

    public void Hover(bool enterExit)
    {
        
    }

    private void OnDestroy()
    {
        IClickable.instances.Remove(gameObject);
        appliedBooks.Clear();
    }
}
