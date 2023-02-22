using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceBoat.Rewards;

namespace SpaceBoat.UI {
    public class BlueprintUnlockUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI blueprintUnlockedText;

        public void CreateBlueprintUnlockUI(Collectable collectable) {
            blueprintUnlockedText.text = collectable.blueprintCollectableName + " can now be created at the crafting bench.";
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                UIManager.Instance.CloseBlueprintUnlockPanel();
            }
        }

    }
}