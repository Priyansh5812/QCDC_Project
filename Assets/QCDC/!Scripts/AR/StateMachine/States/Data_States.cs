using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[Serializable]
public struct Data_SpawnQCDC
{
   public QCDCInteractor qcdcPrefab;
   public XRRayInteractor interactor;
   public XRInputButtonReader inputButtonReader;
}

[Serializable]
public struct Data_PosingQCDC
{
    public XRInputValueReader<Vector2> dragDelta;
    public XRInputValueReader<float> pinchDelta;
    public float pinchDeltaModifier;
    public float dragSpeed, pinchMultiplier;
    public float lerpSpeed;
}

[Serializable]
public struct Data_QCDC_Interaction_1
{

}

[Serializable]
public struct Data_QCDC_Interaction_2
{

}
