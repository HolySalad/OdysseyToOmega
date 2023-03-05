using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TwistingColours : MonoBehaviour
{
    Color currentColor;
    Color currentSecondaryColor;
    float H, S, V;
    void Start(){
        currentColor = new Color(71, 251, 1);
        currentSecondaryColor = new Color(248,80,0);
    }

    // Update is called once per frame
    void Update()
    {
        Color.RGBToHSV(currentColor, out H, out S, out V);
        H += 0.001f % 1;
        currentColor = Color.HSVToRGB(H, 0.99f, 0.99f);
        GetComponent<Image>().material.SetColor("_BasePrimaryColour", currentColor);

        Color.RGBToHSV(currentSecondaryColor, out H, out S, out V);
        H += 0.001f % 1;
        currentSecondaryColor = Color.HSVToRGB(H, 0.99f, 0.99f);
        GetComponent<Image>().material.SetColor("_BaseEyeColour", currentSecondaryColor);
    }
}
