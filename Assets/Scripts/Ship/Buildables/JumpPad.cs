using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.Ship.Buildables {
    public class JumpPad : MonoBehaviour, IBuildable
    {
        [SerializeField] private GameObject buildablePrefab;
        [SerializeField] private BuildableType buildableType = BuildableType.JumpPad;
        public BuildableType BuildableType => buildableType;
        public GameObject BuildablePrefab => buildablePrefab;

        public void Build(Vector3 position) {
            GameObject buildable = Instantiate(buildablePrefab, position, Quaternion.identity);
        }
    }
}