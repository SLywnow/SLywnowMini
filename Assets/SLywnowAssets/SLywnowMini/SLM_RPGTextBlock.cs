using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SLM_RPGTextBlock : MonoBehaviour
{
    public int id;
    public Text text;
    public SLM_RPGText mainsc;
    public Button button;
    public void Clicked()
	{
        mainsc.SelectPoll(id);
    }
}
