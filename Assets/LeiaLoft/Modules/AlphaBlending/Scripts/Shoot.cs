using UnityEngine;

namespace LeiaLoft.Examples.Asteroids
{
    public class Shoot : MonoBehaviour
    {
        [SerializeField] private Transform projectilePrefab = null;
        [SerializeField] private Transform[] spawnPoint = null;
        int currentSpawnPoint = 0;
        [SerializeField] private float interval = .3f;
        float timer = 0;

        public void Fire()
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                Instantiate(projectilePrefab, spawnPoint[currentSpawnPoint].position, Quaternion.Euler(spawnPoint[currentSpawnPoint].forward));
                timer = interval;
                currentSpawnPoint++;
                if (currentSpawnPoint >= spawnPoint.Length)
                {
                    currentSpawnPoint = 0;
                }
            }
        }
    }
}