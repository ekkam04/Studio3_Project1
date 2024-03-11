using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ekkam;
using QFSW.QC;
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
        guideBot = FindObjectOfType<GuideBot>();
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
}
