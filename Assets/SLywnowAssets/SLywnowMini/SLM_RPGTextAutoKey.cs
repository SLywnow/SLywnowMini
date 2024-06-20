using SLywnow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SLM_RPGText))]
public class SLM_RPGTextAutoKey : MonoBehaviour
{
    public string autoKey;
    public bool autoKeyDefault;
    public string autoTimeoutKey;
    public int autoTimeoutKeyDefault=-1;
    public string showModeKey;
    [Range(0,2)] public int showModeKeyDefault;
    public string showSpeedKey;
    public float showSpeedKeyDefault;

    SLM_RPGText sc;
    void Start()
    {
        if (!SaveSystemAlt.IsWorking())
            SaveSystemAlt.StartWork();

        sc = GetComponent<SLM_RPGText>();
        if (!string.IsNullOrEmpty(autoKey) && sc.textopt.auto !=null)
            sc.textopt.auto.isOn = SaveSystemAlt.GetBool(autoKey, autoKeyDefault);
        if (!string.IsNullOrEmpty(autoTimeoutKey))
            sc.textopt.timer = SaveSystemAlt.GetInt(autoTimeoutKey, autoTimeoutKeyDefault);
        if (!string.IsNullOrEmpty(showModeKey))
            if (SaveSystemAlt.GetInt(showModeKey, showModeKeyDefault)>=0 && SaveSystemAlt.GetInt(showModeKey, showModeKeyDefault)<3)
                sc.textopt.showMode = (SLM_RPGText_Text.mde) SaveSystemAlt.GetInt(showModeKey, showModeKeyDefault);
        if (!string.IsNullOrEmpty(showSpeedKey))
            sc.textopt.speedShow = SaveSystemAlt.GetFloat(showSpeedKey, showSpeedKeyDefault);
    }
}
