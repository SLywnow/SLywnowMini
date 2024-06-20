using SLywnow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SLM_RPGTextBlock : MonoBehaviour
{
   public int id;
   [ShowFromBool(nameof(useExternal),false)]
   public Text text;
   public SLM_RPGText mainsc;
   public Button button;
   public void Clicked()
   {
      mainsc.SelectPoll(id);
   }

   public bool useExternal;
   [ShowFromBool(nameof(useExternal))]
   public SLM_RPGTextBlock_Event events;
}

[System.Serializable]
public class SLM_RPGTextBlock_Event
{
   public UnityEvent<string> OnGen;
}