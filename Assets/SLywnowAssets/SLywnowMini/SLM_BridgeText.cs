using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SLM_BridgeText : MonoBehaviour
{
    public SLM_ALSLBridge bridge;
    public string key;
    Text text;
    void Awake()
    {
        text = GetComponent<Text>();
        bridge.texts.Add(this);
        UpdateText();
    }

    void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        text.text = bridge.GetString(key);
    }
}
