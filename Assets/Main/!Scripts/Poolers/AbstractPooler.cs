using UnityEngine;
using System.Collections.Generic;
using System;

using AYellowpaper.SerializedCollections;
using System.Threading.Tasks;
using Pkay.Utils;

// AbstractPooler is a generic base class for object pooling in Unity.
// TEnum: Enum type used as key for poolable types.
// TPoolable: Interface for poolable objects.
public abstract class AbstractPooler<TEnum, TPoolable> : MonoBehaviour where TEnum : Enum where TPoolable : IPoolable<TEnum>
{
    // Dictionary holding queues of poolable objects by type.
    protected readonly Dictionary<TEnum, Queue<TPoolable>> pool = new();


    // Registry mapping types to their prefab GameObjects.
    [SerializedDictionary("Type", "Prefab")]
    [SerializeField] protected SerializedDictionary<TEnum, GameObject> Registry = new();

    // Number of objects to cache per type on initialization.
    [SerializeField] int perCacheCount = 3;

    // Static event to get a poolable object.
    public static Func<TEnum, Transform, Vector3, Quaternion, TPoolable> Get
    {
        get;
        private set;
    }

    // Static event to return a poolable object to the pool.
    public static Action<TPoolable> Return
    {
        get;
        private set;
    }

    // Called when the script instance is being loaded.
    protected virtual void Awake()
    {
        InitializePoolerEvents();
        CachePoolables();
    }

    // Initializes static events for getting and returning poolables.
    private void InitializePoolerEvents()
    {
        Get += GetPoolable;
        Return += ReturnPoolable;
    }

    // Caches poolable objects for each type in the registry.
    protected virtual void CachePoolables()
    {
        foreach (var i in Registry)
        {
            TPoolable[] instances = GetFreshPoolables(i.Key, perCacheCount);
            if (!pool.ContainsKey(i.Key))
                pool.Add(i.Key, new());
            foreach (var instance in instances)
            {
                pool[i.Key].Enqueue(instance);
            }
        }
    }

    // Gets a poolable object of the specified type, creating a new one if necessary.
    protected TPoolable GetPoolable(TEnum type, Transform parent, Vector3 localPosition, Quaternion localRotation)
    {
        TPoolable poolable;
        if (!pool.ContainsKey(type) || pool[type].Count == 0)
        {
            poolable = GetFreshPoolable(type);
        }
        else
            poolable = pool[type].Dequeue();

        poolable?.OnGet(parent, localPosition, localRotation);
        return poolable;
    }

    // Returns a poolable object to the pool and restores its state.
    protected virtual void ReturnPoolable(TPoolable poolable)
    {
        poolable.OnRestore(this.transform);
        pool[poolable.GetPoolableType()].Enqueue(poolable);
    }

    // Instantiates a new poolable object of the specified type.
    protected virtual TPoolable GetFreshPoolable(TEnum type)
    {
        if (!Registry.ContainsKey(type))
        {
            Utils.Error($"Pooler Prefab for the Key {type} is not Initialized.\n Check gameObject {this.gameObject.name}");
            return default;
        }

        GameObject instance = Instantiate(Registry[type]);
        instance.transform.SetParent(this.transform);
        instance.SetActive(false);
        return instance.GetComponent<TPoolable>();
    }

    // Instantiates multiple new poolable objects of the specified type.
    protected virtual TPoolable[] GetFreshPoolables(TEnum type, int count)
    {
        TPoolable[] poolables = new TPoolable[count];

        int i = 0;

        while (i < count)
        {
            poolables[i++] = GetFreshPoolable(type);
        }

        return poolables;
    }

    // Clears all objects from the pool.
    private void ClearPool()
    {
        pool.Clear();
    }

    // Deinitializes static events for getting and returning poolables.
    private void DeinitPoolerEvents()
    {
        Get -= GetPoolable;
        Return -= ReturnPoolable;
    }

    // Called when the object becomes disabled or inactive.
    protected virtual void OnDisable()
    {
        DeinitPoolerEvents();
        ClearPool();
    }
}
