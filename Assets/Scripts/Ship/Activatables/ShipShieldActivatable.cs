using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SpaceBoat.Ship.Activatables {
    public class ShipShieldActivatable : MonoBehaviour, IActivatables
    {   
        [SerializeField] private UI.HelpPrompt helpPrompt;
        public UI.HelpPrompt activatableHelpPrompt {get {return helpPrompt;}}

        [SerializeField] private UI.HelpPrompt inUseHelpPrompt;
        public UI.HelpPrompt activatableInUseHelpPrompt {get {return inUseHelpPrompt;}}
        [Header("Sprites")]
        [SerializeField] private Sprite readySprite;
        [SerializeField] private Color readyColor;
        [SerializeField] private Sprite inUseSprite;
        [SerializeField] private Color inUseColor;
        [SerializeField] private Sprite cooldownSprite;
        [SerializeField] private Color cooldownColor;

        [Header("Shield")]
        [SerializeField] private float shieldFlashStrength = 5f;
        [SerializeField] private float shieldFlashDuration = 0.2f;
        [SerializeField] private float shieldInitialSize = 0.01f;
        [SerializeField] private float shieldGrowthTime = 1f;
        [SerializeField] private float shieldDuration = 12f;
        [SerializeField] private float shieldCooldown = 45f;

        public ActivatablesNames kind {get;} = ActivatablesNames.ShipShield;
        public bool isInUse {get; private set;} = false;
        public bool canManuallyDeactivate {get;} = true;
        public PlayerStateName playerState {get;} = PlayerStateName.ready;
        public string usageAnimation {get;} = "";
        public string usageSound {get;} = "ShipShieldActivate";
        
        private bool isOnCooldown = false;
        private SpriteRenderer spriteRenderer;
        private Light2D generatorLight;
        private GameObject shield;
        private Collider2D shieldCollider;

        void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            generatorLight = GetComponentInChildren<Light2D>();
            shield = GameModel.Instance.shipShield;
            shieldCollider = shield.GetComponent<Collider2D>();
        }
        public bool isShieldUp() {
            return shieldCollider.enabled;
        }
        IEnumerator EnableShield() {
            spriteRenderer.sprite = inUseSprite;
            generatorLight.color = inUseColor;
            yield return new WaitForSeconds(0.1f);
            generatorLight.intensity = 1;
            SpriteRenderer shieldRenderer = shield.GetComponent<SpriteRenderer>();
            Light2D shieldLight = shield.GetComponent<Light2D>();
            shield.transform.localScale = new Vector3(shieldInitialSize, shieldInitialSize, 1f);
            shieldRenderer.enabled = true;
            shieldLight.enabled = true;
            shieldLight.intensity = 0f;
            float timer = 0f;
            SoundManager.Instance.Play("ShipShieldActiveStatic", 0.5f, true, shieldGrowthTime);
            while (timer < shieldGrowthTime) {
                if (timer < shieldFlashDuration/2) {
                    shieldLight.intensity = Mathf.Lerp(0f, shieldFlashStrength, timer/(shieldFlashDuration/2));
                } else if (timer < shieldFlashDuration) {
                    shieldLight.intensity = Mathf.Lerp(shieldFlashStrength, 0f, (timer-(shieldFlashDuration/2))/(shieldFlashDuration/2));
                } else {
                    shieldCollider.enabled = true;
                    shieldLight.intensity = 0f;
                }
                timer += Time.deltaTime;
                shield.transform.localScale = Vector3.Lerp(new Vector3(shieldInitialSize, shieldInitialSize, 1f), Vector3.one, timer/shieldGrowthTime);
                yield return null;
            }
            shieldCollider.enabled = true;
            shieldLight.intensity = 0f;
            yield return new WaitForSeconds(shieldDuration-1f);
            SoundManager.Instance.Stop("ShipShieldActiveStatic");
            SoundManager.Instance.Play("ShipShieldWane");
            timer = 0f;
            bool flashState = false;
            while (timer < 1) {
                if (flashState) {
                    spriteRenderer.sprite = cooldownSprite;
                    generatorLight.color = cooldownColor;
                } else {
                    spriteRenderer.sprite = inUseSprite;
                    generatorLight.color = inUseColor;
                }
                flashState = !flashState;
                timer += Time.deltaTime;
                yield return new WaitForSeconds(0.2f);
            }
            spriteRenderer.sprite = cooldownSprite;
            generatorLight.color = cooldownColor;

            timer = 0f;
            while (timer < shieldGrowthTime) {
                timer += Time.deltaTime;
                shield.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(shieldInitialSize, shieldInitialSize, 1f), timer/shieldGrowthTime);
                if (timer > shieldGrowthTime-shieldFlashDuration) {
                    if (timer < shieldGrowthTime-(shieldFlashDuration/2)) {
                        shieldLight.intensity = Mathf.Lerp(0f, shieldFlashStrength, (timer-(shieldGrowthTime-shieldFlashDuration))/(shieldFlashDuration/2));
                    } else {
                        shieldLight.intensity = Mathf.Lerp(shieldFlashStrength, 0f, (timer-(shieldGrowthTime-(shieldFlashDuration/2)))/(shieldFlashDuration/2));
                    }
                } else {
                    shieldCollider.enabled = false;
                    shieldLight.intensity = 0f;
                }
                yield return null;
            }
            shieldCollider.enabled = false;
            shieldRenderer.enabled = false;
            shieldLight.enabled = false;
            yield return new WaitForSeconds(shieldCooldown);
            isOnCooldown = false;
            spriteRenderer.sprite = readySprite;
            generatorLight.color = readyColor;
        }

        public void Activate(Player player) {
            Debug.Log("Player activated ship shield equipment");
            isOnCooldown = true;
            isInUse = true;
            foreach (UsageCallback callback in usageCallbacks) {
                callback();
            }
            generatorLight.intensity = 2f;
            StartCoroutine(EnableShield());
            Deactivate(player);
            player.DetatchFromActivatable();
        }

        public void Deactivate(Player player) {
            isInUse = false;            
            foreach (UsageCallback callback in deactivationCallbacks) {
                callback();
            }
        }

        public bool ActivationCondition(Player player) {
            return !isOnCooldown;
        }

        private List<UsageCallback> usageCallbacks = new List<UsageCallback>();
        private List<UsageCallback> deactivationCallbacks = new List<UsageCallback>();
        public void AddActivationCallback(UsageCallback callback) {
            usageCallbacks.Add(callback);
        }
        public void AddDeactivationCallback(UsageCallback callback) {
            deactivationCallbacks.Add(callback);
        }

    }
}