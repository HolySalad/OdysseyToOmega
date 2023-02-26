using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SpaceBoat.PlayerSubclasses.Equipment {
    public class DashEquipment : MonoBehaviour, IPlayerEquipment
    {
        [SerializeField] private SpriteRenderer activeSprite;
        [SerializeField] private Light2D activeLight;
        [SerializeField] private SpriteRenderer readySprite;
        [SerializeField] private SpriteRenderer cooldownTwoThirdsSprite;
        [SerializeField] private SpriteRenderer cooldownOneThirdSprite;
        [SerializeField] private SpriteRenderer cooldownZeroSprite;

        [SerializeField] private float dashCooldown = 2f;
        [SerializeField] private float dashDuration = 0.5f;
        [SerializeField] public float dashSpeed = 20f;
        [SerializeField] public float maintainedMomentumMultiplier = 0.5f;

        public EquipmentType equipmentType {get;} = EquipmentType.Dash;
        public PlayerStateName usageState {get;} = PlayerStateName.dash;
        public bool isActive {get; private set;} = false;
        public bool canCancelWorkToUse {get;} = false;
        public EquipmentActivationBehaviour activationBehaviour {get;} = EquipmentActivationBehaviour.Press;
        private float cooldown = 0f;
        private float dashTimer = 0f;
        public bool hasLandedSinceLastDash = true;

        private Player player;

        public bool ActivationCondition(Player player) {
            return (cooldown <= 0f && hasLandedSinceLastDash);
        }
        public void Activate(Player player) {
            this.player = player;
            isActive = true;
            cooldown = dashCooldown;
            dashTimer = dashDuration;
            readySprite.enabled = false;
            activeSprite.enabled = true;
            if (activeLight != null) activeLight.enabled = true;
            SoundManager.Instance.Play("Dash");
        }
        public void CancelActivation(Player player) {
            isActive = false;
            cooldownZeroSprite.enabled = false;
            activeSprite.enabled = false;
            activeLight.enabled = false;
        }
        public void Equip(Player player) {
            this.player = player;
            StartCoroutine(UpdateBackpackVisuals());
        }
        public void Unequip(Player player) {
            StopCoroutine(UpdateBackpackVisuals());
            cooldownTwoThirdsSprite.enabled = false;
            cooldownOneThirdSprite.enabled = false;
            cooldownZeroSprite.enabled = false;
            readySprite.enabled = false;
            activeSprite.enabled = false;
            activeLight.enabled = false;
        }

        IEnumerator UpdateBackpackVisuals() {
            while (player.currentEquipmentType == EquipmentType.Dash) {
                if (cooldown > 0f) {
                    if (cooldown > dashCooldown * 2f / 3f) {
                        cooldownTwoThirdsSprite.enabled = false;
                        cooldownOneThirdSprite.enabled = false;
                        cooldownZeroSprite.enabled = true;
                    } else if (cooldown > dashCooldown * 1f / 3f) {
                        cooldownTwoThirdsSprite.enabled = false;
                        cooldownOneThirdSprite.enabled = true;
                        cooldownZeroSprite.enabled = false;
                    } else {
                        cooldownTwoThirdsSprite.enabled = true;
                        cooldownOneThirdSprite.enabled = false;
                        cooldownZeroSprite.enabled = false;
                    }
                } else {
                    cooldownTwoThirdsSprite.enabled = false;
                    cooldownOneThirdSprite.enabled = false;
                    cooldownZeroSprite.enabled = false;
                    readySprite.enabled = true;
                }
                yield return null;
            }
        }

        public void UpdateEquipment(Player player) {
            if (cooldown > 0f) {
                cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);
            }
            if (isActive) {
                dashTimer = Mathf.Max(0f, dashTimer - Time.deltaTime);
                if (dashTimer <= 0f) {
                    player.DeactivateEquipment();
                }
            }
        }
    }

}