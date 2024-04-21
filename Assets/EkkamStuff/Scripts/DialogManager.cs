using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using Ekkam;
using System;
using Unity.VisualScripting;

namespace Ekkam
{
    public class DialogManager : MonoBehaviour
    {
        UIManager uiManager;
        public Dialog currentDialog;
        public List<Dialog> dialogs;
        public bool lookAtEachOther = true;
        private bool lookedAtEachOther;
        public bool isDialogActive;
        
        private AudioSource audioSource;
        private float dialogSoundVolume = 0.5f;
        public AudioClip dialogSound;
        
        public delegate void OnOptionSelected(string actionKey);
        public static event OnOptionSelected onOptionSelected;

        void Start()
        {
            uiManager = FindObjectOfType<UIManager>();
            audioSource = transform.AddComponent<AudioSource>();
            audioSource.volume = dialogSoundVolume;
            audioSource.clip = dialogSound;
            audioSource.loop = true;
        }

        public void StartDialog(int dialogIndex)
        {
            isDialogActive = true;
            GameManager.Instance.PauseGame();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (lookAtEachOther && !lookedAtEachOther)
            {
                Player.Instance.transform.LookAt(transform.position, Vector3.up);
                transform.LookAt(Player.Instance.transform.position, Vector3.up);
                lookedAtEachOther = true;
            }
            
            uiManager.HideAllOptions();
            uiManager.nextButton.gameObject.SetActive(false);
            
            StartCoroutine(ShowDialogAndWaitForCompletion(dialogIndex));
        }
        
        IEnumerator ShowDialogAndWaitForCompletion(int dialogIndex)
        {
            var dialog = dialogs[dialogIndex];
            currentDialog = dialog;
            uiManager.ShowDialog(dialog.dialogText, dialog.dialogOptions.Length > 0);
            
            // audioSource.Play();
            yield return new WaitUntil(() => !uiManager.showingDialog);
            // audioSource.Stop();
            
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
            if (option.signal != null) option.signal.Signal();
            if (option.extraSignals != null)
            {
                foreach (var signal in option.extraSignals)
                {
                    signal.Signal();
                }
            }
            if (option.selectionActionKey != "" && onOptionSelected != null)
            {
                onOptionSelected(option.selectionActionKey);
            }
            
            switch (option.optionType)
            {
                case DialogOption.OptionType.Next:
                    StartDialog(dialogs.IndexOf(currentDialog) + 1);
                    break;
                case DialogOption.OptionType.End:
                    uiManager.HideDialog();
                    isDialogActive = false;
                    if (GetComponent<Interactable>() != null) GetComponent<Interactable>().enabled = true;
                    break;
                case DialogOption.OptionType.Signal:
                    // option.signal.Signal();
                    uiManager.HideDialog();
                    isDialogActive = false;
                    if (GetComponent<Interactable>() != null) GetComponent<Interactable>().enabled = true;
                    break;
                case DialogOption.OptionType.Jump:
                    StartDialog(option.jumpToIndex);
                    break;
                case DialogOption.OptionType.ModelCheck:
                    if (option.optionText == Player.Instance.currentDisguise.name)
                    {
                        StartDialog(dialogs.IndexOf(currentDialog) + 1);
                    }
                    else
                    {
                        StartDialog(dialogs.IndexOf(currentDialog) + 2);
                    }
                    break;
            }
            SoundManager.Instance.PlaySound("button-click");
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
            Jump,
            Signal,
            ModelCheck
        }

        public OptionType optionType;
        
        public int jumpToIndex;
        public Signalable signal;
        public List<Signalable> extraSignals;
        public string selectionActionKey;
    }
}
