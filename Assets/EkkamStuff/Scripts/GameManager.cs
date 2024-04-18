using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ekkam;
using QFSW.QC;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

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

    public Camera noPostCam;
    public Volume globalVolume;
    public Volume darknessVolume;
    
    public delegate void OnResumeGame();
    public static event OnResumeGame onResumeGame;

    [Header("Scripted Event References")]
    public GameObject droneCrashVCam;
    public GameObject droneCrashSite;
    public GameObject droneBroken;
    public Transform[] droneCrashPath;
    public Action room1FireExtinguisherHolder;
    public Interactable room1RepairPanel;

    public GameObject room6ExplosionVCam;
    public GameObject room6ExplosionFire;
    public Action room6FireExtinguisherHolder;
    public GameObject room6BatteryHolder;
    public GameObject room6BatteryVCam;
    public Action room6Elevator;
    public float room6ElevatorGroundYPosition;
    public Action room6ElevatorUseButtonHolder;
    
    public GameObject hovercraftVCam;
    public GameObject hovercraft;
    public Transform[] hovercraftPath;
    
    [Header("Drone Dialogs")]
    public List<Dialog> droneDialog1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        DialogManager.onOptionSelected += HandleActionKey;
        Wire.onPowered += HandleActionKey;
        Interactable.onInteraction += HandleActionKey;
        
        // Application.targetFrameRate = 60;
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
        
        if (Player.Instance != null) Player.Instance.rb.useGravity = false;
        if (Player.Instance != null) Player.Instance.enabled = true;
        
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

    [Command("handle-action-key")]
    private void HandleActionKey(string completionActionKey)
    {
        print("GM - Objective completed - Key received: " + completionActionKey);
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
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
            case "room6-door-explosion":
                StartCoroutine(Room6DoorExplosion());
                break;
            case "room6-drop-battery":
                StartCoroutine(Room6DropBattery());
                break;
            case "room6-elevator-reset":
                room6Elevator.ResetSequence();
                room6ElevatorUseButtonHolder.Signal();
                room6Elevator.MoveToWorldPosition(
                    new Vector3(room6Elevator.transform.position.x, room6ElevatorGroundYPosition, room6Elevator.transform.position.z),
                    2
                );
                break;
            case "toggle-disguise":
                Player.Instance.ToggleDisguise();
                break;
            case "send-hovercraft":
                StartCoroutine(SendHovercraft());
                break;
            case "ride-hovercraft":
                StartCoroutine(RideHovercraft());
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
    
    IEnumerator Room6DoorExplosion()
    {
        room6ExplosionVCam.SetActive(true);
        yield return new WaitForSeconds(2);
        room6ExplosionFire.SetActive(true);
        Player.Instance.TakeDamage(20, 50, null);
        yield return new WaitForSeconds(4);
        room6ExplosionVCam.SetActive(false);

        List<Dialog> dialogsToShow = new List<Dialog>
        {
            new Dialog
            {
                dialogText = "Fire hazard detected. Fire suppression protocol activated.",
                dialogOptions = new DialogOption[]
                {
                    new DialogOption
                    {
                        optionText = "Accept",
                        optionType = DialogOption.OptionType.Signal,
                        signal = room6FireExtinguisherHolder
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

    IEnumerator SendHovercraft()
    {
        hovercraftVCam.SetActive(true);
        
        float hovercraftSpeed = 5f;
        float hovercraftRotation = hovercraft.transform.rotation.eulerAngles.y;
        float hovercraftTargetRotation = 90;
        for (int i = 1; i < hovercraftPath.Length; i++)
        {
            while (Vector3.Distance(hovercraft.transform.position, hovercraftPath[i].position) > 0.1f)
            {
                hovercraft.transform.position = Vector3.MoveTowards(hovercraft.transform.position, hovercraftPath[i].position, hovercraftSpeed * Time.deltaTime);
                hovercraftRotation = Mathf.Lerp(hovercraftRotation, hovercraftTargetRotation, hovercraftSpeed * Time.deltaTime);
                hovercraft.transform.rotation = Quaternion.Euler(0, hovercraftRotation, 0);
                yield return null;
            }
        }
        
        hovercraftVCam.SetActive(false);
        objectiveManager.AddNextObjective();
    }

    IEnumerator RideHovercraft()
    {
        hovercraftVCam.SetActive(true);
        PauseGame();
        
        float hovercraftSpeed = 5f;
        float hovercraftRotation = hovercraft.transform.rotation.eulerAngles.y;
        float hovercraftTargetRotation = 0;
        for (int i = hovercraftPath.Length - 1; i >= 0; i--)
        {
            while (Vector3.Distance(hovercraft.transform.position, hovercraftPath[i].position) > 0.1f)
            {
                hovercraft.transform.position = Vector3.MoveTowards(hovercraft.transform.position, hovercraftPath[i].position, hovercraftSpeed * Time.deltaTime);
                hovercraftRotation = Mathf.Lerp(hovercraftRotation, hovercraftTargetRotation, hovercraftSpeed * Time.deltaTime);
                hovercraft.transform.rotation = Quaternion.Euler(0, hovercraftRotation, 0);
                yield return null;
            }
        }
        
        hovercraftVCam.SetActive(false);
        ResumeGame();
        objectiveManager.AddNextObjective();
    }
    
    public void PlayDroneDialog(List<Dialog> dialogs, bool assignNextObjective = true, float delay = 0.5f)
    {
        StartCoroutine(PlayDroneDialogCoroutine(dialogs, assignNextObjective, delay));
    }

    IEnumerator PlayDroneDialogCoroutine(List<Dialog> dialogs, bool assignNextObjective, float delay)
    {
        PauseGame();
        guideBot.SwitchToTalking();
        yield return new WaitForSeconds(delay);
        dialogManager.dialogs = dialogs;
        dialogManager.StartDialog(0);
        yield return new WaitUntil(() => !dialogManager.isDialogActive);
        ResumeGame();
        guideBot.SwitchToFollowing();
        if (assignNextObjective)
        {
            objectiveManager.AddNextObjective();
        }
    }
    
    [Command("enable-darkness")]
    public void EnableDarkness()
    {
        noPostCam.gameObject.SetActive(true);
        StartCoroutine(LerpVolume(darknessVolume, 0.75f, 1));
    }
    
    [Command("disable-darkness")]
    public void DisableDarkness()
    {
        StartCoroutine(LerpVolume(darknessVolume, 0.75f, 0));
        Invoke("DisableNoPostCam", 0.75f);
    }
    
    private void DisableNoPostCam()
    {
        noPostCam.gameObject.SetActive(false);
    }
    
    IEnumerator LerpVolume(Volume volume, float duration, float targetValue)
    {
        float timeElapsed = 0;
        float startValue = volume.weight;
        while (timeElapsed < duration)
        {
            volume.weight = Mathf.Lerp(startValue, targetValue, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        volume.weight = targetValue;
    }
}
