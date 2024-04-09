using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Ekkam
{
    public class Tower : Signalable
    {
        [Header("Tower Settings")]
        public string towerName;
        public float progress;
        const float progressRequired = 100;
        public float progressSpeed = 1;
        public bool activated;
        public List<int> progressMilestonesToSpawnWaves;
        public List<int> spawnedMilestones;
        public List<Enemy> enemiesSpawned;
        public Signalable completionSignal;

        [Header("Tower References")]
        public Interactable towerButton;
        public ParticleSystem progressParticles;
        public ParticleSystem completionParticles;
        public GameObject[] enemyPrefabs;
        public PathfindingGrid enemyGrid;
        public GameObject dropShip;
        public Transform enemySpawnPoint;
        public Transform dropShipStartPoint;
        public Transform dropShipEndPoint;
        public Transform[] dropPoints;
        public GameObject entryPortal;
        public GameObject exitPortal;
        public Material progressMaterial;
        public GameObject progressIndicator;
        private UIManager uiManager;
        private Slider progressSlider;

        private void Start()
        {
            uiManager = FindObjectOfType<UIManager>();
            // Setting new instance of material so that it doesn't affect other towers
            progressMaterial = new Material(progressMaterial);
            progressIndicator.GetComponent<MeshRenderer>().material = progressMaterial;
        }

        public override void Signal()
        {
            if (activated) return;
            print("Tower activated!");
            progressSlider = uiManager.towerProgressSlider;
            progressSlider.maxValue = progressRequired;
            activated = true;
            towerButton.enabled = false;
            uiManager.pickUpPrompt.SetActive(false);
            uiManager.towerNameText.text = towerName;
            uiManager.towerUI.SetActive(true);
            progressParticles.Play();
        }
        
        private void Update()
        {
            if (!activated) return;
            
            enemiesSpawned.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeSelf);
            
            if (enemiesSpawned.Count > 0)
            {
                var main = progressParticles.main;
                main.startColor = Color.red;
                progressParticles.Pause();
                return;
            }
            if (progressParticles.isPaused)
            {
                var main = progressParticles.main;
                main.startColor = new Color(0f, 0.75f, 1f, 1);
                progressParticles.Play();
            }
            
            if (progress >= progressRequired)
            {
                print("Tower complete!");
                completionParticles.Play();
                progressParticles.Stop();
                uiManager.towerUI.SetActive(false);
                if (completionSignal != null) completionSignal.Signal();
                activated = false;
                return;
            }
            progress += progressSpeed * Time.deltaTime;
            progressMaterial.SetFloat("_FillAmount", progress / 100);
            progressSlider.value = progress;
            
            foreach (var milestone in progressMilestonesToSpawnWaves)
            {
                if (progress >= milestone && progress > 0)
                {
                    if (!spawnedMilestones.Contains(milestone))
                    {
                        spawnedMilestones.Add(milestone);
                        StartCoroutine(SpawnWave());
                    }
                }
            }
        }

        IEnumerator SpawnWave()
        {
            entryPortal.SetActive(true);
            exitPortal.SetActive(true);
            dropShip.transform.position = dropShipStartPoint.position;
            dropShip.SetActive(true);
            
            var dropPointsReached = 0;
            while (Vector3.Distance(dropShip.transform.position, dropShipEndPoint.position) > 0.1f)
            {
                dropShip.transform.position = Vector3.MoveTowards(dropShip.transform.position, dropShipEndPoint.position, 3 * Time.deltaTime);
                
                // Check if enemy spawnpoint 2d is same as droppoint 2d then spawn enemy and check for next one
                var enemySpawnPoint2DVector = new Vector3(enemySpawnPoint.position.x, 0, enemySpawnPoint.position.z);
                var dropPoint2DVector = new Vector3(dropPoints[dropPointsReached].position.x, 0, dropPoints[dropPointsReached].position.z);
                var distance = Vector3.Distance(enemySpawnPoint2DVector, dropPoint2DVector);
                distance = Mathf.Abs(distance);
                if (distance < 0.1f)
                {
                    print("distance was: " + distance + " so spawning enemy. es 2d: " + enemySpawnPoint2DVector + " dp 2d: " + dropPoint2DVector);
                    SpawnEnemy();
                    if (dropPointsReached < dropPoints.Length - 1)
                    {
                        dropPointsReached++;
                    }
                    else
                    {
                        dropPointsReached = 0;
                    }
                }
                yield return null;
            }
            dropShip.SetActive(false);
            yield return new WaitForSeconds(2);
            entryPortal.SetActive(false);
            exitPortal.SetActive(false);
        }

        private void SpawnEnemy()
        {
            var enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            var enemy = Instantiate(enemyPrefab, enemySpawnPoint.position, Quaternion.identity);
            var enemyScript = enemy.GetComponent<Enemy>();
            enemyScript.grid = enemyGrid;
            enemyScript.enabled = false;
            enemiesSpawned.Add(enemyScript);
            StartCoroutine(EnableEnemy(enemyScript));
        }
        
        IEnumerator EnableEnemy(Enemy enemy)
        {
            enemy.parachute.SetActive(true);
            yield return new WaitForSeconds(1);
            var enemyRb = enemy.GetComponent<Rigidbody>(); // Even though enemy has a rigidbody reference, it has not been assigned at this point.
            // make enemy fall slowly
            enemyRb.mass = 0.1f;
            while (enemyRb.velocity.magnitude > 0.1f)
            {
                yield return null;
            }
            enemyRb.mass = 1;
            enemy.parachute.SetActive(false);
            enemy.enabled = true;
        }
        
    }
}