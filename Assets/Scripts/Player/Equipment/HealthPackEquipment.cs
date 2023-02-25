using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SpaceBoat.PlayerSubclasses.Equipment {
    public class HealthPackEquipment : MonoBehaviour, IPlayerEquipment {
        [SerializeField] private SpriteRenderer[] activeSprite;
        [SerializeField] private SpriteRenderer cooldownSprite;
        [SerializeField] private Light2D cooldownLight;
        [SerializeField] private float blenderCooldownPulseTime = 0.3f;
        [SerializeField] private SpriteRenderer cablesSprite;
        public EquipmentType equipmentType {get;} = EquipmentType.HealthPack;
        public PlayerStateName usageState {get;} = PlayerStateName.staticEquipment;
        public bool isActive {get; private set;} = false;
        public bool canCancelWorkToUse {get;} = false;
        public EquipmentActivationBehaviour activationBehaviour {get;} = EquipmentActivationBehaviour.Toggle;

        [SerializeField] private float usageTime = 1f;
        [SerializeField] private float cooldownTime = 15f;


        private float currentUsageTime = 0f;
        private float currentCooldownTime = 0f;
        private bool isCooldown = false;
        private int currentSprite = 0;

        public bool ActivationCondition(Player player) {
            return currentCooldownTime <= 0f && player.health < player.maxHealth;
        }
        public void Activate(Player player) {
            isActive = true;
            currentUsageTime = usageTime;
            activeSprite[currentSprite].enabled = false;
            SoundManager.Instance.Play("Blender", 0.5f);
        }
        public void CancelActivation(Player player) {
            isActive = false;
            if (isCooldown) {
                cooldownSprite.enabled = true;
                activeSprite[currentSprite].enabled = false;
            } 
            if (SoundManager.Instance.IsPlaying("Blender")) {
                SoundManager.Instance.Stop("Blender");
            }
        }
        public void Equip(Player player) {
            activeSprite[currentSprite].enabled = true;
            //cablesSprite.enabled = false;
        }
        public void Unequip(Player player) {
            activeSprite[currentSprite].enabled = false;
            cooldownSprite.enabled = false;
            cablesSprite.enabled = true;
        }

        IEnumerator PulseCooldownIndictator() {
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
                    cooldownSprite.enabled = false;
                    activeSprite[0].enabled = true;
                    currentSprite = 0;
                }
            } else {
                currentUsageTime = Mathf.Max(currentUsageTime - Time.deltaTime, 0f);                    int nextSprite = (currentSprite + 1);
                if (swapSpriteNextFrame) {
                    if (nextSprite >= activeSprite.Length) {
                        nextSprite = 0;
                    }
                    activeSprite[currentSprite].enabled = false;
                    activeSprite[nextSprite].enabled = true;
                    currentSprite = nextSprite;
                    swapSpriteNextFrame = false;
                } else {
                    swapSpriteNextFrame = true;
                }

                if (currentUsageTime <= 0f) {
                    player.PlayerHeals();
                    activeSprite[currentSprite].enabled = false;
                    cooldownSprite.enabled = true;
                    currentCooldownTime = cooldownTime;
                    isCooldown = true;
                    player.DeactivateEquipment();
                }
            }
        }
    }
}