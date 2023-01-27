using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Enemies.Siren {
    public class SirenController : MonoBehaviour
    {
        [Header("Siren Prefabs")]
        [SerializeField] private GameObject sirenPrefab;
        [SerializeField] private GameObject sirenBubblePrefab;
        [SerializeField] private GameObject sirenOrbPrefab;
        [SerializeField] private GameObject sirenShieldPrefab;
        [SerializeField] private GameObject sirenSoulPrefab;
        [SerializeField] private GameObject sirenTPMarkerPrefab;

        [Header("Siren Level Transforms")]
        [SerializeField] private Transform sirenLocationLeft;
        [SerializeField] private Transform sirenLocationRight;
        [SerializeField] private Transform sirenLocationTop;
        [SerializeField] private Transform sirenLocationCentre;
        [SerializeField] private List<Transform> sirenSoulSpawnPointsLeft;
        [SerializeField] private List<Transform> sirenSoulSpawnPointsRight;
        [SerializeField] private List<Transform> sirenBubbleSongSpawnPoints;

        
        [Header("Siren Gameplay Variables")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private float sirenIdleDowntime = 3f;

        [Header("Siren Teleport Variables")]
        [SerializeField] private float sirenTeleportAfterNumAttacks = 2f;
        [SerializeField] private float sirenTeleportVanishTime = 0.1f;
        [SerializeField] private float sirenTeleportAppearTime = 0.5f;
        [SerializeField] private float sirenTeleportDowntime = 1f;

        [Header("Siren Orb Variables")]
        [SerializeField] private float sirenProjectileSummonDistance = 5f;
        [SerializeField] private float sirenProjectileSummonSpeed = 1.5f;


        private enum SirenState {Idle, SlowSong, ObstacleSong, BubblesSong, OrbAttack, ShieldExplode, Hit, Death, Spawn};

        private enum SirenPositions {Left, Right, Centre, Top};
        private Dictionary<SirenPositions, Transform> sirenPositionDictionary = new Dictionary<SirenPositions, Transform>();

        private SirenState sirenState = SirenState.Idle;
        private SirenPositions sirenPosition = SirenPositions.Centre;
        private int sirenHp;
        private int sirenAttacksSinceLastTeleport = 0;
        private int sirenAttacksSinceShieldExplode = 0;
        private int sirenAttacksSinceOrbAttack = 0;
        private int sirenAttacksSinceSong = 0;
        private SirenState lastSirenSong = SirenState.SlowSong;
        private bool isEnraged = false;

        private Player player;
        private GameObject siren;
        private SpriteRenderer sirenSprite;
        private GameObject sirenShield;
        private SpriteRenderer sirenShieldSprite;
        private GameObject sirenTPMarker;
        private SpriteRenderer sirenTPMarkerSprite;

        public bool IsDefeated() {
            return false;
        }

        private void CheckEnraged() {
            if (sirenHp <= 0) {
                isEnraged = true;
            }
        }

        SirenPositions RandomPosition() {
            return (SirenPositions)Random.Range(0, 2);
        }

        IEnumerator TeleportSiren(SirenPositions position, SirenState transitionState) {
            float opacityChangePerFrame = (1 / sirenTeleportVanishTime) * Time.deltaTime;
            while (sirenSprite.color.a > 0) {
                sirenSprite.color = new Color(sirenSprite.color.r, sirenSprite.color.g, sirenSprite.color.b, sirenSprite.color.a - opacityChangePerFrame);
                yield return null;
            }
            siren.transform.position = new Vector3(400, 400, 0);
            yield return new WaitForSeconds(sirenTeleportAppearTime);
            sirenTPMarker.transform.position = sirenPositionDictionary[position].position;
            sirenTPMarkerSprite.color = new Color(sirenTPMarkerSprite.color.r, sirenTPMarkerSprite.color.g, sirenTPMarkerSprite.color.b, 0);
            sirenTPMarkerSprite.enabled = true;
            while (sirenTPMarkerSprite.color.a < 1) {
                sirenTPMarkerSprite.color = new Color(sirenTPMarkerSprite.color.r, sirenTPMarkerSprite.color.g, sirenTPMarkerSprite.color.b, sirenTPMarkerSprite.color.a + opacityChangePerFrame);
                yield return null;
            }
            siren.transform.position = sirenPositionDictionary[position].position;
            sirenTPMarkerSprite.enabled = false;
            sirenSprite.color = new Color(sirenSprite.color.r, sirenSprite.color.g, sirenSprite.color.b, 1);
            sirenPosition = position;
            sirenAttacksSinceLastTeleport = 0;
            ChangeSirenState(transitionState);
        }


        IEnumerator SirenSlowSong() {
            yield break;
        }

        IEnumerator SirenObstacleSong() {
            yield break;
        }

        IEnumerator SirenBubblesSong() {
            yield break;
        }

        int getNumOrbs() {
            if (sirenHp == 3) {
                return 2;
            } else if (sirenHp == 2) {
                return 3;
            } else {
                return 5;
            }
        }

        IEnumerator SirenOrbAttack() {
            int numOrbs = getNumOrbs();
            Debug.Log("Orb attack with " + numOrbs + " orbs");
            SirenOrb[] orbs = new SirenOrb[numOrbs];
            for (int i = 0; i < numOrbs; i++) {
                Debug.Log("Creating orb " + i);
                float offset = sirenProjectileSummonDistance;
                if (Random.Range(0, 1) == 0) {
                    offset *= -1;
                }
                GameObject orb = Instantiate(sirenOrbPrefab, new Vector3(siren.transform.position.x + offset, siren.transform.position.y, 0), Quaternion.identity);
                SirenOrb orbScript = orb.GetComponent<SirenOrb>();
                float originalScale = orb.transform.localScale.x;
                float scaleChangePerFrame = (originalScale / sirenProjectileSummonSpeed) * Time.deltaTime;
                orb.transform.localScale = new Vector3(0, 0, 1);
                while (orb.transform.localScale.x < originalScale) {
                    orb.transform.localScale = new Vector3(orb.transform.localScale.x + scaleChangePerFrame, orb.transform.localScale.y + scaleChangePerFrame, 1);
                    yield return null;
                }
                Debug.Log("Orb " + i + " firing");
                orbScript.SetupOrb(player);
                orbs[i] = orbScript;
            }
            while (true) {
                bool allOrbsDestroyed = true;
                for (int i = 0; i < numOrbs; i++) {
                    if (orbs[i] != null && orbs[i].gameObject != null) {
                        Debug.Log("Orb " + i + " is " + orbs[i].name);
                        allOrbsDestroyed = false;
                    }
                }
                if (allOrbsDestroyed) {
                    Debug.Log("All orbs destroyed");
                    break;
                }
                yield return null;
            }
            sirenAttacksSinceOrbAttack = 0;
            ChangeSirenState(SirenState.Idle);
            yield break;
        }

        IEnumerator SirenShieldExplode() {
            yield break;
        }

        IEnumerator SirenHit() {
            yield break;
        }

        void CreateSirenShield() {
            sirenShield = Instantiate(sirenShieldPrefab, siren.transform.position, Quaternion.identity);
            sirenShieldSprite = sirenShield.GetComponent<SpriteRenderer>();
        }

        IEnumerator SirenIdle() {
            yield return new WaitForSeconds(sirenIdleDowntime);
            //chose an attack
            SirenState attackToUse = SirenState.OrbAttack;
            if (sirenAttacksSinceLastTeleport >= sirenTeleportAfterNumAttacks) {
                if (attackToUse == SirenState.SlowSong || attackToUse == SirenState.ObstacleSong || attackToUse == SirenState.BubblesSong) {
                    ChangeSirenState(attackToUse, true, SirenPositions.Top);
                } else {
                    ChangeSirenState(attackToUse, true, RandomPosition());
                }
            } else {
                sirenAttacksSinceLastTeleport++;
                ChangeSirenState(attackToUse);
            }
            yield break;
        }

        IEnumerator SirenDeath() {
            yield break;
        }

        IEnumerator SirenSpawn() {
            StartCoroutine(SirenIdle());
            yield break;
        }

        void ChangeSirenState(SirenState newState, bool teleportFirst = false, SirenPositions teleportPosition = SirenPositions.Centre) {
            if (teleportFirst) {
                StartCoroutine(TeleportSiren(teleportPosition, newState));
                return;
            }
            switch (newState) {
                case SirenState.Idle:
                    sirenState = SirenState.Idle;
                    StartCoroutine(SirenIdle());
                    break;
                case SirenState.OrbAttack:
                    sirenState = SirenState.OrbAttack;
                    StartCoroutine(SirenOrbAttack());
                    break;
                case SirenState.SlowSong:
                    sirenState = SirenState.SlowSong;
                    StartCoroutine(SirenSlowSong());
                    break;
                case SirenState.ObstacleSong:
                    sirenState = SirenState.ObstacleSong;
                    StartCoroutine(SirenObstacleSong());
                    break;
                case SirenState.BubblesSong:
                    sirenState = SirenState.BubblesSong;
                    StartCoroutine(SirenBubblesSong());
                    break;
                case SirenState.ShieldExplode:
                    sirenState = SirenState.ShieldExplode;
                    StartCoroutine(SirenShieldExplode());
                    break;
                case SirenState.Hit:
                    sirenState = SirenState.Hit;
                    StartCoroutine(SirenHit());
                    break;
                case SirenState.Death:
                    sirenState = SirenState.Death;
                    StartCoroutine(SirenDeath());
                    break;
                case SirenState.Spawn:
                    sirenState = SirenState.Spawn;
                    StartCoroutine(SirenSpawn());
                    break;
            }
        }

        private void Awake() {

            sirenHp = maxHealth;            

            sirenPositionDictionary.Add(SirenPositions.Left, sirenLocationLeft);
            sirenPositionDictionary.Add(SirenPositions.Right, sirenLocationRight);
            sirenPositionDictionary.Add(SirenPositions.Top, sirenLocationTop);
            sirenPositionDictionary.Add(SirenPositions.Centre, sirenLocationCentre);
        }


        void Start() {
            player = GameModel.Instance.player;
            siren = Instantiate(sirenPrefab, sirenLocationCentre.position, Quaternion.identity);
            sirenSprite = siren.GetComponent<SpriteRenderer>();
            sirenTPMarker = Instantiate(sirenTPMarkerPrefab, sirenLocationCentre.position, Quaternion.identity);
            sirenTPMarkerSprite = sirenTPMarker.GetComponent<SpriteRenderer>();
            sirenTPMarkerSprite.enabled = false;

            ChangeSirenState(SirenState.Spawn);
        }

    }
}