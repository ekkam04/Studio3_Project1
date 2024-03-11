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
}
