using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[Serializable]
public struct Data_SpawnQCDC
{
    public QCDCInteractor qcdcPrefab;
    public XRRayInteractor interactor;
    public XRInputButtonReader inputButtonReader;
    public CanvasGroup cgMain;
}

[Serializable]
public struct Data_PosingQCDC
{   
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
    public XRInputValueReader<float> pinchDelta;
    public XRInputValueReader<float> twistDelta;
    public float twistDeltaModifier;
    public float twistMultiplier;
    public float pinchDeltaModifier;
    public float pinchMultiplier;
    public float lerpSpeed;
    public Button backBtn;
    public Button leftTraversal , rightTraversal;
    public TextMeshProUGUI partDescription;
    public Toggle tog_NormalView;
    public Toggle tog_ExplodedView;
    public CanvasGroup cgMain;
    public CanvasGroup cgInteraction_1;
    public CanvasGroup cgInteraction_2;
}

