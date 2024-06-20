using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SLM_AudioMix : MonoBehaviour
{
    public List<SLM_AudioMix_Block> blocks;

    List<string> names = new List<string>();
    List<int> anims = new List<int>();

	void CheckNames()
    {
        if (!(names.Count == blocks.Count))
        {
            names.Clear();
            foreach (SLM_AudioMix_Block ch in blocks)
                names.Add(ch.name);
        }
    }

	private void OnDisable()
	{
		foreach (SLM_AudioMix_Block b in blocks)
		{
            if (b.shapshots.Count > 0)
                b.shapshots[0].TransitionTo(0);
        }
	}

	private void Start()
	{
        CheckNames();
	}

    public SLM_AudioMix_Block GetBlock(string name)
    {
        CheckNames();

        if (names.Contains(name))
        {
            return blocks[names.IndexOf(name)];
        }
        else return null;
    }

    public int GetID(string name)
    {
        CheckNames();

        if (names.Contains(name))
        {
            return names.IndexOf(name);
        }
        else return -1;
    }

    public void AddClipAndPlay(string name, AudioClip clip)
    {
        CheckNames();

        if (names.Contains(name))
        {
            int id = names.IndexOf(name);
            blocks[id].clips.Add(clip);
            blocks[id].audio.clip = blocks[id].clips[blocks[id].clips.Count - 1];
            blocks[id].audio.Play();
        }
    }

    public void AddClip(string name,AudioClip clip)
	{
        CheckNames();

        if (names.Contains(name))
        {
            int id = names.IndexOf(name);
            blocks[id].clips.Add(clip);
        }
    }

    public void StartPlay(string name)
    {
        CheckNames();

        if (names.Contains(name))
        {
            int id = names.IndexOf(name);
            blocks[id].audio.Play();
        }
    }

    public void StopPlay(string name)
	{
        CheckNames();

        if (names.Contains(name))
		{
            int id = names.IndexOf(name);
            blocks[id].audio.Stop();
        }
	}

    public void SelectClip(string name, int clipId)
    {
        CheckNames();

        if (names.Contains(name))
        {
            int id = names.IndexOf(name);
            bool isp = blocks[id].audio.isPlaying;
            blocks[id].audio.clip = blocks[id].clips[clipId];
            if (isp) blocks[id].audio.Play();
        }
    }

	public void SelectClip(string name, string clipname)
	{
		CheckNames();

		if (names.Contains(name))
		{
			int id = names.IndexOf(name);
			bool isp = blocks[id].audio.isPlaying;
            blocks[id].audio.clip = blocks[id].clips[blocks[id].clips.IndexOf(blocks[id].clips.Find(f => f.name == clipname))];
			if (isp) blocks[id].audio.Play();
		}
	}

	public void SetPitch(string name, float pitch)
    {
        CheckNames();

        if (names.Contains(name))
        {
            int id = names.IndexOf(name);
            blocks[id].audio.pitch = pitch;
        }
    }

    public void SetVolume(string name, float volume)
    {
        CheckNames();

        if (names.Contains(name))
        {
            int id = names.IndexOf(name);

            float vol = volume;
            if (vol < 0) vol = 0;
            else if (vol > 1) vol = 1;

            if (blocks[id].useAudioVolume)
                blocks[id].audio.gameObject.GetComponent<SLM_AudioVolumeSetter>().mainVolume = vol;
            else
                blocks[id].audio.volume = vol;
        }
    }

    public void SetPriority(string name, int priority)
    {
        CheckNames();

        if (names.Contains(name))
        {
            int id = names.IndexOf(name);
            blocks[id].audio.priority = priority;
        }
    }

    public void SetStereoPan(string name, float stereopan)
    {
        CheckNames();

        if (names.Contains(name))
        {
            int id = names.IndexOf(name);
            blocks[id].audio.panStereo = stereopan;
        }
    }

    public void SetLoop(string name, bool loop)
    {
        CheckNames();

        if (names.Contains(name))
        {
            int id = names.IndexOf(name);
            blocks[id].audio.loop = loop;
        }
    }

    public void SetMute(string name, bool mute)
    {
        CheckNames();

        if (names.Contains(name))
        {
            int id = names.IndexOf(name);
            blocks[id].audio.mute = mute;
        }
    }

    public void SetSnapshot(string name, int num, float time=0)
	{
        CheckNames();

        if (names.Contains(name))
        {
            int id = names.IndexOf(name);
            blocks[id].shapshots[num].TransitionTo(time);
        }
    }

    /*public void SetMixer(string name, int num)
    {
        CheckNames();

        if (names.Contains(name))
        {
            int id = names.IndexOf(name);
            bool pl = blocks[id].audio.isPlaying;
            blocks[id].audio.outputAudioMixerGroup = blocks[id].mixers[num];
            if (pl)
                blocks[id].audio.Play();
        }
    }
    public void DisableMixer(string name)
    {
        CheckNames();

        if (names.Contains(name))
        {
            int id = names.IndexOf(name);
            bool pl = blocks[id].audio.isPlaying;
            blocks[id].audio.outputAudioMixerGroup = null;
            if (pl)
                blocks[id].audio.Play();
        }
    }*/
}

[System.Serializable]
public class SLM_AudioMix_Block
{
    public string name;
    public AudioSource audio;
    //public AudioMixerGroup mixer;
    public List<AudioMixerSnapshot> shapshots;
    public List<AudioClip> clips;
    public bool useAudioVolume;
}