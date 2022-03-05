using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SLM_Ex_CommandTest : MonoBehaviour
{
    public SLM_Commands sc;

	public void Start()
	{
		sc.RunBlock(0);
	}
}
