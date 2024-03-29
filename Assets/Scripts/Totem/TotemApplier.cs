using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotemEntities;
using TotemEntities.DNA;
using TotemServices.DNA;


public class TotemApplier : MonoBehaviour
{
    [SerializeField] Material cthulkMaterial;
    [SerializeField] Material harpoonMaterial;
    [SerializeField] Material elementMaterial;
    [SerializeField] GameObject harpoon;
    [SerializeField] GameObject harpoonGunSpriteParent;

    [SerializeField] SpriteRenderer cthulkAssymetricalHair;
    [SerializeField] SpriteRenderer cthulkBuzzcutHair;
    [SerializeField] SpriteRenderer cthulkLongHair;
    [SerializeField] SpriteRenderer cthulkShortHair;
    [SerializeField] SpriteRenderer cthulkDreadlocksHair;
    [SerializeField] SpriteRenderer cthulkBraidHair;
    [SerializeField] SpriteRenderer cthulkPonytailHair;

    [SerializeField] Sprite harpoonMaterialObsidian;
    [SerializeField] Sprite harpoonMaterialBone;
    [SerializeField] Sprite harpoonMaterialWood;
    [SerializeField] Sprite harpoonMaterialFlint;

    [SerializeField] Sprite harpoonElementFire;
    [SerializeField] Sprite harpoonElementWater;
    [SerializeField] Sprite harpoonElementEarth;
    [SerializeField] Sprite harpoonElementAir;

    [SerializeField] Color cthulkColorDefault;
    [SerializeField] Color cthulkEyeColorDefault;

    private Dictionary<string, SpriteRenderer> avatarssDictionary = new Dictionary<string, SpriteRenderer>();
    private Dictionary<string, Sprite> harpoonMaterialDictionary = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> harpoonElementDictionary = new Dictionary<string, Sprite>();

    private SpriteRenderer currentHair;
    void Awake() {
            avatarssDictionary.Add("afro", cthulkBuzzcutHair);
            avatarssDictionary.Add("asymmetrical", cthulkAssymetricalHair);
            avatarssDictionary.Add("braids", cthulkBraidHair);
            avatarssDictionary.Add("buzz cut", cthulkBuzzcutHair);
            avatarssDictionary.Add("dreadlocks",cthulkDreadlocksHair);
            avatarssDictionary.Add("long", cthulkLongHair);
            avatarssDictionary.Add("ponytail", cthulkPonytailHair);
            avatarssDictionary.Add("short", cthulkShortHair);
            currentHair = cthulkShortHair;

            harpoonMaterialDictionary.Add("bone", harpoonMaterialBone);
            harpoonMaterialDictionary.Add("obsidian", harpoonMaterialObsidian);
            harpoonMaterialDictionary.Add("wood", harpoonMaterialWood);
            harpoonMaterialDictionary.Add("flint", harpoonMaterialFlint);

            harpoonElementDictionary.Add("fire", harpoonElementFire);
            harpoonElementDictionary.Add("water", harpoonElementWater);
            harpoonElementDictionary.Add("earth", harpoonElementEarth);
            harpoonElementDictionary.Add("air", harpoonElementAir);
    }

    void Start() { 
        if (VariableManager.Instance != null) {
            ApplyTotemCthulk(
                VariableManager.Instance.Avatar.hair_styles.ToLower(),
                VariableManager.Instance.Avatar.primary_color,
                VariableManager.Instance.Avatar.secondary_color
            );
            if (!VariableManager.Instance.DefaultHarpoon) ApplyTotemHarpoon(
                VariableManager.Instance.Harpoon.weapon_material.ToLower(),
                VariableManager.Instance.Harpoon.classical_element.ToLower(),
                VariableManager.Instance.Harpoon.primary_color,
                VariableManager.Instance.Harpoon.secondary_color);
        }
        /*
        SpriteRenderer harpoonMaterialSpriteRenderer = harpoon.transform.Find("SpriteParent").Find("Bone").GetComponent<SpriteRenderer>();
        SpriteRenderer harpoonElementSpriteRenderer = harpoon.transform.Find("SpriteParent").Find("Earth").GetComponent<SpriteRenderer>();
        harpoon.GetComponent<SpriteRenderer>().enabled = true;
        harpoonMaterialSpriteRenderer.enabled = false;
        harpoonElementSpriteRenderer.enabled = false;

        SpriteRenderer harpoonGunMaterialSpriteRenderer = harpoonGunSpriteParent.transform.Find("SpriteParent").Find("Bone").GetComponent<SpriteRenderer>();
        SpriteRenderer harpoonGunElementSpriteRenderer = harpoonGunSpriteParent.transform.Find("SpriteParent").Find("Earth").GetComponent<SpriteRenderer>();
        harpoonGunSpriteParent.GetComponent<SpriteRenderer>().enabled = true;
        harpoonGunMaterialSpriteRenderer.enabled = false;
        harpoonGunElementSpriteRenderer.enabled = false;

        cthulkMaterial.SetColor("_BasePrimaryColour", cthulkColorDefault);
        cthulkMaterial.SetColor("_BaseEyeColour", cthulkEyeColorDefault);
        */
    }

    public void ApplyTotemCthulk(string hairStyle, Color32 primaryColour, Color32 secondaryColour) {
        Debug.Log(
            "Applying Totem Cthulk: " + hairStyle + " " + primaryColour + " " + secondaryColour
        );
        currentHair.enabled = false;
        if (!avatarssDictionary.ContainsKey(hairStyle)) {
            Debug.Log("Hair style not found: " + hairStyle);
            return;
        }
        currentHair = avatarssDictionary[hairStyle];
        currentHair.enabled = true;
        cthulkMaterial.SetColor("_BasePrimaryColour", primaryColour);
        cthulkMaterial.SetColor("_BaseEyeColour", secondaryColour);
        
    }

    public void ApplyTotemHarpoon(string material, string element, Color32 primaryColour, Color32 secondaryColour) {
        SpaceBoat.GameModel.Instance.HarpoonGun.GetComponentInChildren<SpaceBoat.Ship.Activatables.HarpoonGunActivatable>().useTotemHarpoon = true;

        SpriteRenderer harpoonMaterialSpriteRenderer = harpoon.transform.Find("SpriteParent").Find("Bone").GetComponent<SpriteRenderer>();
        SpriteRenderer harpoonElementSpriteRenderer = harpoon.transform.Find("SpriteParent").Find("Earth").GetComponent<SpriteRenderer>();
        harpoon.GetComponent<SpriteRenderer>().enabled = false;
        harpoonMaterialSpriteRenderer.enabled = true;
        harpoonElementSpriteRenderer.enabled = true;
        SpriteRenderer harpoonGunMaterialSpriteRenderer = harpoonGunSpriteParent.transform.Find("SpriteParent").Find("Bone").GetComponent<SpriteRenderer>();
        SpriteRenderer harpoonGunElementSpriteRenderer = harpoonGunSpriteParent.transform.Find("SpriteParent").Find("Earth").GetComponent<SpriteRenderer>();
        harpoonGunSpriteParent.GetComponent<SpriteRenderer>().enabled = false;
        harpoonGunMaterialSpriteRenderer.enabled = true;
        harpoonGunElementSpriteRenderer.enabled = true;
        if (!harpoonMaterialDictionary.ContainsKey(material)) {
            Debug.Log("Harpoon material not found: " + material);
            if (!harpoonElementDictionary.ContainsKey(element)) {
                Debug.Log("Harpoon element not found: " + element);
            }
            return;
        }
        if (!harpoonElementDictionary.ContainsKey(element)) {
            Debug.Log("Harpoon element not found: " + element);
            return;
        }


        harpoonMaterialSpriteRenderer.sprite = harpoonMaterialDictionary[material];
        harpoonElementSpriteRenderer.sprite = harpoonElementDictionary[element];
        harpoonGunMaterialSpriteRenderer.sprite = harpoonMaterialDictionary[material];
        harpoonGunElementSpriteRenderer.sprite = harpoonElementDictionary[element];

        harpoonMaterial.SetColor("_BasePrimaryColour", primaryColour);
        harpoonMaterial.SetColor("_BaseEyeColour", primaryColour);
        elementMaterial.SetColor("_BasePrimaryColour", secondaryColour);
        elementMaterial.SetColor("_BasePrimaryColour", secondaryColour);
    }
}
