using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBoat.UI
{    public class HelpPrompts : MonoBehaviour
    {
        [SerializeField] private List<Sprite> StartupPrompts;
        [SerializeField] public Sprite pickupPrompt;
        [SerializeField] public Sprite usePrompt;
        [SerializeField] public Sprite criticalShipPrompt;
        [SerializeField] public Sprite criticalPlayerPrompt;
        [SerializeField] private float promptDuration = 4f;

        private float fadeSpeedPerFrame = 0.05f;
        private float totalFadeTime;
        private Image image;

        public bool isPromptDisplayed {get; private set;}
        private bool isFadingOut = false;

        public delegate bool PromptEndCondition();
 
        public void Start() {
            image = GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
            totalFadeTime = 1f / fadeSpeedPerFrame;

            StartCoroutine(DisplayPromptLong(StartupPrompts));
        }

        public IEnumerator DisplayPromptLong(List<Sprite> prompts) {
            foreach (Sprite prompt in prompts) {
                image.sprite = prompt;
                Color color = image.color;
                while (color.a < 1f) {
                    color.a += fadeSpeedPerFrame;
                    image.color = color;
                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForSeconds(promptDuration);
                while (color.a > 0f) {
                    color.a -= fadeSpeedPerFrame;
                    image.color = color;
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        public IEnumerator DisplayPromptLong(Sprite prompt) {
            image.sprite = prompt;
            Color color = image.color;
            while (color.a < 1f) {
                color.a += fadeSpeedPerFrame;
                image.color = color;
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(promptDuration);
            while (color.a > 0f) {
                color.a -= fadeSpeedPerFrame;
                image.color = color;
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator FadeInPrompt(Sprite prompt) {
            image.sprite = prompt;
            Color color = image.color;
            isPromptDisplayed = true;
            while (color.a < 1f) {
                color.a += fadeSpeedPerFrame;
                image.color = color;
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator FadeOutPromptAfterTimeOrCondition(PromptEndCondition DeactivationCondition) {
            float timeStarted = Time.time;
            while (Time.time - timeStarted < promptDuration+totalFadeTime && !DeactivationCondition()) {
                yield return new WaitForSeconds(0.1f);
            }
            Color color = image.color;
            isPromptDisplayed = false;
            isFadingOut = true;
            while (color.a > 0f) {
                color.a -= fadeSpeedPerFrame;
                image.color = color;
                yield return new WaitForEndOfFrame();
            }
            isFadingOut = false;
        }

        IEnumerator FadePromptInAfterNextFadesOut(Sprite prompt) { 
            Color color = image.color;
            isPromptDisplayed = true;
            isFadingOut = true;
            while (color.a > 0f) {
                color.a -= fadeSpeedPerFrame;
                image.color = color;
                yield return new WaitForEndOfFrame();
            }
            isFadingOut = false;
            StartCoroutine(FadeInPrompt(prompt));
        }

        public void DisplayPromptWithDeactivationCondition(Sprite prompt, PromptEndCondition DeactivationCondition) {
           if (isPromptDisplayed) {
               Debug.Log("Asked for a prompt when one is already present!");
           } else if (isFadingOut) {
               Debug.Log("Asked for a prompt when one is fading out!");
               FadePromptInAfterNextFadesOut(prompt);
           } else {
               StartCoroutine(FadeInPrompt(prompt));
               StartCoroutine(FadeOutPromptAfterTimeOrCondition(DeactivationCondition));
           }
        }
    }
}