using UnityEngine;

/// <summary>
/// Simple finite-state-machine state interface used by
/// <see cref="ArenaStateController"/>. Implementers should provide logic
/// for entering, updating and exiting the state.
/// </summary>
public interface IState
{
    /// <summary>
    /// Called when the state becomes active.
    /// </summary>
    public void OnEnter();

    /// <summary>
    /// Called every frame while the state is active.
    /// </summary>
    public void OnUpdate();

    /// <summary>
    /// Called when the state is exited.
    /// </summary>
    public void OnExit();
}
