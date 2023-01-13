using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Enemies.DemoHydra {
    public class HydraMain : MonoBehaviour
    {
        void Shout() {
            Debug.Log("Shout");
            SoundManager.Instance.Play("HydraRoar", () => {
                Debug.Log("Shout callback");
                GameModel.Instance.demoSceneReadyExit = true;
            });
        }
    }
}
