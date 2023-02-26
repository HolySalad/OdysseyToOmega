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
    [SerializeField] GameObject harpoon;

    [SerializeField] SpriteRenderer cthulkAssymetricalHair;
    [SerializeField] SpriteRenderer cthulkBuzzcutHair;
    [SerializeField] SpriteRenderer cthulkLongHair;
    [SerializeField] SpriteRenderer cthulkShortHair;
    [SerializeField] SpriteRenderer cthulkDreadlocksHair;
    [SerializeField] SpriteRenderer cthulkBraidHair;
    [SerializeField] SpriteRenderer cthulkPonytailHair;

    [SerializeField] SpriteRenderer harpoonMaterialObsidian;
    [SerializeField] SpriteRenderer harpoonMaterialBone;
    [SerializeField] SpriteRenderer harpoonMaterialWood;
    [SerializeField] SpriteRenderer harpoonMaterialFlint;

    [SerializeField] SpriteRenderer harpoonElementFire;
    [SerializeField] SpriteRenderer harpoonElementWater;
    [SerializeField] SpriteRenderer harpoonElementEarth;
    [SerializeField] SpriteRenderer harpoonElementAir;

    private Dictionary<string, SpriteRenderer> avatarssDictionary = new Dictionary<string, SpriteRenderer>();

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
    }

    public void ApplyTotemCthulk(string hairStyle, Color32 primaryColour, Color32 secondaryColour) {
        Debug.Log(
            "Applying Totem Cthulk: " + hairStyle + " " + primaryColour + " " + secondaryColour
        );
        currentHair.enabled = false;
        currentHair = avatarssDictionary[hairStyle];
        currentHair.enabled = true;
        cthulkMaterial.SetColor("_BasePrimaryColour", primaryColour);
        cthulkMaterial.SetColor("_BaseEyeColour", secondaryColour);
        
    }
}
