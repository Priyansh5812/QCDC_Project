using System;

public static class EventManager
{   
    public static ActionEvent OnWaveCompleted
    {
        get;
        private set;
    } = new();

    public static ActionEvent OnGameRestart
    {
        get; private set;
    } = new();

    public static ActionEvent OnOrbKill
    {
        get;
        private set;
    } = new();    
    
    public static ActionEvent PostOrbKill
    {
        get;
        private set;
    } = new();


    public static FuncEvent<float> GetScore
    {
        get;
        private set;
    } = new();
}

public class ActionEvent
{
    private event Action baseAction;
    public void Invoke() => baseAction?.Invoke();
    public void AddListener(Action action) => baseAction += action;
    public void RemoveListener(Action action) => baseAction -= action;
}

public class FuncEvent<T1>
{
    private event Func<T1> baseFunc;
    public T1 Invoke() => baseFunc.Invoke();
    public void AddListener(Func<T1> action) => baseFunc += action;
    public void RemoveListener(Func<T1> action) => baseFunc -= action;

}



