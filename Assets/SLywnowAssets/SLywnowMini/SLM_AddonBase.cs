using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SLM_AddonBase : MonoBehaviour
{
    public SLM_Commands commandScript;

	public List<string> commands;
	public List<bool> nextcommands;
	public void AddCommand(string command, bool runNextCommand = true)
	{
		commands.Add(command);
		nextcommands.Add(runNextCommand);
	}

	public void Initialize(string name, SLM_AddonManager.Addon add)
	{
		if (commandScript != null)
			commandScript.addonManager.AddAddon(name, add, commands, nextcommands);
	}


}
