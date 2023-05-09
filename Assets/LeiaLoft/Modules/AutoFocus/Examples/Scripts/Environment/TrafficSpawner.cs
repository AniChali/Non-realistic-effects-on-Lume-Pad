using UnityEngine;

namespace LeiaLoft.Examples
{
    public class TrafficSpawner : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Transform carPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnInterval = 3f;
#pragma warning restore 649

        void OnEnable()
        {
            SpawnCarTimer();
        }

        public void SpawnCarTimer()
        {
            if (this.enabled)
            {
                int chosenSpawnPoint = (int)(Random.value * spawnPoints.Length);
                Instantiate(carPrefab, spawnPoints[chosenSpawnPoint].position, spawnPoints[chosenSpawnPoint].rotation);
                Invoke("SpawnCarTimer", spawnInterval);
            }
        }
    }
}