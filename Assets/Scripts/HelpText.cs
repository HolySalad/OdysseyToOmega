using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpText : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private GameObject helpTextPrefab;
    [SerializeField] private Sprite repairPrompt;
    [SerializeField] private Sprite pickupControlsHint;
    [SerializeField] private Sprite repairControlsHint;
    [SerializeField] private Sprite cookingControlsHint;
    [SerializeField] private Sprite criticalShip;
    [SerializeField] private Sprite moveControlsHint;
    [SerializeField] private Sprite introHint;
    [SerializeField] private Sprite criticalHealth;

    private bool alreadyShownRepairPrompt = false;
    private bool alreadyShownHealthPrompt = false;

    public bool canHintBeShown(string text) {
        switch (text) {
            case "repair":
                if (alreadyShownRepairPrompt) {
                    return false;
                } else {
                    alreadyShownRepairPrompt = true;
                    return true;
                }
            case "health":
                if (alreadyShownHealthPrompt) {
                    return false;
                } else {
                    alreadyShownHealthPrompt = true;
                    return true;
                }
            default:
                return true;
        }
    }
    

    public Sprite repairPromptSpriteForString(string text)
    {
        switch(text)
        {
            case "repairPrompt":
                return repairPrompt;
            case "pickupControlsHint":
                return pickupControlsHint;
            case "repairControlsHint":
                return repairControlsHint;
            case "cookingControlsHint":
                return cookingControlsHint;
            case "criticalShip":
                return criticalShip;
            case "moveControlsHint":
                return moveControlsHint;
            case "introHint":
                return introHint;
            case "criticalHealth":
                return criticalHealth;
            default:
                return null;
        }
    }

    public void displayHint(string hintType) {
        if (canHintBeShown(hintType)) {
            GameObject helpText = Instantiate(helpTextPrefab, transform.position, Quaternion.identity);
            helpText.GetComponent<SpriteRenderer>().sprite = repairPromptSpriteForString(hintType);
        }
    }

}
