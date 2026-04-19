using DG.Tweening;
using System;
using UnityEngine;

public class QCDCInteractor : MonoBehaviour
{
    [SerializeField] ParticleSystem smokeParticleSystem;
    [field: SerializeField]
    public Animator Animator
    {
        get; private set;
    }

    bool isChangingAnimationState = false;
    [SerializeField] PartData[] femaleParts;
    [SerializeField] PartData[] maleParts;

    public void PlaySmokeParticle()
    { 
        smokeParticleSystem.Emit(30);
    }

    public async void SetAnimationState(QCDCAnimationState State , Action OnAnimationStateChanged = null)
    {
        if (isChangingAnimationState)
            return;

        isChangingAnimationState = true;

        float targetValue;
        float currValue = Animator.GetFloat("BlendKey");
        switch (State)
        {
            case QCDCAnimationState.INTERACTION_1:
                targetValue = 0;
                break;

            case QCDCAnimationState.INTERACTION_2:
                targetValue = 1;
                break;
            
            default:
                targetValue = 0;
                break;  
        }

        if (targetValue == currValue)
        {
            OnAnimationStateChanged?.Invoke();
            isChangingAnimationState = false;
            return;
        }

        await DOVirtual.Float(currValue, targetValue, 0.5f, UpdateBlendKeyValue).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        Sequence seq = targetValue == 1 ? ViewExploded() : ViewNormal();
        await seq.AsyncWaitForCompletion();
        OnAnimationStateChanged?.Invoke();
        isChangingAnimationState = false;
    }

    void UpdateBlendKeyValue(float value) => Animator.SetFloat("BlendKey", value);

    Sequence ViewExploded()
    {   
        Sequence seq = DOTween.Sequence();

        foreach (var i in femaleParts)
        { 
            seq.Join(i.partTransform.DOLocalMove(i.ExplodedLocalPosition , 0.5f).SetEase(Ease.OutSine));
        }

        foreach (var i in maleParts)
        { 
            seq.Join(i.partTransform.DOLocalMove(i.ExplodedLocalPosition , 0.5f).SetEase(Ease.OutSine));
        }

        return seq;
    }

    Sequence ViewNormal()
    {
        Sequence seq = DOTween.Sequence();

        foreach (var i in femaleParts)
        {
            seq.Join(i.partTransform.DOLocalMove(i.InitialLocalPosition, 0.5f).SetEase(Ease.OutSine));
        }

        foreach (var i in maleParts)
        {
            seq.Join(i.partTransform.DOLocalMove(i.InitialLocalPosition, 0.5f).SetEase(Ease.OutSine));
        }

        return seq;
    }

    //public void OnValidate()
    //{
    //    for (int i = 0; i < femaleParts.Length; i++)
    //    {
    //        if (femaleParts[i].partTransform != null)
    //        {
    //            femaleParts[i].InitialLocalPosition = femaleParts[i].partTransform.localPosition;
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
    //    for (int i = 0; i < femaleParts.Length; i++)
    //    {
    //        if (femaleParts[i].partTransform != null)
    //        {
    //            femaleParts[i].ExplodedLocalPosition = femaleParts[i].partTransform.localPosition;
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
    public Vector3 InitialLocalPosition;
    public Vector3 ExplodedLocalPosition;
}

public enum QCDCAnimationState
{ 
    INTERACTION_1,
    INTERACTION_2
}
