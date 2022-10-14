using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Movement {
    public interface IMovementModifier
    {
        Vector2 Value { get; }
        bool Enabled { get; }
        void OnEnable();

        void OnDisable();

        void UpdateModifier(float deltaTime);

    }
}
