using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDUpdater : MonoBehaviour
{
    public TextMeshProUGUI currentVCam;
    public TextMeshProUGUI controls;
    TextMeshProUGUI[] _hudObjects;

    // Start is called before the first frame update
    private void Awake()
    {
       _hudObjects = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
       foreach (TextMeshProUGUI hudObject in _hudObjects)
       {
            switch (hudObject.name)
            {
                case "TMPCurrentVcam":
                {
                    currentVCam = hudObject;
                    break;
                }
                case "TMPControls":
                {
                    controls = hudObject;
                    break;
                }
            }
       }
    }

    public void UpdateHUD(string Vcam)
    {
        switch (Vcam)
                    {
                        case "OverTheShoulder":
                        {
                            currentVCam.text = "Cam: Over The Shoulder";
                            controls.text = "Left Stick / WASD: Move" +
                                             "\nRight Stick / Mouse: Look (Up/Down/Left/Right)"+
                                            "\nLeft Trigger / Shift: Run" +
                                            "\nA / Space: Jump" +
                                            "\nD-Pad / 1,2,3,4: Camera Select" +
                                            "\nEscape: Quit";
                            break;
                        }
                        case "LowAngle":
                        {
                            currentVCam.text = "Cam: Low Angle";
                            controls.text = "Left Stick / WASD: Move" +
                                            "\nRight Stick / Mouse: Look (Left/Right)"+
                                            "\nLeft Trigger / Shift: Run" +
                                            "\nA / Space: Jump" +
                                            "\nD-Pad / 1,2,3,4: Camera Select" +
                                            "\nEscape: Quit";
                            break;
                        }
                        case "Isometric":
                        {
                            currentVCam.text = "Cam: Isometric";
                            controls.text = "Left Stick / WASD: Move" +
                                            "\nRight Stick / Q / E: Quarter Rotate (Left/Right)"+
                                            "\nLeft Trigger / Shift: Run" +
                                            "\nA / Space: Jump" +
                                            "\nD-Pad / 1,2,3,4: Camera Select" +
                                            "\nEscape: Quit";
                            break;
                        }
                        case "MigratingIsometric":
                        {
                            currentVCam.text = "Cam: Migrating Isometric";
                            controls.text = "Left Stick / WASD: Move" +
                                            "\nRight Stick / Q / E: Quarter Rotate (Left/Right)" +
                                            "\nLeft Trigger / Shift: Run" +
                                            "\nA / Space: Jump" +
                                            "\nD-Pad / 1,2,3,4: Camera Select" +
                                            "\nEscape: Quit";
                            break;
                        }
                    }
    }
}
