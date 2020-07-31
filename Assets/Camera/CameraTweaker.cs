using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class CameraTweaker : MonoBehaviour
{
    public CameraSettings mainCamSettings;
    public Transform container;

    private void Start()
    {
        CreateSlider.CreateAttribute(mainCamSettings, container);
    }

}
