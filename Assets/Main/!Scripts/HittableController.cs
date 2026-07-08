using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class HittableController : MonoBehaviour , IDamagable
{   
    [SerializeField] Collider collider;
    [SerializeField] MeshRenderer mesh;
    [SerializeField] Slider healthUI;
    [SerializeField] Image fillImage;
    [SerializeField , Min(0f)] float health;
    bool isUnderKillAnimation = false;
    float currHealth;
    HittableView view;
    private static readonly int ShakeSpeedID = Shader.PropertyToID("_ShakeSpeed");
    Coroutine hitRoutine = null;

    void Start()
    {
        mesh.sharedMaterial = new Material(mesh.sharedMaterial);
    }


    public void InitializeHittable()
    {
        view ??= new(healthUI , fillImage);
        currHealth = health;
        view?.ToggleHealthUI(true);
        view?.UpdateHealthUI(1);
    }

    public void SetHittableState(HittableState state)
    {
        switch(state)
        {
            case HittableState.COLLIDABLE:
                collider.enabled = true;
                mesh.enabled = true;
                break;
            case HittableState.VISUAL_ONLY:
                collider.enabled = false;
                mesh.enabled = true;
                break;
            case HittableState.DISABLED:
                collider.enabled = false;
                mesh.enabled = false;
                break;
            default:
                break;
        }
    }

    public void ReceiveDamage(float damageAmt)
    {   
        if(isUnderKillAnimation)
            return;

        Debug.Log("Received Damage : "+this.gameObject.name);
        currHealth-= damageAmt;
        currHealth = Mathf.Max(0f , currHealth);
        view?.UpdateHealthUI(currHealth / health);

        if(currHealth == 0f)
        {
            KillEnemy();
        }
        else
        {
            HitEnemy();
        }
    }


    public void HitEnemy()
    {
        if(hitRoutine != null)
        {
            StopCoroutine(hitRoutine);
        }

        hitRoutine = StartCoroutine(HitRoutine());
    }


    public void KillEnemy()
    {
        SetHittableState(HittableState.VISUAL_ONLY);
        view?.ToggleHealthUI(false);
        view?.InitiateKillAnimation(mesh , OnKillCompleted);
    }

    void OnKillCompleted()
    {   
        Debug.Log("Kill Anim Initiated");
        SetHittableState(HittableState.DISABLED);
        var particleSystem = ParticlePooler.Get(ParticleEffectType.DESTROY_ORB, null , mesh.transform.position, Quaternion.identity) as ParticleController;

        if(particleSystem != null)
            particleSystem.gameObject.SetActive(true);
        
        mesh.transform.localPosition = Vector3.zero;
        mesh.transform.localScale = Vector3.one;
        EventManager.OnOrbKill.Invoke();
    }



    IEnumerator HitRoutine()
    {
        Vector3 startScale = Vector3.one;
        Vector3 randomScale = Vector3.one * UnityEngine.Random.Range(0.85f, 1f);
        float hitDuration = 0.25f;
        float elapsed = 0f;

        var particle = ParticlePooler.Get(ParticleEffectType.DAMAGE_ORB , null , this.transform.position , Quaternion.identity) as ParticleController;
        if(particle == null)
        {
            Debug.Log("Particle was null");
        }
        else
        {
            particle.gameObject.SetActive(true);
        }
        while (elapsed < hitDuration)
        {
            elapsed += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(elapsed / hitDuration);
            float t = Mathf.Sin(normalizedTime * Mathf.PI);

            mesh.transform.localScale = Vector3.Lerp(startScale, randomScale, t);
            mesh.sharedMaterial.SetFloat(ShakeSpeedID, t);

            yield return null;
        }

        mesh.transform.localScale = Vector3.one;
        mesh.sharedMaterial.SetFloat(ShakeSpeedID, 0f);
        hitRoutine = null;
    }


}

public enum HittableState
{
    COLLIDABLE,
    VISUAL_ONLY,
    DISABLED
}