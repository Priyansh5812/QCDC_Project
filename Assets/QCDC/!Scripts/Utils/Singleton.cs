using System;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T m_instance = null;
    public static T Instance
    {
        get
        {
            if (m_instance != null)
            {
                return m_instance;
            }

            m_instance = FindAnyObjectByType<T>();
            if (m_instance != null)
            {   
                DontDestroyOnLoad(m_instance.gameObject);
                return m_instance;
            }

            GameObject go = new GameObject($"{typeof(T).Name} [AUTO GENERATED]");
            m_instance = go.AddComponent<T>();
            DontDestroyOnLoad(m_instance.gameObject);
            return m_instance;
            
        }
        private set
        {
            m_instance = value;
        }
    }

    protected virtual void Awake()
    { 
        EnsureSingleton();
    }

    private void EnsureSingleton()
    {
        if (m_instance == null)
        {
            m_instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (m_instance == this)
        {
            return;
        }
        else
        {
            if (m_instance != this as T)
                Destroy(this.gameObject);
        }
    }
}
