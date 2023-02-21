using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.Ship.Buildables {

    [System.Serializable] public class buildableSaveData {
        public BuildableType buildableType;
        public Vector3 position;
    }

    public enum BuildableType {JumpPad}
    public interface IBuildable
    {
        public BuildableType BuildableType {get;}
        public GameObject BuildablePrefab {get;}

        public void Build(Vector3 position);
        
    }
}