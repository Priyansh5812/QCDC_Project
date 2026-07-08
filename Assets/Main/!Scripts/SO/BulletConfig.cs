using UnityEngine;

[CreateAssetMenu(fileName = "BulletConfig", menuName = "Scriptable Objects/Bullet/BulletConfig")]
public class BulletConfig : ScriptableObject
{
    public float damage;
    public float bulletSpeed;
    public float maxPoolbackDistance = 5;
}
