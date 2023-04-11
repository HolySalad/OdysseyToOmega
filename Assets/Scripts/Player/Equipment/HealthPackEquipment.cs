using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SpaceBoat.PlayerSubclasses.Equipment {
    public class HealthPackEquipment : MonoBehaviour, IPlayerEquipment {
        [SerializeField] private string[] activeSprites;
        [SerializeField] private Light2D cooldownLight;
        [SerializeField] private float blenderCooldownPulseTime = 0.3f;
        public EquipmentType equipmentType {get;} = EquipmentType.HealthPack;
        public PlayerStateName usageState {get;} = PlayerStateName.staticEquipment;
        public bool isActive {get; private set;} = false;
        public bool canCancelWorkToUse {get;} = false;
        public EquipmentActivationBehaviour activationBehaviour {get;} = EquipmentActivationBehaviour.Hold;

        [SerializeField] private float usageTime = 1f;
        [SerializeField] private float cooldownTime = 15f;


        private float currentUsageTime = 0f;
        private float currentCooldownTime = 0f;
        private bool isCooldown = false;
        private int currentSprite = 0;

        private EquipmentSpriteManager spriteManager;
        void Awake() {
            spriteManager = GetComponent<EquipmentSpriteManager>();
        }

        void nextSprite() {
            currentSprite = (currentSprite + 1);
            if (currentSprite >= 2) {
                currentSprite = 0;
            }
            spriteManager.SetDisplayedSprite(activeSprites[currentSprite]);
        }

        public bool ActivationCondition(Player player) {
            return currentCooldownTime <= 0f && player.health < player.maxHealth;
        }
        public void Activate(Player player) {
            isActive = true;
            currentUsageTime = usageTime;
            nextSprite();
            SoundManager.Instance.Play("Blender", 0.5f);
        }
        public void CancelActivation(Player player) {
            isActive = false;
            if (isCooldown) {
                spriteManager.SetDisplayedSprite("BlenderEmpty");
            } 
            if (SoundManager.Instance.IsPlaying("Blender")) {
                SoundManager.Instance.Stop("Blender");
            }
        }
        public void Equip(Player player) {
            currentSprite = 0;
            spriteManager.SetDisplayedSprite(activeSprites[currentSprite]);
            //cablesSprite.enabled = false;
        }
        public void Unequip(Player player) {
            //cablesSprite.enabled = true;
        }

        IEnumerator PulseCooldownIndictator() {
            if (cooldownLight == null) yield break;
            cooldownLight.intensity = 0f;
            cooldownLight.enabled = true;
            Debug.Log("Pulsing");
            SoundManager.Instance.Play("ShieldRecharge");
            float pulseTimer = 0f;
            while (pulseTimer < blenderCooldownPulseTime) {
                if (pulseTimer < blenderCooldownPulseTime/2) {
                    cooldownLight.intensity = Mathf.Lerp(0f, 5f, pulseTimer/(blenderCooldownPulseTime/2));
                    Debug.Log("Intensity " + cooldownLight.intensity);
                } else if (pulseTimer < blenderCooldownPulseTime) {
                    cooldownLight.intensity = Mathf.Lerp(5f, 0f, (pulseTimer-(blenderCooldownPulseTime/2))/(blenderCooldownPulseTime/2));
                    Debug.Log("Intensity " + cooldownLight.intensity);
                }
                pulseTimer += Time.deltaTime;
                yield return null;
            }
            Debug.Log("Pulse Complete");
            cooldownLight.intensity = 0f;
        }

        private bool swapSpriteNextFrame = false;

        public void UpdateEquipment(Player player) {
            if (!isActive || currentCooldownTime > 0f) {
                currentCooldownTime = Mathf.Max(currentCooldownTime - Time.deltaTime, 0f);
                if (isCooldown && currentCooldownTime <= 0) {
                    isCooldown = false;
                    StartCoroutine(PulseCooldownIndictator());
                    currentSprite = 0;
                    spriteManager.SetDisplayedSprite(activeSprites[currentSprite]);
                }
            } else {
                currentUsageTime = Mathf.Max(currentUsageTime - Time.deltaTime, 0f);
                if (swapSpriteNextFrame) {                    
                    nextSprite();
                    swapSpriteNextFrame = false;
                } else {
                    swapSpriteNextFrame = true;
                }

                if (currentUsageTime <= 0f) {
                    player.Heal();
                    SoundManager.Instance.Oneshot("ActivationCompleteDing");
                    spriteManager.SetDisplayedSprite("BlenderEmpty");
                    currentCooldownTime = cooldownTime;
                    isCooldown = true;
                    player.DeactivateEquipment();
                }
            }
        }
    }
}