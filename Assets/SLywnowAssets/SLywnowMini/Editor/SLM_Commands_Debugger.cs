using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SLM_Commands_Debugger : EditorWindow
{
	SLM_Commands main;
	int shiftup = 5;
	int shiftdown = 5;
	bool first;

	void Awake()
	{
		if (main == null)
			main = FindObjectOfType<SLM_Commands>();
	}

	void OnGUI()
	{
		if (EditorApplication.isPlaying)
		{
			if (!first)
			{
				if (main == null)
					main = FindObjectOfType<SLM_Commands>();

				first = true;
			}

			if (GUILayout.Button("Find commands"))
			{
				main = FindObjectOfType<SLM_Commands>();
			}

			main = EditorGUILayout.ObjectField("Commands script", main, typeof(SLM_Commands), true) as SLM_Commands;
			shiftup = EditorGUILayout.IntField("Displaying the commands after", shiftup);
			shiftdown = EditorGUILayout.IntField("Displaying the commands before", shiftdown);

			GUILayout.Space(20);


			if (main != null)
			{
				if (main.currentid == -1)
				{
					GUILayout.Label("Waiting for the block to start");
				}
				else
				{
					for (int i = main.currentcommand - shiftdown; i < main.currentcommand + shiftup; i++)
					{
						if (i >= 0 && i < main.blocks[main.currentid].commands.Count)
						{
							GUILayout.BeginHorizontal();

							if (i == main.currentcommand)
							{
								GUILayout.Label("=> "+i, GUILayout.Width(50));
								GUILayout.TextField(main.blocks[main.currentid].commands[i]);
							}
							else
							{
								GUILayout.Label(i + "", GUILayout.Width(50));
								GUILayout.TextField(main.blocks[main.currentid].commands[i]);
							}

							GUILayout.EndHorizontal();
						}
					}

					//debug data
					GUILayout.BeginHorizontal();
					GUILayout.Label("Current text: " + main.curtext);
					if (main.imagesc != null && !string.IsNullOrEmpty(main.curimglayer))
						GUILayout.Label("Last image change: " + main.curimgnum + " in " + main.curimglayer);
					GUILayout.EndHorizontal();

					if (GUILayout.Button("Force next command"))
						main.RunNextCommand();
				}
			}
			else
				GUILayout.Label("Add Command object!");
		}
		else
			GUILayout.Label("Script works only when game running!");
	}
}


public class SLM_Commands_DebuggerManager : Editor
{
	[MenuItem("SLywnow/SLM/Commands debugger")]
	static void SetDirection()
	{
		EditorWindow.GetWindow(typeof(SLM_Commands_Debugger), false, "SLM Commands Debugger", true);
	}
}