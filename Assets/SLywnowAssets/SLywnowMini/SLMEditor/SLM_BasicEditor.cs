using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SLM_BasicEditor : MonoBehaviour
{
    public SLM_BasicEditor_SaveStrings saveStrings;
    public Image ImageLayerObject;
    public Transform ImageLayerParrent;

    public List<int> statsGUIS;
    public List<SLM_Stats_GUI> guiPresets;
    public List<SLM_Commands_CustomCommand> customCommands;
}

[System.Serializable]
public class SLM_BasicEditor_SaveStrings
{
    public List<List<string>> langsStrings;
}
