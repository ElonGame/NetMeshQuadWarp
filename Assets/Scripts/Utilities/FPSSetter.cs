using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class FPSSetter : MonoBehaviour
{

    public int TargetFPS = 30;

    void Awake()
    {
        QualitySettings.vSyncCount  = 0;
        Application.targetFrameRate = TargetFPS;
    }
}
