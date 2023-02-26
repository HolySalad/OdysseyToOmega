using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Ship.Activatables;

namespace SpaceBoat.Ship.Buildables {
    public class ShipShield : MonoBehaviour, IBuildable
    {
        [SerializeField] private GameObject buildablePrefab;
        [SerializeField] private BuildableType buildableType = BuildableType.ShipShield;
        public BuildableType BuildableType => buildableType;
        public GameObject BuildablePrefab => buildablePrefab;

        public void Build(Vector3 position) {
            GameObject buildable = Instantiate(buildablePrefab, position, Quaternion.identity);
        }
    }
}