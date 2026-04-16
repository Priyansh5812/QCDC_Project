using System;
using System.Collections.Generic;
using UnityEngine;
using Pkay.Utils;
using DG.Tweening;
using Cysharp.Threading.Tasks;
public class MenuStateController : MonoBehaviour
{
    private readonly Dictionary<Type, IMonoState> stateReg = new();
    [SerializeField] AuthScreenData authScreenData;
    [SerializeField] GameScreenData gameScreenData;
    private bool isChangingState;
    IMonoState currentState;
    private void Start()
    {
        // Target a high framerate for responsive UI. Not essential for auth, but kept as original.
        Application.targetFrameRate = 144;
        ConstructMenuStates();
        InitiateStateChange(typeof(AuthScreenView));
    }
    private void ConstructMenuStates()
    {
        stateReg.Add(typeof(AuthScreenView), new AuthScreenView(authScreenData,this));
        stateReg.Add(typeof(GameScreenView), new GameScreenView(gameScreenData,this));
    }

    public void InitiateStateChange(Type newStateType)
    {
        if (isChangingState)
        {
            Utils.Warn($"Already changing state.");
            return;
        }

        IMonoState newState;
        if (stateReg.ContainsKey(newStateType))
            newState = stateReg[newStateType];
        else
        {
            Utils.Error($"Unknown {newStateType} state type");
            return;
        }

        isChangingState = true;
        if (currentState != null)
            currentState.OnDisable(OnInitialCompleted);
        else
            OnInitialCompleted();


        void OnInitialCompleted()
        {
            currentState = newState;

            if (currentState.IsAlreadyTriggered)
                currentState.OnEnable(OnEnableCompleted);
            else
                currentState.OnEnable(OnEnableThenStart);
        }

        void OnEnableThenStart()
        {
            currentState.Start(OnStartCompleted);
        }

        void OnEnableCompleted()
        {
            isChangingState = false;
        }

        void OnStartCompleted()
        {
            isChangingState = false;
        }
    }

    private void OnDisable()
    {
        currentState?.OnDisable();
    }
}
