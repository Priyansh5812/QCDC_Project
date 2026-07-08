using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


[Serializable]
public struct Data_SpawnQCDC
{
    // Data required by the spawn state. Contains references to either a
    // direct prefab or an addressable asset, the AR interactor used for
    // raycasts, input readers and UI elements.
    public ArenaSpawner arenaPrefab;
    public ARPlaneManager planeManager;
    public XRRayInteractor interactor;
    public XRInputButtonReader inputButtonReader;
    public CanvasGroup cgMain;
    public Button btn_confirmPlacement;
    public TextMeshProUGUI prompt;
    public string msg_ScanForArea, msg_ClicktoSpawn;
}

[Serializable]
public struct Data_PosingQCDC
{
    // Data used while placing and posing the QCDC in the AR scene. This
    // includes plane manager, input readers for drag/pinch/twist gestures
    // as well as UI controls and tuning parameters.
    public ARPlaneManager planeManager;
    public XRInputValueReader<Vector2> dragDelta;
    public XRInputValueReader<float> pinchDelta;
    public XRInputValueReader<float> twistDelta;
    public float pinchDeltaModifier;
    public float dragSpeed, pinchMultiplier;
    public float twistDeltaModifier, twistMultiplier;
    public float lerpSpeed;
    public CanvasGroup cgMain;
    public Button btn_finalizePosition;
}

[Serializable]
public struct Data_Gameplay
{
    public TextMeshProUGUI score;
    public Button btn_fire;
    public CanvasGroup cgMain;
}

[Serializable]
public struct Data_GameEnd
{
    public TextMeshProUGUI score;
    public Button btn_playAgain;
    public CanvasGroup cgMain;
}