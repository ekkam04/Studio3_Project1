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

    [Header("Scripted Event References")]
    public GameObject droneCrashVCam;
    public GameObject droneCrashSite;
    public GameObject droneBroken;
    public Transform[] droneCrashPath;
    public Action room1FireExtinguisherHolder;
    public Interactable room1RepairPanel;

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
                print("Room 1 repair drone");
                StartCoroutine(Room1RepairDrone());
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
        Player.Instance.TakeDamage(40, this.gameObject, transform.up);
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
        dialogManager.dialogs = dialogsToShow;
        dialogManager.StartDialog(0);

        yield return new WaitUntil(() => !dialogManager.isDialogActive);
        objectiveManager.AddNextObjective();
    }

    IEnumerator Room1RepairDrone()
    {
        ShowGuideBot();
        yield return new WaitForSeconds(2);
        List<Dialog> dialogsToShow = new List<Dialog>
        {
            new Dialog // index 0
            {
                dialogText = "You have been selected for the search and extraction of anomaly Charlie.",
                dialogOptions = new DialogOption[] {}
            },
            new Dialog // index 1
            {
                dialogText = "Please make your way through this facility for Seeker Evaluation.",
                dialogOptions = new DialogOption[]
                {
                    new DialogOption
                    {
                        optionText = "Who are you?",
                        optionType = DialogOption.OptionType.Jump,
                        jumpToIndex = 2
                    },
                    new DialogOption
                    {
                        optionText = "Who am I?",
                        optionType = DialogOption.OptionType.Jump,
                        jumpToIndex = 3
                    },
                    new DialogOption
                    {
                        optionText = "What is this place?",
                        optionType = DialogOption.OptionType.Jump,
                        jumpToIndex = 4
                    },
                    new DialogOption
                    {
                        optionText = "Understood, let's go.",
                        optionType = DialogOption.OptionType.Jump,
                        jumpToIndex = 5
                    }
                }
            },
            new Dialog // index 2
            {
                dialogText = "This unit is a drone, Code Name: Seraph, This unit will provide Tactical Data on the Seeker Evaluation and Tasks given by ……. Name of the superior is ….. ACCESS DENIED.",
                dialogOptions = new DialogOption[]
                {
                    new DialogOption
                    {
                        optionText = "Continue",
                        optionType = DialogOption.OptionType.Jump,
                        jumpToIndex = 1
                    }
                }
            },
            new Dialog // index 3
            {
                dialogText = "Scanning... Unit name: Dynamo, No dread virus detected, No anomaly levels detected, Generation 3 robot, Fire suppression protocol Outlier Detected, deemed not a threat, Selected for Seeker Evaluation.",
                dialogOptions = new DialogOption[]
                {
                    new DialogOption
                    {
                        optionText = "Continue",
                        optionType = DialogOption.OptionType.Jump,
                        jumpToIndex = 1
                    }
                }
            },
            new Dialog // index 4
            {
                dialogText = "This facility is Site RC-32, previously used as a prison for anomaly type Robots, but after the Dread Virus Outbreak this facility was Abandoned.",
                dialogOptions = new DialogOption[]
                {
                    new DialogOption
                    {
                        optionText = "Continue",
                        optionType = DialogOption.OptionType.Jump,
                        jumpToIndex = 1
                    }
                }
            },
            new Dialog // index 5
            {
                dialogText = "Before we proceed, the Seraph unit has observed that an impact caused by a drone has damaged some systems, Proceed to the repair Station proudly Sponsored by Haptic Repairs, First ones on us!!",
                dialogOptions = new DialogOption[]
                {
                    new DialogOption
                    {
                        optionText = "Continue",
                        optionType = DialogOption.OptionType.End,
                        jumpToIndex = 1
                    }
                }
            },
        };
        dialogManager.dialogs = dialogsToShow;
        dialogManager.StartDialog(0);
        
        room1RepairPanel.enabled = false;
        uiManager.pickUpPrompt.SetActive(false);
        
        yield return new WaitUntil(() => !dialogManager.isDialogActive);
        objectiveManager.AddNextObjective();
    }
}
