using System;
using UnityEngine;
public interface IPoolable<T> where T : Enum
{   
    public bool IsActive
    {
        get;
    }
    public void PoolBackSelf();

    //-----------------------------------
    public void OnGet(Transform parent, Vector3 localPosition, Quaternion localRotation);
    public void OnRestore(Transform poolerParent);

    // ----------------------------------
    public T GetPoolableType();
}
