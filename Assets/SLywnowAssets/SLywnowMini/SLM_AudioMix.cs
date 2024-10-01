using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SLM_AudioMix : MonoBehaviour
{
   public List<SLM_AudioMix_Block> blocks;


   private void OnDisable()
   {
      foreach (SLM_AudioMix_Block b in blocks)
      {
         if (b.shapshots.Count > 0)
            b.shapshots[0].TransitionTo(0);
      }
   }

   public SLM_AudioMix_Block GetBlock(string name)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      return b;
   }

   public int GetID(string name)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      if (b != null)
      {
         return blocks.IndexOf(b);
      }
      else return -1;
   }

   public void AddClipAndPlay(string name, AudioClip clip)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      if (b != null)
      {
         b.clips.Add(clip);
         b.audio.clip = b.clips[b.clips.Count - 1];
         b.audio.Play();
      }
   }

   public void AddClip(string name, AudioClip clip)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      if (b != null)
      {
         b.clips.Add(clip);
      }
   }

   public void StartPlay(string name)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      if (b != null)
      {
         b.audio.Play();
      }
   }

   public void StopPlay(string name)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      if (b != null)
      {
         b.audio.Stop();
      }
   }

   public void SelectClip(string name, int clipId)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      if (b != null)
      {
         bool isp = b.audio.isPlaying;
         b.audio.clip = b.clips[clipId];
         if (isp) b.audio.Play();
      }
   }

   public void SelectClip(string name, string clipname)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      if (b != null)
      {
         bool isp = b.audio.isPlaying;
         b.audio.clip = b.clips[b.clips.IndexOf(b.clips.Find(f => f.name == clipname))];
         if (isp) b.audio.Play();
      }
   }

   public void SetPitch(string name, float pitch)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      if (b != null)
      {
         b.audio.pitch = pitch;
      }
   }

   public void SetVolume(string name, float volume)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      if (b != null)
      {
         float vol = volume;
         if (vol < 0) vol = 0;

         if (b.useAudioVolume)
         {
            b.audio.gameObject.GetComponent<SLM_AudioVolumeSetter>().mainVolume = vol;
         }
         else
         {
            if (vol > 1) vol = 1;
            b.audio.volume = vol;
         }
      }
   }

   public void SetPriority(string name, int priority)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      if (b != null)
      {
         b.audio.priority = priority;
      }
   }

   public void SetStereoPan(string name, float stereopan)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      if (b != null)
      {
         b.audio.panStereo = stereopan;
      }
   }

   public void SetLoop(string name, bool loop)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      if (b != null)
      {
         b.audio.loop = loop;
      }
   }

   public void SetMute(string name, bool mute)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      if (b != null)
      {
         b.audio.mute = mute;
      }
   }

   public void SetSnapshot(string name, int num, float time = 0)
   {
      SLM_AudioMix_Block b = blocks.Find(f => f.name == name);

      if (b != null)
      {
         b.shapshots[num].TransitionTo(time);
      }
   }
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