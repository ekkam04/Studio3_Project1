using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ekkam;
using QFSW.QC;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public enum Items
    {
        Sword,
        Bow,
        Staff
    }
    
    public GameObject[] itemPrefabs;
    
    public UIManager uiManager;
    public GuideBot guideBot;
    public DialogManager dialogManager;
    public ObjectiveManager objectiveManager;
    
    public GameObject coinPrefab;
    public GameObject tokenPrefab;
    
    public delegate void OnPauseGame();
    public static event OnPauseGame onPauseGame;
    
    private List<Enemy> pausedEnemies = new List<Enemy>();
    private List<Drone> pausedDrones = new List<Drone>();
    private List<Interactable> pausedInteractables = new List<Interactable>();
    
    public delegate void OnResumeGame();
    public static event OnResumeGame onResumeGame;

    [Header("Scripted Event References")]
    public GameObject droneCrashVCam;
    public GameObject droneCrashSite;
    public GameObject droneBroken;
    public Transform[] droneCrashPath;
    public Action room1FireExtinguisherHolder;
    public Interactable room1RepairPanel;
    
    public GameObject room6BatteryHolder;
    public GameObject room6BatteryVCam;
    
    [Header("Drone Dialogs")]
    public List<Dialog> droneDialog1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        objectiveManager = FindObjectOfType<ObjectiveManager>();
        dialogManager = GetComponent<DialogManager>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        ObjectiveManager.onObjectiveComplete += HandleActionKey;
        Wire.onPowered += HandleActionKey;
        Interactable.onInteraction += HandleActionKey;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PauseGame();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        if (onPauseGame != null)
        {
            onPauseGame();
        }
        
        Player.Instance.anim.SetBool("isMoving", false);
        Player.Instance.anim.SetBool("isJumping", false);
        Player.Instance.rb.velocity = Vector3.zero;
        Player.Instance.rb.useGravity = true;
        Player.Instance.enabled = false;
        
        foreach (var enemy in FindObjectsOfType<Enemy>())
        {
            if (
                enemy.gameObject.activeSelf
                && enemy.enabled
            )
            {
                if (enemy.anim != null) enemy.anim.SetBool("isMoving", false);
                pausedEnemies.Add(enemy);
                enemy.enabled = false;
            }
        }
        
        foreach (var drone in FindObjectsOfType<Drone>())
        {
            if (
                drone.gameObject.activeSelf
                && drone.enabled
            )
            {
                pausedDrones.Add(drone);
                drone.enabled = false;
            }
        }

        foreach (var interactable in FindObjectsOfType<Interactable>())
        {
            if (
                interactable.enabled
            )
            {
                pausedInteractables.Add(interactable);
                interactable.enabled = false;
            }
        }
        uiManager.pickUpPrompt.SetActive(false);
    }
    
    public void ResumeGame()
    {
        if (onResumeGame != null)
        {
            onResumeGame();
        }
        
        Player.Instance.rb.useGravity = false;
        Player.Instance.enabled = true;
        
        foreach (var enemy in pausedEnemies)
        {
            if (enemy.gameObject.activeSelf)
            {
                enemy.enabled = true;
            }
        }
        
        foreach (var drone in pausedDrones)
        {
            if (drone.gameObject.activeSelf)
            {
                drone.enabled = true;
            }
        }
        
        foreach (var interactable in pausedInteractables)
        {
            if (interactable != null)
            {
                interactable.enabled = true;
            }
        }
        
        pausedEnemies.Clear();
        pausedDrones.Clear();
        pausedInteractables.Clear();
    }
    
    [Command("grant-item")]
    public async void GrantWeapon(Items item)
    { 
        GameObject newItem = null;
        switch (item)
        {
            case Items.Sword:
                newItem = Instantiate(itemPrefabs[0]);
                await Task.Delay(500);
                newItem.GetComponent<Interactable>().Interact();
                break;
            case Items.Bow:
                newItem = Instantiate(itemPrefabs[1]);
                await Task.Delay(500);
                newItem.GetComponent<Interactable>().Interact();
                break;
            case Items.Staff:
                newItem = Instantiate(itemPrefabs[2]);
                await Task.Delay(500);
                newItem.GetComponent<Interactable>().Interact();
                break;
            default:
                break;
        }
    }
    
    [Command("show-objectives")]
    public void ShowObjectives()
    {
        ShowGuideBot();
        uiManager.objectivesUI.SetActive(true);
    }
        
    [Command("hide-objectives")]
    public void HideObjectives()
    {
        HideObjectives();
        uiManager.objectivesUI.SetActive(false);
    }
    
    public async void HideGuideBot()
    {
        guideBot.anim.SetTrigger("hide");
        await Task.Delay(800);
        guideBot.gameObject.SetActive(false);
    }
    
    public void ShowGuideBot()
    {
        guideBot.gameObject.SetActive(true);
    }

    private void HandleActionKey(string completionActionKey)
    {
        print("GM - Objective completed - Key received: " + completionActionKey);
        uiManager.pickUpPrompt.SetActive(false);
        List<Dialog> dialogsToShow;
        
        switch (completionActionKey)
        {
            case "show-objectives":
                ShowObjectives();
                break;
            case "hide-objectives":
                HideObjectives();
                break;
            case "room1-drone-intro":
                StartCoroutine(Room1DroneIntro());
                break;
            case "room1-allow-drone-pickup":
                droneBroken.GetComponent<Interactable>().enabled = true;
                break;
            case "room1-repair-drone":
                ShowGuideBot();
                room1RepairPanel.enabled = false;
                break;
            case "room6-drop-battery":
                StartCoroutine(Room6DropBattery());
                break;
            default:
                break;
        }
    }
    
    IEnumerator Room1DroneIntro()
    {
        droneBroken.SetActive(true);
        droneCrashVCam.SetActive(true);
        droneBroken.transform.position = droneCrashPath[0].position;
        yield return new WaitForSeconds(1);
        // move along the path and slowly ramp up the speed
        float droneSpeed = 0.5f;
        for (int i = 1; i < droneCrashPath.Length; i++)
        {
            // droneBroken.transform.LookAt(droneCrashPath[i].position);
            while (Vector3.Distance(droneBroken.transform.position, droneCrashPath[i].position) > 0.1f)
            {
                droneBroken.transform.position = Vector3.MoveTowards(droneBroken.transform.position, droneCrashPath[i].position, droneSpeed * Time.deltaTime);
                droneSpeed += 0.1f;
                yield return null;
            }
        }
        droneCrashSite.SetActive(true);
        Player.Instance.TakeDamage(40, 50, null);
        yield return new WaitForSeconds(3);
        droneCrashVCam.SetActive(false);
        
        List<Dialog> dialogsToShow = new List<Dialog>
        {
            new Dialog
            {
                dialogText = "Fire hazard detected. Fire suppression protocol update required.",
                dialogOptions = new DialogOption[]
                {
                    new DialogOption
                    {
                        optionText = "Accept",
                        optionType = DialogOption.OptionType.Next
                    }
                }
            },
            new Dialog
            {
                dialogText = "Downloading fire suppression protocol update...",
                dialogOptions = new DialogOption[] {}
            },
            new Dialog
            {
                dialogText = "10%................12%................15%................20%................25%................85%................90%......................95%....................................100%",
                dialogOptions = new DialogOption[] {}
            },
            new Dialog
            {
                dialogText = "Update complete.",
                dialogOptions = new DialogOption[]
                {
                    new DialogOption
                    {
                        optionText = "Continue",
                        optionType = DialogOption.OptionType.Signal,
                        signal = room1FireExtinguisherHolder
                    }
                }
            }
        };

        PlayDroneDialog(dialogsToShow);
    }

    IEnumerator Room6DropBattery()
    {
        var room6BatteryHolderRb = room6BatteryHolder.GetComponent<Rigidbody>();
        room6BatteryHolderRb.isKinematic = false;
        room6BatteryVCam.SetActive(true);
        yield return new WaitForSeconds(2);
        room6BatteryHolderRb.AddForce(Vector3.forward * -5, ForceMode.Impulse);
        yield return new WaitForSeconds(4);
        room6BatteryHolderRb.isKinematic = true;
        room6BatteryHolder.GetComponent<Collider>().enabled = false;
        room6BatteryVCam.SetActive(false);
    }
    
    public void PlayDroneDialog(List<Dialog> dialogs, bool assignNextObjective = true, float delay = 0.5f)
    {
        StartCoroutine(PlayDroneDialogCoroutine(dialogs, assignNextObjective, delay));
    }

    IEnumerator PlayDroneDialogCoroutine(List<Dialog> dialogs, bool assignNextObjective, float delay)
    {
        PauseGame();
        yield return new WaitForSeconds(delay);
        dialogManager.dialogs = dialogs;
        dialogManager.StartDialog(0);
        yield return new WaitUntil(() => !dialogManager.isDialogActive);
        ResumeGame();
        if (assignNextObjective)
        {
            objectiveManager.AddNextObjective();
        }
    }
}
