using System.Collections.Generic;
using UnityEngine;

public abstract class UIController : MonoBehaviour
{
    public static UIController mainInstance;

    public List<ScreenController> screens = new List<ScreenController>();
    protected int currentScreen = -1;
    protected int nextScreen;

    public delegate void OnScreenIndexChangeDelegate(int index);
    public event OnScreenIndexChangeDelegate OnScreenIndexChange;

    public delegate void OnStartFinishDelegate();
    public event OnStartFinishDelegate OnStartFinish;

    protected virtual void Awake()
    {
        if (mainInstance != null)
            Destroy(mainInstance);

        mainInstance = this;
    }
    protected virtual void Start()
    {
        // Initializes every screen and hides all but the currently active screen
        for (int i = 0; i < screens.Count; i++)
        {
            screens[i].Initialize(this);

            if (i == 0)
                screens[i].ShowScreen();
            else
                screens[i].HideScreen();
        }

        ChangeToScreen(0);

        OnStartFinish?.Invoke();
    }

    /// <summary>
    /// Changes the currently active UI screen
    /// </summary>
    /// <param name="i">The index of the screen to change to</param>
    public void ChangeToScreen(int i)
    {
        nextScreen = i;

        // Ensures that the current screen isn't null
        if (currentScreen != -1)
        {
            // Hides the previous screen and shows the new screen
            screens[currentScreen].HideScreen();
        }

        // Used to hide all screens
        if(i != -1)
            screens[nextScreen].ShowScreen();

        // sets the current screen to the new screen
        currentScreen = nextScreen;
        nextScreen = -1;

        OnScreenIndexChange?.Invoke(currentScreen);
    }
    public void NextScreen()
    {
        ChangeToScreen((currentScreen + 1) % screens.Count);
    }
    public void PreviousScreen()
    {
        int index = (currentScreen - 1) % screens.Count;
        ChangeToScreen(index < 0 ? screens.Count - 1 : index);
    }

    #region Events

    protected virtual void OnGamePause(object sender, bool e)
    {

    }

    #endregion
}
