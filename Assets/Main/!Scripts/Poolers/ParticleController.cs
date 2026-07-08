using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleController : MonoBehaviour, IPoolable<ParticleEffectType>
{   
    [SerializeField] ParticleEffectType particleType;
    ParticleSystem particleSystem;

    public ParticleSystem Comp_ParticleSystem
    {
        get
        {
            if(particleSystem == null)
            {
                particleSystem = this.GetComponent<ParticleSystem>();
            }

            return particleSystem;
        }
    }

    public bool IsActive => gameObject.activeInHierarchy;

    void OnEnable()
    {
        Comp_ParticleSystem?.Play();
        CancelInvoke(nameof(PoolBackSelf));
        Invoke(nameof(PoolBackSelf), particleSystem.main.duration);
    }

    public void PoolBackSelf()
    {
        ParticlePooler.Return?.Invoke(this);
    }

    public void OnGet(Transform parent, Vector3 localPosition, Quaternion localRotation)
    {
        this.transform.SetParent(null);
        this.transform.localPosition = localPosition;
        this.transform.localRotation = localRotation;
    }

    public async void OnRestore(Transform poolerParent)
    {   
        this.gameObject.SetActive(false);
        await UniTask.Yield();
        this.transform.SetParent(poolerParent);
    }

    public ParticleEffectType GetPoolableType()
    {
        return particleType;
    }
}
