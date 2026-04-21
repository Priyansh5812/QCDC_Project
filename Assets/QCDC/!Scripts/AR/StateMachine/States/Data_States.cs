using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
    public QCDCInteractor qcdcPrefab;
    public AssetReferenceGameObject qcdcAsset;
    public XRRayInteractor interactor;
    public XRInputButtonReader inputButtonReader;
    public CanvasGroup cgMain;
    public GameObject spawningOverlay;
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
    public Toggle lookRotationToggle;
}

[Serializable]
public struct Data_QCDC_Interaction
{
    // Data used in the interaction state where the user can rotate,
    // zoom and switch between normal/exploded views. Contains input
    // readers and UI elements necessary to control the interaction.
    public XRInputValueReader<float> pinchDelta;
    public XRInputValueReader<float> twistDelta;
    public float twistDeltaModifier;
    public float twistMultiplier;
    public float pinchDeltaModifier;
    public float pinchMultiplier;
    public float lerpSpeed;
    public Button backBtn;
    public Button playSim;
    public Button leftTraversal , rightTraversal;
    public TextMeshProUGUI partDescription;
    public Toggle tog_NormalView;
    public Toggle tog_ExplodedView;
    public CanvasGroup cgMain;
    public CanvasGroup cgInteraction_1;
    public CanvasGroup cgInteraction_2;
}

