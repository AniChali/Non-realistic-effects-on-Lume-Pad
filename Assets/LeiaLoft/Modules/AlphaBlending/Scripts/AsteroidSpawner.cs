using System.Collections;
using UnityEngine;

namespace LeiaLoft.Examples.Asteroids
{
    public class AsteroidSpawner : MonoBehaviour
    {
        [SerializeField] private Transform asteroidPrefab = null;
        [SerializeField] private Vector3 size = Vector3.zero;
        [SerializeField] private float spawnInterval = 1f;

        void Start()
        {
            StartSpawnTimer(spawnInterval);
        }

        void StartSpawnTimer(float waitTime)
        {
            IEnumerator timer = SpawnTimer(waitTime);
            StartCoroutine(timer);
        }
        IEnumerator SpawnTimer(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            //Spawn Asteroid
            Instantiate(asteroidPrefab, transform.position + new Vector3(
                Random.value * size.x - size.x / 2f,
                Random.value * size.y - size.y / 2f,
                Random.value * size.z - size.z / 2f),
                Quaternion.identity
            );
            StartSpawnTimer(spawnInterval);
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}