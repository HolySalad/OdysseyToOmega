using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.UI;
using SpaceBoat.Ship.Activatables;

namespace SpaceBoat.Environment {

    public class HelpPromptTrigger : MonoBehaviour
    {
        [SerializeField] private HelpPrompt helpPrompt;
        [SerializeField] private bool requireUsableActivatable = false;

        private bool supressTrigger = false;
        private bool displayingPrompt = false;

        void Start() {
            if (requireUsableActivatable) {
                IActivatables activatables = this.GetComponent<IActivatables>();
                if (activatables == null) {
                    Debug.LogError("HelpPromptTrigger: Require usable activatable is set, but no IActivatables component found on " + this.gameObject.name);
                    Destroy(this);
                    return;
                }
                activatables.AddActivationCallback(() => {
                    supressTrigger = true;
                    GameModel.Instance.helpPrompts.RemovePrompt(helpPrompt);
                    displayingPrompt = false;
                });
                activatables.AddDeactivationCallback(() => {
                    supressTrigger = false;
                });
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (supressTrigger) return;
            if (other.gameObject.GetComponent<Player>() != null)
            {
                if (requireUsableActivatable) {
                    IActivatables activatables = this.GetComponent<IActivatables>();
                    if (activatables != null && activatables.ActivationCondition(other.GetComponent<Player>())) {
                        GameModel.Instance.helpPrompts.AddPrompt(helpPrompt);
                        displayingPrompt = true;
                    }
                } else {
                    GameModel.Instance.helpPrompts.AddPrompt(helpPrompt);
                    displayingPrompt = true;
                }
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!displayingPrompt) return;
            if (other.gameObject.GetComponent<Player>() != null)
            {
                GameModel.Instance.helpPrompts.RemovePrompt(helpPrompt);
                displayingPrompt = false;
                if (helpPrompt.TriggerOnlyOnce) Destroy(gameObject);
            }
        }
    }
}