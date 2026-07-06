public interface IMonoState
{
    public bool IsAlreadyTriggered
    {
        get;
    }

    public void OnEnable(System.Action OnEnableCompleted = null);
    public void Start(System.Action OnStartCompleted = null);
    public void OnDisable(System.Action OnDisableCompleted = null);
}