using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{
    public class AreaTrigger : MonoBehaviour
    {
        public enum TriggerAction
        {
            ShowAreaPopup,
            ShowDialog
        }
        public TriggerAction triggerAction;
        public bool oneTimeTrigger = true;
        private bool triggered;
        private UIManager uiManager;
        
        [Header("Area Popup Settings")]
        public string areaPopupText;
        public float areaPopupDuration = 3f;
        
        [Header("Dialog Settings")]
        public List<Dialog> dialogs;

        private void Start()
        {
            uiManager = FindObjectOfType<UIManager>();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !triggered)
            {
                triggered = true;
                switch (triggerAction)
                {
                    case TriggerAction.ShowAreaPopup:
                        uiManager.ShowAreaPopup(areaPopupText, areaPopupDuration);
                        break;
                    case TriggerAction.ShowDialog:
                        GameManager.Instance.PlayDroneDialog(dialogs, false);
                        break;
                }
                if (oneTimeTrigger)
                {
                    this.enabled = false;
                }
            }
        }
    }
}