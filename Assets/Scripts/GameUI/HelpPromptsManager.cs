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
        CustomPromptEndCondition endCondition;
    }

    public class HelpPromptsManager : MonoBehaviour
    {
        [SerializeField] private float promptFadeTime = 0.25f;
        public delegate bool CustomPromptEndCondition();

        private float currentDuration = 0;
        private bool isFadingOut = false;
        private bool isFadingIn = false;
        private bool isBlank = false;
        private string currentPrompt = "";
        private List<HelpPrompt> prompts = new List<HelpPrompt>();
        private Dictionary<string, bool> triggeredPrompts = new Dictionary<string, bool>();
        
        private TextMeshPro textElement;

        void Start() {
            textElement = GetComponent<TextMeshPro>();
        }

        IEnumerator transitionRoutine() {
            isFadingOut = true;
            float opacityIncrement = 1 / promptFadeTime;
            while (textElement.color.a > 0) {
                float newOpacity = Mathf.Min(1, textElement.color.a - (opacityIncrement * Time.deltaTime));
                textElement.color = new Color(textElement.color.r, textElement.color.g, textElement.color.b, newOpacity);
                yield return null;
            }
            isFadingOut = false;
            if (!isBlank) {
                isFadingIn = true;
                HelpPrompt targetPrompt = prompts.Find(p => p.promptLabel == currentPrompt);
                if (targetPrompt == null) {
                    Debug.LogWarning("Prompt transition coroutine could not find prompt with label " + currentPrompt + ".");
                    isFadingOut = false;
                    yield break;
                }
                textElement.text = targetPrompt.text;
                currentDuration = targetPrompt.duration;
                textElement.color = new Color(targetPrompt.color.r, targetPrompt.color.g, targetPrompt.color.b, 0);
                if (targetPrompt.TriggerOnlyOnce) {
                    triggeredPrompts[targetPrompt.promptLabel] = true;
                }
                while (textElement.color.a < 1) {
                    float newOpacity = Mathf.Min(1, textElement.color.a + (opacityIncrement * Time.deltaTime));
                    textElement.color = new Color(textElement.color.r, textElement.color.g, textElement.color.b, newOpacity);
                    yield return null;
                }
                isFadingIn = false;
            }
        }

        void StopTransition() {
            isFadingIn = false;
            isFadingOut = false;
            StopCoroutine(transitionRoutine());
        }

        void CheckPromptTargetting() {
            if (prompts.Count == 0) {
                currentPrompt = "";
                isBlank = true;
                if (isFadingIn) StopTransition();
                if (textElement.color.a > 0 && !isFadingOut) StartCoroutine(transitionRoutine());
                return;
            }
            HelpPrompt originalTargetPrompt = prompts.Find(p => p.promptLabel == currentPrompt);
            int currentPromptPriority = originalTargetPrompt == null ? -1 : originalTargetPrompt.priority;
            string existingPromptLabel = currentPrompt;
            foreach (HelpPrompt prompt in prompts) {
                bool higherPriority = prompt.priority > currentPromptPriority;
                bool canTrigger = !prompt.TriggerOnlyOnce || (triggeredPrompts[prompt.promptLabel] == false);
                if (higherPriority && canTrigger) {
                    currentPrompt = prompt.promptLabel;
                    currentPromptPriority = prompt.priority;
                }
            }
            if (currentPromptPriority == -1 && !isBlank) {
                currentPrompt = "";
                isBlank = true;
                if (isFadingIn) StopTransition();
                if (!isFadingOut) StartCoroutine(transitionRoutine());
            }
            if (currentPrompt != existingPromptLabel) {
                isBlank = false;
                if (!isFadingOut) {
                    if (isFadingIn) StopTransition();
                    StartCoroutine(transitionRoutine());
                }
            }
        }


        void OnGUI() {
            if (currentPrompt != "" && !isFadingIn && currentDuration > 0) {
                currentDuration -= Time.deltaTime;
                if (currentDuration <= 0) {
                    currentDuration = 0;
                    RemovePrompt(prompts.Find(p => p.promptLabel == currentPrompt));
                }
            }
            CheckPromptTargetting();
        }

        public void AddPrompt(HelpPrompt prompt) {
            string newLabel = prompt.promptLabel;
            if (newLabel == "" || newLabel == null) {
                Debug.LogWarning("Prompt with empty label. Ignoring.");
                return;
            }
            if (prompts.Exists(p => p.promptLabel == newLabel)) {
                Debug.LogWarning("Prompt with label " + newLabel + " already exists. Ignoring.");
                return;
            }

            if (triggeredPrompts.ContainsKey(newLabel)) 
                if (triggeredPrompts[newLabel] == true)
                    return;
                else 
                    triggeredPrompts.Add(newLabel, false);
            else if (prompt.TriggerOnlyOnce)
                triggeredPrompts.Add(newLabel, false);
            
            prompts.Add(prompt);
            CheckPromptTargetting();
        }

        public void RemovePrompt(HelpPrompt prompt) {
            if (!prompts.Contains(prompt)) {
                Debug.LogWarning("Prompt with label " + prompt.promptLabel + " does not exist. Ignoring.");
                return;
            }
            prompts.Remove(prompt);
            if (currentPrompt == prompt.promptLabel) {
                currentPrompt = "";
                CheckPromptTargetting();
                StartCoroutine(transitionRoutine());
            }
        }
    }
}