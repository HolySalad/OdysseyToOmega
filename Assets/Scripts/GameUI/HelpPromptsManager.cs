using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SpaceBoat.UI {
    public delegate bool CustomPromptEndCondition();
    [System.Serializable] public class HelpPrompt {
        public string promptLabel;
        public int priority;
        public float duration;
        public string text;
        public Color color;
        public bool TriggerOnlyOnce;
        public CustomPromptEndCondition endCondition;
    }

    public class HelpPromptsManager : MonoBehaviour
    {
        [SerializeField] private string PromptManagerName = "DefaultHelpPromptsManager";
        [SerializeField] private float promptFadeTime = 0.25f;

        private float currentDuration = 0;
        public string currentDisplayedPrompt = "";
        public string currentTargetPrompt = "";
        public List<HelpPrompt> prompts = new List<HelpPrompt>();
        private Dictionary<string, bool> triggeredPrompts = new Dictionary<string, bool>();
        
        private TextMeshProUGUI textElement;

        void PromptManagerLog(string message) {
            Debug.Log("PromptManager/"+PromptManagerName+": " + message);
        }

        void PromptManagerLogError(string message) {
            Debug.LogError("PromptManager/"+PromptManagerName+": " + message);
        }

        void PromptManagerLogWarning(string message) {
            Debug.LogWarning("PromptManager/"+PromptManagerName+": " + message);
        }

        void Start() {
            textElement = GetComponent<TextMeshProUGUI>();
            textElement.color = new Color(textElement.color.r, textElement.color.g, textElement.color.b, 0);
            StartCoroutine(transitionRoutine());
        }

        IEnumerator transitionRoutine() {
            while (true) {    
                float opacityIncrement = 1 / promptFadeTime;
                bool isBlank = currentTargetPrompt == "";
                bool isDifferent = currentDisplayedPrompt != currentTargetPrompt;
                if ((isBlank || isDifferent) && textElement.color.a > 0) {
                    PromptManagerLog("Current displayed prompt "+ currentDisplayedPrompt + " is different from target " + (isBlank ? "Blank" : currentTargetPrompt) + ". Fading out.");
                    while (textElement.color.a > 0) {
                        float newOpacity = Mathf.Max(0, textElement.color.a - (opacityIncrement * Time.unscaledDeltaTime));
                        textElement.color = new Color(textElement.color.r, textElement.color.g, textElement.color.b, newOpacity);
                        yield return null;
                    }
                    currentDisplayedPrompt = "";
                }
                if (!isBlank && isDifferent) {
                    HelpPrompt targetPrompt = prompts.Find(p => p.promptLabel == currentTargetPrompt);
                    if (targetPrompt == null) {
                        PromptManagerLogError("Prompt transition coroutine could not find prompt with label " + currentTargetPrompt + ".");
                        yield break;
                    }
                    PromptManagerLog("Current target prompt " + currentTargetPrompt + " is different from displayed " + (currentDisplayedPrompt == "" ? "Blank" : currentDisplayedPrompt)  + ". Fading in.");
                    currentDisplayedPrompt = currentTargetPrompt;
                    textElement.text = targetPrompt.text;
                    currentDuration = targetPrompt.duration;
                    textElement.color = new Color(targetPrompt.color.r, targetPrompt.color.g, targetPrompt.color.b, textElement.color.a);
                    if (targetPrompt.TriggerOnlyOnce) {
                        triggeredPrompts[targetPrompt.promptLabel] = true;
                    }
                    bool PromptHasChanged = false;
                    while (textElement.color.a < 1 && !PromptHasChanged) {
                        float newOpacity = Mathf.Min(1, textElement.color.a + (opacityIncrement * Time.unscaledDeltaTime));
                        textElement.color = new Color(textElement.color.r, textElement.color.g, textElement.color.b, newOpacity);
                        yield return null;
                        PromptHasChanged = currentDisplayedPrompt != currentTargetPrompt;
                    }
                    PromptManagerLog("FinishedFadingIn");
                } 
                yield return null;
            }
        }


        void CheckPromptTargetting() {
            if (prompts.Count == 0) {
                if (currentTargetPrompt != "") {
                    PromptManagerLog("No prompts left. Clearing.");
                    currentTargetPrompt = "";
                }
                return;
            }
            HelpPrompt originalTargetPrompt = prompts.Find(p => p.promptLabel == currentTargetPrompt);
            if (originalTargetPrompt != null && originalTargetPrompt.endCondition != null && originalTargetPrompt.endCondition() == true) {
                PromptManagerLog("Prompt " + originalTargetPrompt.promptLabel + " has ended by delegate condition.");
                RemovePrompt(originalTargetPrompt);
                originalTargetPrompt = null;
            }
            int currentPromptPriority = originalTargetPrompt == null ? -1 : originalTargetPrompt.priority;
            string newTargetPrompt = currentTargetPrompt;
            foreach (HelpPrompt prompt in prompts) {
                bool higherPriority = prompt.priority > currentPromptPriority;
                bool canTrigger = !prompt.TriggerOnlyOnce || (triggeredPrompts[prompt.promptLabel] == false);
                if (higherPriority && canTrigger) {
                    newTargetPrompt = prompt.promptLabel;
                    currentPromptPriority = prompt.priority;
                }
            }
            if (currentPromptPriority == -1 && currentTargetPrompt != "") {
                PromptManagerLog("No prompts left. Clearing.");
                currentTargetPrompt = "";
                return;
            }
            if (currentTargetPrompt != newTargetPrompt) {
                PromptManagerLog("Prompt changed from " + currentTargetPrompt + " to " + newTargetPrompt + ".");
                currentTargetPrompt = newTargetPrompt;
                return;
            }
        }


        void OnGUI() {
            if (currentDisplayedPrompt != "" && currentDuration > 0) {
                currentDuration -= Time.unscaledDeltaTime;
                if (currentDuration <= 0) {
                    currentDuration = 0;
                    PromptManagerLog("Prompt " + currentDisplayedPrompt + " has ended by duration.");
                    RemovePrompt(prompts.Find(p => p.promptLabel == currentDisplayedPrompt));
                }
            }
            CheckPromptTargetting();
        }

        public void AddPrompt(HelpPrompt prompt) {
            string newLabel = prompt.promptLabel;
            if (newLabel == "" || newLabel == null) {
                PromptManagerLogWarning("Prompt with empty label. Ignoring.");
                return;
            }
            if (prompts.Exists(p => p.promptLabel == newLabel)) {
                //PromptManagerLogWarning("Prompt with label " + newLabel + " already exists. Ignoring.");
                return;
            }

            if (triggeredPrompts.ContainsKey(newLabel)) 
                if (prompt.TriggerOnlyOnce && triggeredPrompts[newLabel] == true)
                    return;
                else 
                    triggeredPrompts.Add(newLabel, false);
            else if (prompt.TriggerOnlyOnce)
                triggeredPrompts.Add(newLabel, false);
            prompt.text = prompt.text.Replace("\\n", "\n");
            prompts.Add(prompt);
            CheckPromptTargetting();
        }

        public void AddPrompt(HelpPrompt prompt, CustomPromptEndCondition endCondition) {
            prompt.endCondition = endCondition;
            AddPrompt(prompt);
        }

        public bool wasPromptDisplayed(string promptLabel, bool isNoLongerDisplayed = false) {
            if (!triggeredPrompts.ContainsKey(promptLabel)) {
                return false;
            }
            return triggeredPrompts[promptLabel] && (!isNoLongerDisplayed || currentTargetPrompt != promptLabel);
        }

        public void RemovePrompt(HelpPrompt prompt) {
            if (!prompts.Contains(prompt)) {
                //PromptManagerLogWarning("Prompt with label " + prompt.promptLabel + " does not exist. Ignoring.");
                return;
            }
            prompts.Remove(prompt);
            if (currentTargetPrompt == prompt.promptLabel) {
                currentTargetPrompt = "";
                CheckPromptTargetting();
            }
        }
    }
}