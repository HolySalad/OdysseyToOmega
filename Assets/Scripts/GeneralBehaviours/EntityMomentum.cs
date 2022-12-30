using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat {
        public class EntityMomentum {
        private Vector2 acceleration;
        private Vector2 terminalVelocity;
        private Vector2 currentVelocity = Vector2.zero;
        private int frameStarted;
        private int frameDecelerationStarted;
        private int decelerationMinStartFrame = 0;
        private bool reachedTerminalVelocity = false;

        private Vector2 deceleration;
        private delegate bool DecelerationEndCondition();
        private DecelerationEndCondition decelerationEndCondition;

        //optional fields
        private bool hasJerk;
        private float accelerationJerkFrameMultiplier = 1f;
        private float accelerationJerkFrameExponent = 1f;
        private float accelerationCap = 1f;
        private float decelerationJerkFrameMultiplier = 1f;
        private float decelerationJerkFrameExponent = 1f;
        private float decelerationCap = 1f;


        
        // basic constructor
        EntityMomentum(Vector2 acceleration, Vector2 terminalVelocity, Vector2 deceleration, DecelerationEndCondition decelerationEndCondition, int decelerationMinStartFrame) {
            this.acceleration = acceleration;
            this.terminalVelocity = terminalVelocity;
            this.deceleration = deceleration;
            this.decelerationEndCondition = decelerationEndCondition;
            this.decelerationMinStartFrame = decelerationMinStartFrame;
        }

        // constructor with jerk (change in acceleration over time)
        EntityMomentum(Vector2 acceleration, Vector2 terminalVelocity, Vector2 deceleration, DecelerationEndCondition decelerationEndCondition, int decelerationMinStartFrame, float accelerationJerkFrameMultiplier, float accelerationJerkFrameExponent, float accelerationCap, float decelerationJerkFrameMultiplier, float decelerationJerkFrameExponent, float decelerationCap) {
            this.acceleration = acceleration;
            this.terminalVelocity = terminalVelocity;
            this.deceleration = deceleration;
            this.decelerationEndCondition = decelerationEndCondition;
            this.decelerationMinStartFrame = decelerationMinStartFrame;

            this.accelerationJerkFrameMultiplier = accelerationJerkFrameMultiplier;

            this.accelerationJerkFrameExponent = accelerationJerkFrameExponent;
            this.accelerationCap = accelerationCap;
            this.decelerationJerkFrameMultiplier = decelerationJerkFrameMultiplier;
            this.decelerationJerkFrameExponent = decelerationJerkFrameExponent;
            this.decelerationCap = decelerationCap;
            hasJerk = true;

        }

        private Vector2 ClampVector(Vector2 vector, Vector2 max) {
            float x = Mathf.Min(Mathf.Abs(vector.x), Mathf.Abs(max.x)) * Mathf.Sign(vector.x);
            float y = Mathf.Min(Mathf.Abs(vector.y), Mathf.Abs(max.y)) * Mathf.Sign(vector.y);
            return new Vector2(x, y);
        }

        private Vector2 CheckVectorOvershoot(Vector2 currentVelocity) {
            bool xOver = Mathf.Sign(terminalVelocity.x) != Mathf.Sign(currentVelocity.x) && currentVelocity.x != 0;
            bool yOver = Mathf.Sign(terminalVelocity.y) != Mathf.Sign(currentVelocity.y) && currentVelocity.y != 0;
            return new Vector2(xOver ? 0 : currentVelocity.x, yOver ? 0 : currentVelocity.y);
        }

        private Vector2 ApplyAccelJerk() {
            if (!hasJerk) return acceleration;
            int frames = Time.frameCount - frameStarted;
            float multiplier = Mathf.Min(Mathf.Pow(frames, accelerationJerkFrameExponent) * accelerationJerkFrameMultiplier, accelerationCap);
            return acceleration * multiplier;
        }

        private Vector2 ApplyDecelJerk() {
            if (!hasJerk) return deceleration;
            int frames = Time.frameCount - frameDecelerationStarted;
            float multiplier = Mathf.Min(Mathf.Pow(frames, decelerationJerkFrameExponent) * decelerationJerkFrameMultiplier, decelerationCap);
            return deceleration * multiplier;
        }

        public IEnumerator UpdateMomentum() {
            frameStarted = Time.frameCount;
            while (!reachedTerminalVelocity) {
                currentVelocity = ClampVector(currentVelocity + ApplyAccelJerk(), terminalVelocity);
                if (currentVelocity.Equals(terminalVelocity)) {
                    reachedTerminalVelocity = true;
                }
                yield return null;
            }
            frameDecelerationStarted = Time.frameCount;
            while (!currentVelocity.Equals(Vector2.zero)) {
                // don't decelerate until we have passed the minimum frame count
                if (Time.frameCount - frameStarted < decelerationMinStartFrame) {
                    frameDecelerationStarted = Time.frameCount + 1;
                    yield return null;
                    continue;
                } else if (decelerationEndCondition()) {
                    currentVelocity = Vector2.zero;
                } else {
                    currentVelocity = CheckVectorOvershoot(currentVelocity - ApplyDecelJerk());
                }
                yield return null;
            }
        }
    }
}