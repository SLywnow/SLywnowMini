using SLywnow;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SLM_AudioVolumeSetter : MonoBehaviour
{
    public string SSAKey;
    [Range(0, 2)] public float mainVolume = 1;
    [Range(0, 1)] public float optionVolume = 1;
    [Range(0, 1)] public float additionVolume = 1;
    public bool useUpdate = true;
    public bool updateOptionVolumeInUpdate;

    AudioSource aud;
    void Start()
    {
        aud = GetComponent<AudioSource>();
        UpdateSSA();
        float vol = mainVolume * optionVolume * additionVolume;
        aud.volume = vol;
    }

    private void Update()
    {
        if (useUpdate)
        {
            if (updateOptionVolumeInUpdate) UpdateSSA();

            UpdateVolume();
        }
    }

	public void UpdateSSA()
    {
        float vol = optionVolume;
        if (SaveSystemAlt.HasKey(SSAKey))
            vol = SaveSystemAlt.GetFloat(SSAKey, 1);
        if (vol > 1) vol = 1;
        else if (vol < 0) vol = 0;
        optionVolume = vol;
    }

    public void UpdateVolume()
    {
        float vol = mainVolume * optionVolume * additionVolume;
        if (aud.volume != vol)
            aud.volume = vol;
    }
}
