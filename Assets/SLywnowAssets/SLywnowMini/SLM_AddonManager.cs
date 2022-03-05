using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class SLM_AddonManager : MonoBehaviour
{
    public List<string> names;
    public delegate bool Addon(string command, string[] args);
    public List<Addon> Addons = new List<Addon>();

    List<string> commandsList = new List<string>();
    List<bool> runnextcommandList = new List<bool>();

    [HideInInspector] public bool runnextcommandInfo;

    public void AddAddon(string addonName, Addon add, List<string> commands, List<bool> runnextcommand)
	{
        if (!names.Contains(addonName))
        {
            if (commands.Count == runnextcommand.Count) {
                Addons.Add(add);
                names.Add(addonName);

                foreach (string s in commands)
                    commandsList.Add(s);
                foreach (bool b in runnextcommand)
                    runnextcommandList.Add(b);
            }
            else
			{
                Debug.LogError("Size of commands and runnextcommand not match!");
            }
        }
        else
		{
            Debug.LogError("Addon with this name already exist!");
        }
    } 

    public bool RunAddon(string command, string[] args)
	{
        bool ret = false;

        foreach (Addon a in Addons)
		{
            if (a.Invoke(command, args))
			{
                ret = true;

                if (commandsList.Contains(command))
                    runnextcommandInfo = runnextcommandList[commandsList.IndexOf(command)];
                else
                    runnextcommandInfo = true;
                break;
            }
        }

        return ret;
	}
}
