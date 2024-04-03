using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using Ekkam;
using System;

namespace Ekkam
{
    public class DialogManager : MonoBehaviour
    {
        UIManager uiManager;
        public Dialog currentDialog;
        public List<Dialog> dialogs;

        void Start()
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        public void StartDialog(int dialogIndex)
        {
            Player.Instance.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            Player.Instance.transform.LookAt(transform.position, Vector3.up);
            transform.LookAt(Player.Instance.transform.position, Vector3.up);
            
            uiManager.HideAllOptions();
            uiManager.nextButton.gameObject.SetActive(false);
            
            var dialog = dialogs[dialogIndex];
            currentDialog = dialog;
            uiManager.ShowDialog(dialog.dialogText, dialog.dialogOptions.Length > 0);

            if (dialog.dialogOptions.Length > 0)
            {
                for (int i = 0; i < dialog.dialogOptions.Length; i++)
                {
                    var option = dialog.dialogOptions[i];
                    uiManager.optionButtons[i].onClick.RemoveAllListeners();
                    uiManager.optionButtons[i].onClick.AddListener(() => HandleOption(option));
                    uiManager.optionButtons[i].GetComponentInChildren<TMP_Text>().text = option.optionText;
                    uiManager.optionButtons[i].gameObject.SetActive(true);
                }
            }
            else
            {
                uiManager.nextButton.onClick.RemoveAllListeners();
                if (dialogIndex == dialogs.Count - 1)
                {
                    uiManager.nextButton.onClick.AddListener(() => HandleOption(new DialogOption { optionType = DialogOption.OptionType.End }));
                }
                else
                {
                    uiManager.nextButton.onClick.AddListener(() => HandleOption(new DialogOption { optionType = DialogOption.OptionType.Next }));
                }
                uiManager.nextButton.gameObject.SetActive(true);
            }
        }
        
        public void HandleOption(DialogOption option)
        {
            Player.Instance.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            switch (option.optionType)
            {
                case DialogOption.OptionType.Next:
                    StartDialog(dialogs.IndexOf(currentDialog) + 1);
                    break;
                case DialogOption.OptionType.End:
                    uiManager.HideDialog();
                    GetComponent<Interactable>().enabled = true;
                    break;
                case DialogOption.OptionType.Signal:
                    option.signal.Signal();
                    uiManager.HideDialog();
                    GetComponent<Interactable>().enabled = true;
                    break;
            }
        }
    }
    
    [Serializable]
    public struct Dialog
    {
        public string dialogText;
        public DialogOption[] dialogOptions;
    }

    [Serializable]
    public struct DialogOption
    {
        public string optionText;

        public enum OptionType
        {
            Next,
            End,
            Signal
        }

        public OptionType optionType;

        public Signalable signal;
    }
}
