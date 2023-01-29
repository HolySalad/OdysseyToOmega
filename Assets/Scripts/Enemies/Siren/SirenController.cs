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
        [SerializeField] private float sirenSongInterval = 3f;
        [SerializeField] private float sirenBonusAttackInterval = 3f;

        [Header("Siren Teleport Variables")]
        [SerializeField] private int sirenTeleportInterval = 2;
        [SerializeField] private float sirenTeleportVanishTime = 0.1f;
        [SerializeField] private float sirenTeleportAppearTime = 0.5f;
        [SerializeField] private float sirenTeleportDowntime = 1f;

        [Header("Siren Orb Variables")]
        [SerializeField] private float sirenProjectileSummonDistance = 5f;
        [SerializeField] private float sirenProjectileSummonSpeed = 1.5f;
        [SerializeField] private float sirenMaxDowntimeAfterOrbAttack = 4f;

        [Header("Siren Bubblesong Variables")]
        [SerializeField] private float sirenBubbleSongSpawnTime = 0.4f;
        [SerializeField] private int sirenBubbleSongBaseNumBubbles = 3;
        [SerializeField] private int sirenBubbleSongChanceToSpawn = 75;
        [SerializeField] private int sirenBubbleSongMaxNumBubblesPerWave = 6;


        private enum SirenState {Idle, SlowSong, ObstacleSong, BubblesSong, OrbAttack, BubbleAttack, ObstacleAttack, ShieldExplode, Hit, Death, Spawn};

        private enum SirenPositions {Left, Right, Centre, Top};
        private Dictionary<SirenPositions, Transform> sirenPositionDictionary = new Dictionary<SirenPositions, Transform>();

        private SirenState sirenState = SirenState.Spawn;
        private SirenPositions sirenPosition = SirenPositions.Centre;
        private int sirenHp;
        private int sirenTeleportAfterNumAttacks = 0;
        private int sirenAttacksSinceLastTeleport = 0;
        private float sirenSheildDestroyedTime = 0f;
        private int sirenAttacksSinceBonusAttack = 0;
        private int sirenAttacksSinceSong = 0;
        private SirenState lastSirenSong = SirenState.SlowSong;
        private bool shieldIsDown = false;

        private Player player;
        private GameObject siren;
        private SpriteRenderer sirenSprite;
        private GameObject sirenShield;
        private SpriteRenderer sirenShieldSprite;
        private GameObject sirenTPMarker;
        private SpriteRenderer sirenTPMarkerSprite;

        void SirenLog(string text) {
            Debug.Log("SirenController: " + text);
        }

        public bool IsDefeated() {
            return false;
        }

        private void CheckEnraged() {
            if (sirenHp <= 0) {
                shieldIsDown = true;
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
            SirenLog("Completed teleport to " + position);
            if (position == SirenPositions.Top) {
                sirenTeleportAfterNumAttacks = 3;
            } else {
                sirenTeleportAfterNumAttacks = sirenTeleportInterval;
            }
            yield return new WaitForSeconds(sirenTeleportDowntime);
            ChangeSirenState(transitionState);
        }

        SirenState RandomSong() {
            SirenState[] songs = {SirenState.SlowSong, SirenState.ObstacleSong, SirenState.BubblesSong};
            return SirenState.BubblesSong;
            //return songs[Random.Range(0, 2)];
        }


        IEnumerator SirenSlowSong() {
            yield break;
        }

        IEnumerator SirenObstacleSong() {
            yield break;
        }

        IEnumerator SirenBubblesSong() {
            int numBubbles = sirenBubbleSongBaseNumBubbles + (3-sirenHp);
            sirenBubbleSongSpawnPoints.Sort((a, b) => Random.Range(-1, 1));
            ChangeSirenState(SirenState.OrbAttack);
            for (int i = 0; i < numBubbles; i++) {
                int bubblesSpawned = 0;
                foreach (Transform spawnPoint in sirenBubbleSongSpawnPoints) {
                    if (bubblesSpawned < sirenBubbleSongMaxNumBubblesPerWave && Random.Range(0, 100) < sirenBubbleSongChanceToSpawn) {
                        GameObject bubble = Instantiate(sirenBubblePrefab, spawnPoint.position, Quaternion.identity);
                        SirenBubble bubbleScript = bubble.GetComponent<SirenBubble>();
                        bubbleScript.setupBubble();
                        yield return new WaitForSeconds(sirenBubbleSongSpawnTime);
                        bubblesSpawned++;
                    }
                }
            }
            yield break;
        }

        int getNumOrbs() {
            if (sirenHp == 3) {
                return Random.Range(2,3);
            } else if (sirenHp == 2) {
                return Random.Range(2,4);
            } else {
                return Random.Range(3,5);
            }
        }

        SirenState RandomBonusAttack() {
            SirenState[] attacks = {SirenState.BubbleAttack, SirenState.ObstacleAttack};
            return SirenState.OrbAttack;
            //return attacks[Random.Range(0, 1)];
        }   

        IEnumerator SirenBubbleAttack() {
            yield break;
        }

        IEnumerator SirenObstacleAttack() {
            yield break;
        }

        IEnumerator SirenOrbAttack() {
            int numOrbs = getNumOrbs();
            SirenLog("Orb attack with " + numOrbs + " orbs");
            SirenOrb[] orbs = new SirenOrb[numOrbs];
            for (int i = 0; i < numOrbs; i++) {
                SirenLog("Creating orb " + i);
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
                SirenLog("Orb " + i + " firing");
                orbScript.SetupOrb(player);
                orbs[i] = orbScript;
            }
            float attackTimeoutTimer = 0;
            while (true) {
                bool allOrbsDestroyed = true;
                for (int i = 0; i < numOrbs; i++) {
                    if (orbs[i] != null && orbs[i].gameObject != null) {
                        allOrbsDestroyed = false;
                    }
                }
                if (allOrbsDestroyed) {
                    SirenLog("All orbs destroyed");
                    break;
                }
                yield return null;
                attackTimeoutTimer += Time.deltaTime;
                if (attackTimeoutTimer > sirenMaxDowntimeAfterOrbAttack) {
                    SirenLog("Orb attack timed out");
                    break;
                }
            }
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
            bool shouldTP = false;
            SirenPositions tpPosition = RandomPosition();

            //determine attack type.
            if (sirenAttacksSinceSong >= sirenSongInterval) {
                attackToUse = RandomSong();
            } else {
                attackToUse = SirenState.OrbAttack;
            }

            SirenLog("Siren attack: " + attackToUse);

            //determine tp locations and increment counters
            if (attackToUse == SirenState.SlowSong || attackToUse == SirenState.ObstacleSong || attackToUse == SirenState.BubblesSong) {
                shouldTP = true;
                tpPosition = SirenPositions.Top;
            } else {
                sirenAttacksSinceSong++;   
            }
            if (attackToUse == SirenState.OrbAttack) {
                if (sirenAttacksSinceBonusAttack >= sirenBonusAttackInterval) {
                    sirenAttacksSinceBonusAttack = 0;
                    attackToUse = RandomBonusAttack();
                } else {
                    sirenAttacksSinceBonusAttack++;
                    if (sirenAttacksSinceLastTeleport >= sirenTeleportAfterNumAttacks) {
                        shouldTP = true;
                    } else {
                        sirenAttacksSinceLastTeleport++;
                    }
                }
            }
            ChangeSirenState(attackToUse, shouldTP, tpPosition);
            yield break;
        }

        IEnumerator SirenDeath() {
            yield break;
        }

        IEnumerator SirenSpawn() {
            ChangeSirenState(SirenState.Idle);
            yield break;
        }

        void ChangeSirenState(SirenState newState, bool teleportFirst = false, SirenPositions teleportPosition = SirenPositions.Centre) {
            if (teleportFirst) {
                SirenLog("Teleporting to " + teleportPosition + " before changing state");
                StartCoroutine(TeleportSiren(teleportPosition, newState));
                return;
            }
            
            SirenLog("Changing siren state from " + sirenState+ " to " + newState);
            switch (newState) {
                case SirenState.Idle:
                    sirenState = SirenState.Idle;
                    StartCoroutine(SirenIdle());
                    break;
                case SirenState.OrbAttack:
                    sirenState = SirenState.OrbAttack;
                    StartCoroutine(SirenOrbAttack());
                    break;
                case SirenState.BubbleAttack:
                    sirenState = SirenState.BubbleAttack;
                    StartCoroutine(SirenBubbleAttack());
                    break;
                case SirenState.ObstacleAttack:
                    sirenState = SirenState.ObstacleAttack;
                    StartCoroutine(SirenObstacleAttack());
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
            sirenTeleportAfterNumAttacks = sirenTeleportInterval;            

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

            StartCoroutine(SirenSpawn());
        }

    }
}