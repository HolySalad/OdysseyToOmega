using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Enemies.Siren {
    public class SirenController : MonoBehaviour
    {
        [SerializeField] private GameObject sirenBubblePrefab;
        [SerializeField] private GameObject sirenOrbPrefab;
        [SerializeField] private GameObject sirenShieldPrefab;
        [SerializeField] private GameObject sirenSoulPrefab;

        IEnumerator SirenSlowSong() {
            yield break;
        }

        IEnumerator SirenObstacleSong() {
            yield break;
        }

        IEnumerator SirenBubblesSong() {
            yield break;
        }

        IEnumerator SirenOrbAttack() {
            yield break;
        }

        IEnumerator SirenShieldExplode() {
            yield break;
        }

        IEnumerator SirenHit() {
            yield break;
        }

    }
}