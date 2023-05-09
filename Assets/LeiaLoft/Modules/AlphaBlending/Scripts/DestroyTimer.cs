using UnityEngine;

namespace LeiaLoft.Examples.Asteroids
{
    public class DestroyTimer : MonoBehaviour
    {
        [SerializeField] private float time = 0;
        void Start()
        {
            Destroy(gameObject, time);
        }
    }
}
