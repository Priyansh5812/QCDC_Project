using System;

public class GameScreenView : IMonoState
{
    public bool IsAlreadyTriggered { get; private set; }
    GameScreenData data;
    MenuStateController controller;
    public GameScreenView(GameScreenData data, MenuStateController controller)
    {
        this.data = data;
        this.controller = controller;

    }

    public void OnEnable(Action OnEnableCompleted = null)
    {
        OnEnableCompleted?.Invoke();
    }

    public void Start(Action OnStartCompleted = null)
    {
        IsAlreadyTriggered = true;
        OnStartCompleted?.Invoke();
    }

    public void OnDisable(Action OnDisableCompleted = null)
    {
        OnDisableCompleted?.Invoke();
    }
}
