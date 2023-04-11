using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SpaceBoat.PlayerSubclasses.Equipment {
    public class ShieldEquipment : MonoBehaviour, IPlayerEquipment {
        [SerializeField] public GameObject shieldObject;
        [SerializeField] public Light2D cooldownLight;
        [SerializeField] private float shieldMaxDuration = 3f;
        [SerializeField] private float shieldRecoveryTime = 6f;
        [SerializeField] private float shieldBreakRecoveryPenalty = 4f;
        [SerializeField] private float shieldMinStrengthToUse = 0.34f;
        [SerializeField] private float shieldTogglePenalty = 0.1f;
        [SerializeField] private float shieldCooldownPulseTime = 0.3f;

        public EquipmentType equipmentType {get;} = EquipmentType.Shield;
        public PlayerStateName usageState {get;} = PlayerStateName.staticEquipment;
        public bool isActive {get; private set;} = false;
        public bool canCancelWorkToUse {get;} = true;
        public EquipmentActivationBehaviour activationBehaviour {get;} = EquipmentActivationBehaviour.Hold;
        public float shieldStrength;
        private float shieldBrokenTime = -99f;
        private bool onCooldown = false;
        private bool wasBroken = false;

        private EquipmentSpriteManager spriteManager;

        void Awake() {
            spriteManager = GetComponent<EquipmentSpriteManager>();
        }

        public bool ActivationCondition(Player player) {
            return  shieldStrength > shieldMinStrengthToUse;
        }
        public void Activate(Player player) {
            Debug.Log("Shield Activated");
            isActive = true;
            shieldObject.SetActive(true);
            SoundManager.Instance.Play("ShieldActivate");
        }
        public void CancelActivation(Player player) {
            Debug.Log("Shield Cancelled");
            isActive = false;
            shieldObject.SetActive(false);
            shieldStrength = Mathf.Max(0, shieldStrength - (shieldTogglePenalty*shieldMaxDuration));
            if (shieldStrength < shieldMinStrengthToUse) {
                SoundManager.Instance.Play("ShieldBreak");
            }
        }
        public void Equip(Player player) {
            spriteManager.SetDisplayedSprite("ShieldFull");
            cooldownLight.enabled = true;
            cooldownLight.intensity = 0f;
            shieldStrength = shieldMaxDuration;
        }
        public void Unequip(Player player) {
            cooldownLight.enabled = false;
        }

        public void TakeDamage(Player player) {
            if (isActive) {
                shieldStrength = 0f;
                player.DeactivateEquipment();
                shieldBrokenTime = Time.time;
                wasBroken = true;
            }
        }

        IEnumerator PulseCooldownIndictator() {
            cooldownLight.intensity = 0f;
            cooldownLight.enabled = true;
            Debug.Log("Pulsing");
            SoundManager.Instance.Play("ShieldRecharge");
            float pulseTimer = 0f;
            while (pulseTimer < shieldCooldownPulseTime) {
                if (pulseTimer < shieldCooldownPulseTime/2) {
                    cooldownLight.intensity = Mathf.Lerp(0f, 5f, pulseTimer/(shieldCooldownPulseTime/2));
                    Debug.Log("Intensity " + cooldownLight.intensity);
                } else if (pulseTimer < shieldCooldownPulseTime) {
                    cooldownLight.intensity = Mathf.Lerp(5f, 0f, (pulseTimer-(shieldCooldownPulseTime/2))/(shieldCooldownPulseTime/2));
                    Debug.Log("Intensity " + cooldownLight.intensity);
                }
                pulseTimer += Time.deltaTime;
                yield return null;
            }
            Debug.Log("Pulse Complete");
            cooldownLight.intensity = 0f;
        }

        IEnumerator FlashShield() {
            SpriteRenderer shieldRenderer = shieldObject.GetComponent<SpriteRenderer>();
            while (true) {
                shieldRenderer.enabled = false;
                if (!isActive) break;
                yield return new WaitForSeconds(0.1f);
                if (!isActive) break;
                shieldRenderer.enabled = true;
                yield return new WaitForSeconds(0.1f);
            }
            shieldRenderer.enabled = true;
        }

        public void UpdateEquipment(Player player) {
            if (!isActive) {
                float sheildRecoveryPerSecond = shieldMaxDuration / shieldRecoveryTime;
                if (wasBroken) {
                    if (ActivationCondition(player)) {
                        wasBroken = false;
                    } else {
                        sheildRecoveryPerSecond = shieldMaxDuration / (shieldRecoveryTime+shieldBreakRecoveryPenalty);
                    }
                }
                shieldStrength = Mathf.Min(shieldMaxDuration, shieldStrength + (Time.deltaTime * sheildRecoveryPerSecond));

            } else {
                shieldStrength = Mathf.Max(0, shieldStrength - Time.deltaTime);
                if (shieldStrength <= 0) {
                    player.DeactivateEquipment();
                } else if (shieldStrength <= shieldMaxDuration/4) {
                    StartCoroutine(FlashShield());
                }
             }
            Light2D shieldLight = shieldObject.GetComponent<Light2D>();
            if (shieldLight != null) 
                shieldLight.intensity = Mathf.Lerp(0f, 5f, shieldStrength/shieldMaxDuration);
            if (shieldStrength >= shieldMaxDuration*0.95f) {
                spriteManager.SetDisplayedSprite("ShieldFull");
                if (!isActive && onCooldown) {
                    onCooldown = false;
                    Debug.Log("Shield Cooldown Complete, Pulsing");
                    StartCoroutine(PulseCooldownIndictator());
                }
            } else if (shieldStrength > (shieldMaxDuration/3)*2) {
                spriteManager.SetDisplayedSprite("ShieldTwoThirds");
                onCooldown = true;
            } else if (shieldStrength > shieldMaxDuration/3) {
                spriteManager.SetDisplayedSprite("ShieldOneThird");
                onCooldown = true;
            } else {
                spriteManager.SetDisplayedSprite("ShieldBroken");
                onCooldown = true;
            }
        }
    }
}