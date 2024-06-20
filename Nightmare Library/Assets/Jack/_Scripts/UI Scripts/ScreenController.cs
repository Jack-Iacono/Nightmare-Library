using UnityEngine;

public abstract class ScreenController : MonoBehaviour
{
    
    protected Animator anim;

    [Header("Screen Controller Variables")]
    [Tooltip("The time (in seconds) that the show animation takes")]
    public float showAnimTime;
    [Tooltip("The time (in seconds) that the hide animation takes")]
    public float hideAnimTime;

    protected UIController parentController;

    public virtual void Initialize(UIController parent) 
    { 
        parentController = parent;
        anim = GetComponent<Animator>();
    }

    public virtual void ShowScreen()
    {
        gameObject.SetActive(true);
    }
    public virtual void HideScreen()
    {
        gameObject.SetActive(false);
    }
}
