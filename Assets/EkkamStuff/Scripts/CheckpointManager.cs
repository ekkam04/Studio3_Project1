using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using QFSW.QC;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ekkam
{
    public class CheckpointManager : MonoBehaviour
    {
        public static CheckpointManager Instance;
        public CheckpointData currentCheckpointData;
        public Transform savedItemsParent;

        private ObjectiveManager objectiveManager;
        private UIManager uiManager;
        private Inventory inventory;
        
        [Header("--- Levels ---")]
        public CheckpointData prologueCheckpointData;
        public CheckpointData theTowerCheckpointData;
        public CheckpointData theDeceptionCheckpointData;
        public CheckpointData theGarageCheckpointData;

        private void Awake()
        {
            AssignReferences();
            
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SaveTempCheckpoint();
                Invoke(nameof(InitializeObjective), 2);
                Invoke(nameof(OnCheckpointLoaded), 2.25f);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void AssignReferences()
        {
            objectiveManager = FindObjectOfType<ObjectiveManager>();
            inventory = FindObjectOfType<Inventory>();
            uiManager = FindObjectOfType<UIManager>();
            uiManager.prologueButton.onClick.AddListener(LoadPrologueCheckpoint);
            uiManager.theTowerButton.onClick.AddListener(LoadTheTowerCheckpoint);
            uiManager.theDeceptionButton.onClick.AddListener(LoadTheDeceptionCheckpoint);
            uiManager.theGarageButton.onClick.AddListener(LoadTheGarageCheckpoint);
        }

        [Command("save-checkpoint")]
        public void SaveTempCheckpoint()
        {
            var player = FindObjectOfType<Player>();
            SaveCheckpointData(player.transform.position, player.transform.eulerAngles);
        }
        
        public void SaveCheckpointData(Vector3 position, Vector3 rotation)
        {
            var player = FindObjectOfType<Player>();
            currentCheckpointData = new CheckpointData
            {
                position = position,
                rotation = rotation,
                health = player.health,
                items = new List<GameObject>(),
                coins = player.coins,
                tokens = player.tokens,
                objectiveIndex = objectiveManager.currentObjectiveIndex
            };
            var savedItems = savedItemsParent.GetComponentsInChildren<Item>();
            foreach (Item item in savedItems)
            {
                Destroy(item.gameObject);
            }
            foreach (Item item in inventory.items)
            {
                var newItemGO = Instantiate(item.gameObject, savedItemsParent.transform);
                currentCheckpointData.items.Add(newItemGO);
            }
        }
        
        public void LoadCheckpointData()
        {
            StartCoroutine(LoadCheckpoint(currentCheckpointData));
        }
        
        IEnumerator LoadCheckpoint(CheckpointData checkpointData)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            print("Scene reload complete");
            AssignReferences();
            
            Player.Instance.transform.position = checkpointData.position;
            Player.Instance.transform.eulerAngles = checkpointData.rotation;
            Player.Instance.health = checkpointData.health;
            Player.Instance.coins = checkpointData.coins;
            Player.Instance.tokens = checkpointData.tokens;

            yield return new WaitForSeconds(0.25f);
            print("Restoring " + currentCheckpointData.items.Count + " items to inventory...");
            foreach (GameObject go in currentCheckpointData.items)
            {
                print("Restoring " + go.gameObject.name + " to inventory...");
                if (go.GetComponent<Interactable>() != null)
                {
                    go.GetComponent<Interactable>().Interact();
                    yield return new WaitForSeconds(0.5f);
                }
            }
            currentCheckpointData.items.Clear();
            
            yield return new WaitForSeconds(0.25f);
            foreach (var item in inventory.items)
            {
                var newItemGO = Instantiate(item.gameObject, savedItemsParent.transform);
                newItemGO.SetActive(false);
                currentCheckpointData.items.Add(newItemGO);
            }
            
            objectiveManager.currentObjectiveIndex = checkpointData.objectiveIndex;
            InitializeObjective();

            OnCheckpointLoaded();
        }
        
        private void InitializeObjective()
        {
            if (currentCheckpointData.objectiveIndex > 38)
            {
                GameManager.Instance.objectiveManager.AddObjectiveToUI(GameManager.Instance.finalObjectiveVisual);
            }
            objectiveManager.InitializeFromCurrentIndex();
        }
        
        private async void OnCheckpointLoaded()
        {
            if (objectiveManager.currentObjectiveIndex > 6)
            {
                GameManager.Instance.ShowGuideBot();
            }
            if (objectiveManager.currentObjectiveIndex > 16)
            {
                GameManager.Instance.GrantWeapon(GameManager.Items.Staff);
                await Task.Delay(1000);
            }
            if (objectiveManager.currentObjectiveIndex > 35)
            {
                GameManager.Instance.GrantWeapon(GameManager.Items.Bow);
                await Task.Delay(1000);
            }
            if (objectiveManager.currentObjectiveIndex > 9)
            {
                GameManager.Instance.GrantWeapon(GameManager.Items.Sword);
                await Task.Delay(1000);
            }
            if (objectiveManager.currentObjectiveIndex > 38)
            {
                Player.Instance.SwitchDisguise(1);
            }
        }
        
        public void LoadPrologueCheckpoint()
        {
            StartCoroutine(LoadCheckpoint(prologueCheckpointData));
        }
        
        public void LoadTheTowerCheckpoint()
        {
            StartCoroutine(LoadCheckpoint(theTowerCheckpointData));
        }
        
        public void LoadTheDeceptionCheckpoint()
        {
            StartCoroutine(LoadCheckpoint(theDeceptionCheckpointData));
        }
        
        public void LoadTheGarageCheckpoint()
        {
            StartCoroutine(LoadCheckpoint(theGarageCheckpointData));
        }
    }
    
    [System.Serializable]
    public class CheckpointData
    {
        public Vector3 position;
        public Vector3 rotation;
        public int health;
        public List<GameObject> items;
        public int coins;
        public int tokens;
        public int objectiveIndex;
    }
}