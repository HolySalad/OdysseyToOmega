using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SpaceBoat.PlayerSubclasses {

    [System.Serializable] 
    public class EquipmentSprite {
        public string name;
        public SpriteRenderer spriteRenderer;
    }
    public class EquipmentSpriteManager : MonoBehaviour
    {
        [SerializeField] private List<EquipmentSprite> equipmentSprites = new List<EquipmentSprite>();
        private Dictionary<string, SpriteRenderer> equipmentSpritesDict = new Dictionary<string, SpriteRenderer>();

        private SpriteRenderer currentDisplayedSprite;

        void Awake() {
            foreach (EquipmentSprite equipmentSprite in equipmentSprites) {
                if (equipmentSpritesDict.ContainsKey(equipmentSprite.name)) {
                    Debug.LogWarning("EquipmentSpriteManager already contains a sprite with name " + equipmentSprite.name);
                    continue;
                }
                equipmentSpritesDict.Add(equipmentSprite.name, equipmentSprite.spriteRenderer);
                if (equipmentSprite.spriteRenderer.enabled) {
                    if (currentDisplayedSprite != null) {
                        Debug.LogWarning("More than one sprite is enabled in EquipmentSpriteManager");
                    } else {
                        currentDisplayedSprite = equipmentSprite.spriteRenderer;
                    }
                } else {
                    equipmentSprite.spriteRenderer.enabled = true;
                    equipmentSprite.spriteRenderer.color = new Color(1, 1, 1, 0);
                }
            }
        }

        public void SetDisplayedSprite(string spriteName) {
            if (currentDisplayedSprite != null) {
                currentDisplayedSprite.color = new Color(1, 1, 1, 0);
            }
            Debug.Log("Player Equipment: Setting displayed sprite to " + spriteName);
            currentDisplayedSprite = equipmentSpritesDict[spriteName];
            currentDisplayedSprite.enabled = true;
            currentDisplayedSprite.color = new Color(1, 1, 1, 1);
        }

    }
}