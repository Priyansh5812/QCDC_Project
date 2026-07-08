using UnityEngine;
using Cysharp.Threading.Tasks;
public class BulletController : MonoBehaviour, IPoolable<BulletType>
{   
    [SerializeField] BulletConfig config;
    [SerializeField] BulletType bulletType;
    [SerializeField] Rigidbody rb;
    [SerializeField] LayerMask hittableLayer;
    static readonly RaycastHit[] hits = new RaycastHit[5];
    public bool IsActive => gameObject.activeInHierarchy;
    static Camera cam;


    void Awake()
    {
        if(cam == null)
        {
            cam = Camera.main;
        }
    }



    void FixedUpdate()
    {
        Vector3 currentPosition = transform.position;
        Vector3 direction = transform.forward;
        float distance = config.bulletSpeed * Time.fixedDeltaTime;
        Vector3 nextPosition = currentPosition + direction * distance;
        rb.MovePosition(nextPosition);
        ReuseBoundCheck();
    }

    void ReuseBoundCheck()
    {
        if((this.transform.position - cam.transform.position).sqrMagnitude > (config.maxPoolbackDistance * config.maxPoolbackDistance))
        {
            PoolBackSelf();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if ((hittableLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            if(collision.gameObject.TryGetComponent<IDamagable>(out var comp))
            {
                comp.ReceiveDamage(config.damage);
            }

        }
        PoolBackSelf();
    }


    public void PoolBackSelf()
    {   
        BulletPooler.Return?.Invoke(this);
    }

    public void OnGet(Transform parent, Vector3 localPosition, Quaternion localRotation)
    {
        this.transform.SetParent(parent);
        this.transform.localPosition = localPosition;
        this.transform.localRotation = localRotation;
    }

    public async void OnRestore(Transform poolerParent)
    {   
        this.gameObject.SetActive(false);
        await UniTask.Yield();
        this.transform.SetParent(poolerParent);
    }

    public BulletType GetPoolableType()
    {
        return bulletType;
    }
}
