using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject inventoryUI;
    public GameObject pickUpPrompt;
    public GameObject targetLockPrompt;
    
    public GameObject objectivesUI;
    
    public GameObject dialogUI;
    public TMP_Text dialogText;
    public GameObject dialogCamera;

    public Button nextButton;
    public Button[] optionButtons;

    private void Start()
    {
        pickUpPrompt.SetActive(false);
        dialogUI.SetActive(false);
    }

    public async void ShowDialog(string dialog, bool showNextButton = false)
    {
        inventoryUI.SetActive(false);
        dialogCamera.SetActive(true);
        dialogText.text = "";
        dialogUI.SetActive(true);
        foreach (var letter in dialog)
        {
            dialogText.text += letter;
            await Task.Delay(50);
        }
    }
    
    public void HideDialog()
    {
        inventoryUI.SetActive(true);
        dialogUI.SetActive(false);
        dialogText.text = "";
        dialogCamera.SetActive(false);
    }
    
    public void HideAllOptions()
    {
        foreach (var button in optionButtons)
        {
            button.gameObject.SetActive(false);
        }
    }
}
