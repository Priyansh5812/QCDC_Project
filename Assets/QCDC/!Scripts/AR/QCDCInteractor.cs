using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class QCDCInteractor : MonoBehaviour
{
    [SerializeField] ParticleSystem smokeParticleSystem;
    [SerializeField] PartsInfo partInfoData;
    [SerializeField] CanvasGroup indCg;
    [SerializeField] Transform indicator;
    [SerializeField] TextMeshProUGUI partName;
    int partIndex = 0;

    [field: SerializeField]
    public Animator Animator
    {
        get; private set;
    }

    bool isChangingAnimationState = false;
    [SerializeField] PartData[] parts;
    public int interactionHash = Animator.StringToHash("Interaction-1");
    public int noneHash = Animator.StringToHash("None");
    public QCDCAnimationState currState = QCDCAnimationState.INTERACTION_1;
    public void PlaySmokeParticle()
    { 
        smokeParticleSystem.Emit(30);
    }

    public async void SetAnimationState(QCDCAnimationState State , Action OnAnimationStateChanged = null)
    {
        if (isChangingAnimationState)
            return;

        isChangingAnimationState = true;


        if (State == currState)
        {
            OnAnimationStateChanged?.Invoke();
            isChangingAnimationState = false;
            return;
        }
        currState = State;
        Sequence seq = State == QCDCAnimationState.INTERACTION_2 ? ViewExploded() : ViewNormal();
        await seq.AsyncWaitForCompletion();
        OnAnimationStateChanged?.Invoke();
        isChangingAnimationState = false;
    }



    Sequence ViewExploded()
    {   
        Sequence seq = DOTween.Sequence();

        foreach (var i in parts)
        { 
            seq.Join(i.partTransform.DOLocalMove(i.ExplodedLocalPosition , 0.5f).SetEase(Ease.OutSine));
        }
        seq.Join(indCg.DOFade(1f, 0.25f).SetEase(Ease.OutSine));
        Vector3 explosionDiff = parts[partIndex].partTransform.parent.TransformPoint(parts[partIndex].ExplodedLocalPosition) - parts[partIndex].partTransform.parent.TransformPoint(parts[partIndex].InitialLocalPosition);
        indicator.position = parts[partIndex].IndicationTransform.position + explosionDiff;

        return seq;
    }

    Sequence ViewNormal()
    {
        Sequence seq = DOTween.Sequence();

        foreach (var i in parts)
        {
            seq.Join(i.partTransform.DOLocalMove(i.InitialLocalPosition, 0.5f).SetEase(Ease.OutSine));
        }

        seq.Join(indCg.DOFade(0f, 0.25f).SetEase(Ease.OutSine));

        return seq;
    }

    public void PlayAnimationFor(QCDCAnimationState animationState)
    {
        switch (animationState)
        {
            case QCDCAnimationState.INTERACTION_1:
                this.Animator.Play(interactionHash);
                break;
            case QCDCAnimationState.INTERACTION_2:
                this.Animator.Play(noneHash);
                break;
            default:

                break;
        }

        
    }

    public void OnTraversal(int modifier)
    {
        partIndex += modifier;
        
        if(partIndex < 0)
            partIndex = parts.Length-1;

        partIndex %= parts.Length;
        UpdateIndicator();
    }

    void UpdateIndicator()
    {
        if (partIndex >= 0 || partIndex < parts.Length)
        {
            indicator.position = parts[partIndex].IndicationTransform.position;
            partName?.SetText(partInfoData.descReg[parts[partIndex].partID].partName);
        }
    }

    public void PopulateDescription(TextMeshProUGUI textInfo)
    {
        int id = parts[partIndex].partID;
        if (partInfoData.descReg.ContainsKey(id))
        {
            textInfo?.SetText(partInfoData.descReg[id].description);
        }
    }

    //[ContextMenu("Set Indication Position")]
    //public void Func()
    //{
    //    for (int i = 0; i < parts.Length; i++)
    //    {
    //        if (parts[i].partTransform != null)
    //        {
    //            parts[i].IndicationTransform = parts[i].partTransform.Find("IndicationPoint");
    //        }
    //    }
    //}

    //public void OnValidate()
    //{
    //    for (int i = 0; i < parts.Length; i++)
    //    {
    //        if (parts[i].partTransform != null)
    //        {
    //            parts[i].InitialLocalPosition = parts[i].partTransform.localPosition;
    //        }
    //    }

    //    for (int i = 0; i < maleParts.Length; i++)
    //    {
    //        if (maleParts[i].partTransform != null)
    //        {
    //            maleParts[i].InitialLocalPosition = maleParts[i].partTransform.localPosition;
    //        }
    //    }
    //}

    //[ContextMenu("Set Exploded Position")]
    //public void SetExplodedPosition()
    //{
    //    for (int i = 0; i < parts.Length; i++)
    //    {
    //        if (parts[i].partTransform != null)
    //        {
    //            parts[i].ExplodedLocalPosition = parts[i].partTransform.localPosition;
    //        }
    //    }

    //    for (int i = 0; i < maleParts.Length; i++)
    //    {
    //        if (maleParts[i].partTransform != null)
    //        {
    //            maleParts[i].ExplodedLocalPosition = maleParts[i].partTransform.localPosition;
    //        }
    //    }
    //}

}

[Serializable]
public struct PartData
{
    public Transform partTransform;
    public Transform IndicationTransform;
    public Vector3 InitialLocalPosition;
    public Vector3 ExplodedLocalPosition;
    public int partID;
}

public enum QCDCAnimationState
{ 
    INTERACTION_1,
    INTERACTION_2
}
