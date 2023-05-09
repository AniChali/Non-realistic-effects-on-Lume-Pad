using UnityEngine;
using UnityEngine.Events;


namespace LeiaLoft.Examples.Asteroids
{
    public class ProjectileCollision : MonoBehaviour
    {
        [SerializeField] private UnityEvent action = null;

        void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.GetComponent<Projectile>())
            {
                action.Invoke();
                Destroy(other.gameObject);
            }
        }
    }
}