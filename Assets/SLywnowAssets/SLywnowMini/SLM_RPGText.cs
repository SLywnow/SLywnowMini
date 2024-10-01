using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using AutoLangSLywnow;
using System.Linq;
using SLywnow;
using Unity.Burst.Intrinsics;
using static UnityEngine.Timeline.AnimationPlayableAsset;

public class SLM_RPGText : MonoBehaviour
{
	public bool useALSL;
	public bool checkStats;
	public SLM_Stats stats;
	public AudioSource audioSource;
	public bool useHideButton=true;
	[ShowFromBool("useHideButton")]
	public bool keyCode=true;
	[ShowFromMultiple(new string[2] { "useHideButton", "keyCode" },"true","bool",ShowFromMultipleAttribute.mode.and)]
	public KeyCode key=KeyCode.H;
	[ShowFromMultiple(new string[2] { "useHideButton", "keyCode" }, new string[2] { "true", "false" }, "bool", ShowFromMultipleAttribute.mode.and)]
	public string keyString;
	public SLM_RPGText_Text textopt;
	public bool autoSetUpZeroTextPreset;
	public List<SLM_RPGText_TextObject> textPresets;
	public SLM_RPGText_Poll poll;
	public bool autoSetUpZeroPollPreset;
	public List<SLM_RPGText_Poll> pollpresets;

	string[] words;
	int curentchar = -1;
	[HideInInspector] public string showtext;
	[HideInInspector] public string fulltext;
	[HideInInspector] public Color charcolor;
	[HideInInspector] public AudioClip charvoice;

	public enum cur { none, text, poll };
	[HideInInspector] public cur currentState;

	float stimer;
	[HideInInspector] public float timer;

	private void Awake()
	{
		if (autoSetUpZeroPollPreset && pollpresets.Count > 0)
			poll = pollpresets[0];

		if (poll.mainPollObj != null)
		{
			if (!poll.autogen)
			{
				for (int i = 0; i < poll.blocks.Count; i++)
				{
					poll.blocks[i].gameObject.SetActive(false);
				}
			}
			else
			{
				poll.spawnobj.gameObject.SetActive(false);
			}
			if (poll.timershow != null)
				poll.timershow.gameObject.SetActive(false);
			if (poll.timershowText != null)
				poll.timershowText.gameObject.SetActive(false);
			poll.mainPollObj.SetActive(false);
			poll.type = SLM_RPGText_Poll.tpe.off;

			for (int a = 0; a < pollpresets.Count; a++)
			{
				if (!pollpresets[a].autogen)
				{
					for (int i = 0; i < pollpresets[a].blocks.Count; i++)
					{
						pollpresets[a].blocks[i].gameObject.SetActive(false);
					}
				}
				else
				{
					pollpresets[a].spawnobj.gameObject.SetActive(false);
				}
				if (pollpresets[a].timershow != null)
					pollpresets[a].timershow.gameObject.SetActive(false);
				if (pollpresets[a].timershowText != null)
					pollpresets[a].timershowText.gameObject.SetActive(false);
				pollpresets[a].mainPollObj.SetActive(false);
				pollpresets[a].type = SLM_RPGText_Poll.tpe.off;
			}
		}

		for (int i=0;i< textPresets.Count;i++)
		{
			textPresets[i].mainTextObj.SetActive(true);
			if (textopt.bg != null)
				textopt.bg.SetActive(true);

			if (textPresets[i].text != null && !textPresets[i].useExternalWrite)
				textPresets[i].text.gameObject.SetActive(false);
			if (textPresets[i].charlogo != null)
				textPresets[i].charlogo.gameObject.SetActive(false);
			if (textPresets[i].charIcon != null)
				textPresets[i].charIcon.gameObject.SetActive(false);
			if (textPresets[i].charEmotionLayer != null)
				textPresets[i].charEmotionLayer.gameObject.SetActive(false);
			if (textPresets[i].auto != null)
				textPresets[i].auto.gameObject.SetActive(false);
			if (textPresets[i].bg != null)
				textPresets[i].bg.SetActive(false);
			textPresets[i].mainTextObj.SetActive(false);
		}

		if (autoSetUpZeroTextPreset && textPresets.Count > 0)
		{
			textopt.mainTextObj = textPresets[0].mainTextObj;
			textopt.text = textPresets[0].text;
			textopt.charlogo = textPresets[0].charlogo;
			textopt.charIcon = textPresets[0].charIcon;
			textopt.charEmotionLayer = textPresets[0].charEmotionLayer;
			textopt.bg = textPresets[0].bg;
			textopt.auto = textPresets[0].auto;
			textopt.useExternalWrite = textPresets[0].useExternalWrite;
			textopt.events = textPresets[0].events;

			if (textopt.text != null && !textopt.useExternalWrite)
				textopt.text.gameObject.SetActive(true);
			if (textopt.charlogo != null && !textopt.useExternalWrite)
				textopt.charlogo.gameObject.SetActive(true);
			if (textopt.charIcon != null)
				textopt.charIcon.gameObject.SetActive(true);
			if (textopt.charEmotionLayer != null)
				textopt.charEmotionLayer.gameObject.SetActive(true);
			if (textopt.bg != null)
				textopt.bg.SetActive(true);
			if (textopt.auto != null)
				textopt.auto.gameObject.SetActive(true);
		}

		if (textopt.mainTextObj != null)
		{
			textopt.mainTextObj.SetActive(false);
			textopt.type = SLM_RPGText_Text.tpe.off;
			if (textopt.charlogo != null && !textopt.useExternalWrite)
				textopt.charlogo.text = "";
			if (textopt.charIcon != null)
			{
				textopt.charIcon.sprite = null;
				textopt.charIcon.color = new Color(0, 0, 0, 0);
			}
			if (textopt.charEmotionLayer != null)
			{
				textopt.charEmotionLayer.sprite = null;
				textopt.charEmotionLayer.color = new Color(0, 0, 0, 0);
			}
		}
		currentState = cur.none;
	}

	public void SetPreset(bool text, int id)
	{
		if (!text)
		{
			if (id < pollpresets.Count && id >= 0 && currentState != cur.poll)
			{
				poll.mainPollObj.SetActive(false);
				poll = pollpresets[id];
			}
		}
		else
		{
			if (id < textPresets.Count && id >= 0 && currentState != cur.text)
			{
				//Debug.Log(id);
				Sprite iconlayer = null;
				Sprite emotionlayer = null;
				Color charIconC = Color.white;
				Color charemoC = Color.white;
				string charlogo = "";

				textopt.mainTextObj.SetActive(false);
				if (textopt.bg != null)
					textopt.bg.SetActive(true);
				if (textopt.text != null && !textopt.useExternalWrite)
					textopt.text.gameObject.SetActive(false);
				if (textopt.charlogo != null && !textopt.useExternalWrite)
				{
					charlogo = textopt.charlogo.text;
					textopt.charlogo.gameObject.SetActive(false);
				}

				if (textopt.charIcon != null)
				{
					iconlayer = textopt.charIcon.sprite;
					charIconC = textopt.charIcon.color;
					textopt.charIcon.gameObject.SetActive(false);
				}
				if (textopt.charEmotionLayer != null)
				{
					emotionlayer = textopt.charEmotionLayer.sprite;
					charemoC = textopt.charEmotionLayer.color;
					textopt.charEmotionLayer.gameObject.SetActive(false);
				}
				if (textopt.auto != null)
				{
					textopt.auto.gameObject.SetActive(false);
					if (textPresets[id].auto != null)
						textPresets[id].auto.isOn = textopt.auto.isOn;
				}

				if (textopt.bg != null)
					textopt.bg.SetActive(false);

				textopt.mainTextObj = textPresets[id].mainTextObj;
				textopt.useExternalWrite = textPresets[id].useExternalWrite;
				textopt.events = textPresets[id].events;

				textopt.mainTextObj.SetActive(true);
				if (textPresets[id].text != null && !textPresets[id].useExternalWrite)
				{
					textopt.text = textPresets[id].text;
					textopt.text.gameObject.SetActive(true);
				}
				else
					textopt.text = null;

				textopt.charlogo = textPresets[id].charlogo;

				textopt.charIcon = textPresets[id].charIcon;
				textopt.charEmotionLayer = textPresets[id].charEmotionLayer;
				textopt.auto = textPresets[id].auto;
				textopt.bg = textPresets[id].bg;

				if (textopt.bg != null)
					textopt.bg.SetActive(true);
				if (textopt.charlogo != null && !textopt.useExternalWrite)
				{
					textopt.charlogo.gameObject.SetActive(true);
					textopt.charlogo.text = charlogo;
					textopt.charlogo.color = charcolor;
				}
				if (textopt.useExternalWrite)
				{
					textopt.events.OnChangeCharName.Invoke(charlogo, charcolor == new Color(0, 0, 0, 0) ? new Color(1, 1, 1, 1) : charcolor);
				}

				if (textopt.charIcon != null)
				{
					textopt.charIcon.gameObject.SetActive(true);
					textopt.charIcon.sprite = iconlayer;
					textopt.charIcon.color = charIconC;
				}
				if (textopt.charEmotionLayer != null)
				{
					textopt.charEmotionLayer.gameObject.SetActive(true);
					textopt.charEmotionLayer.sprite = emotionlayer;
					textopt.charEmotionLayer.color = charemoC;
				}
				if (textopt.bg != null)
					textopt.bg.SetActive(true);
				if (textopt.auto != null)
					textopt.auto.gameObject.SetActive(true);

				if (textopt.text != null && !textopt.useExternalWrite)
				{
					if (charcolor != new Color(0, 0, 0, 0))
					{
						textopt.text.color = charcolor;
						textopt.colorSave = textPresets[id].defaultTextColor;
					}
					else
					{
						textopt.text.color = textPresets[id].defaultTextColor;
					}
				}

				textopt.mainTextObj.SetActive(false);
			}
		}
	}


	bool textHide;
	bool pollHide;
	public void Update()
	{
		if (currentState == cur.text && !string.IsNullOrEmpty(fulltext))
		{
			if (!(textopt.showMode == SLM_RPGText_Text.mde.fullshow))
			{
				if (!string.Equals(showtext,fulltext) || (showtext.Length < fulltext.Length))
				{
					if (textopt.showMode == SLM_RPGText_Text.mde.animBySymbols)
					{
						if (curentchar == -1)
						{
							showtext = "";
							curentchar = 0;
							stimer = 0;
						}
						else
						{
							if (stimer < 1 / textopt.speedShow) stimer += Time.deltaTime;
							else
							{
								if (fulltext[curentchar] == '<')
								{
									string addtag = "";
									int addchars = 0;
									for (int i = curentchar; i < fulltext.Length; i++)
									{
										addtag += fulltext[i];
										addchars++;
										if (fulltext[i] == '>')
											break;
									}
									curentchar += addchars;
									showtext += addtag;
								}

								if (curentchar < fulltext.Length)
									showtext += fulltext[curentchar];

								curentchar++;
								stimer = 0;
								if (audioSource != null && charvoice !=null)
								{
									audioSource.Stop();
								}
							}
						}
					}
					if (textopt.showMode == SLM_RPGText_Text.mde.animByWords)
					{
						if (curentchar == -1)
						{
							showtext = "";
							words = fulltext.Split(' ');
							curentchar = 0;
							stimer = 0;
						}
						else
						{
							if (stimer < 1 / textopt.speedShow) stimer += Time.deltaTime;
							else
							{
								if (curentchar > 0)
									showtext += " ";
								showtext += words[curentchar];
								curentchar++;
								stimer = 0;
								if (audioSource != null && charvoice != null)
								{
									audioSource.Stop();
								}
							}
						}
					}

					if (!textopt.useExternalWrite)
					{
						if (textopt.text != null)
							textopt.text.text = showtext;
					}
					else
					{
						textopt.events.OnTextWrite?.Invoke(showtext);
					}
				}
				else
				{
					stimer = 0;
					curentchar = -1;
					if (audioSource != null && charvoice != null)
					{
						audioSource.Stop();
					}
				}
			}

			if (textopt.timer > 0 && ((showtext == fulltext) || (showtext.Length >= fulltext.Length)) && (textopt.auto != null && textopt.auto.isOn))
			{
				if (timer < textopt.timer) timer += Time.deltaTime;
				else
				{
					timer = 0;
					EndShowText();
				}
			}
			if (textopt.auto == null || !textopt.auto.isOn)
			{
				if (useHideButton)
				{
					if (keyCode)
					{
						if (Input.GetKeyDown(key))
						{
							if (!textHide)
							{
								textopt.mainTextObj.SetActive(false);
							}
							else
							{
								textopt.mainTextObj.SetActive(true);
							}
							textHide = !textHide;
						}
					}
					else
					{
						if (Input.GetButtonDown(keyString))
						{
							if (!textHide)
							{
								textopt.mainTextObj.SetActive(false);
							}
							else
							{
								textopt.mainTextObj.SetActive(true);
							}
							textHide = !textHide;
						}
					}
				}
			}
		}
		if (currentState == cur.poll)
		{
			if (!(poll.timer==-1))
			{
				if (timer < poll.timer)
				{
					timer += Time.deltaTime;
					if (poll.timershow != null)
						poll.timershow.value = 1 - (timer / poll.timer);
					if (poll.timershowText != null)
						poll.timershowText.text = (int)(poll.timer-timer) + "";
				}
				else
				{
					int ans = poll.nonRandomChoice;
					if (poll.RandomChoiceAfterEndOfTimmer)
						if (poll.type == SLM_RPGText_Poll.tpe.commands)
							ans = Random.Range(0, poll.commands.Length);
						else if (poll.type == SLM_RPGText_Poll.tpe.events)
							ans = Random.Range(0, poll.events.Length);

					SelectPoll(ans);
				}
			}
			else
			{
				if (useHideButton)
				{
					if (keyCode)
					{
						if (Input.GetKeyDown(key))
						{
							if (!pollHide)
							{
								poll.mainPollObj.SetActive(false);
							}
							else
							{
								poll.mainPollObj.SetActive(true);
							}
							pollHide = !pollHide;
						}
					}
					else
					{
						if (Input.GetButtonDown(keyString))
						{
							if (!pollHide)
							{
								poll.mainPollObj.SetActive(false);
							}
							else
							{
								poll.mainPollObj.SetActive(true);
							}
							pollHide = !pollHide;
						}
					}
				}
			}
		}
	}

	public void ForseWriteFullText()
	{
		if (currentState == cur.text)
		{
			showtext = fulltext;

			if (!textopt.useExternalWrite)
			{
				if (textopt.text != null)
					textopt.text.text = showtext;
			}
			else
			{
				textopt.events.OnTextWrite?.Invoke(showtext);
			}
		}
	}

	public void ShowText(string text, SLM_Commands commsc, bool offafternext)
	{
		textopt.type = SLM_RPGText_Text.tpe.commands;
		currentState = cur.text;
		textopt.comm = commsc;

		textopt.offafternext = offafternext;
		textopt.mainTextObj.SetActive(true);
		if (textopt.bg != null)
			textopt.bg.SetActive(true);
		if (useALSL)
		{
			if (commsc.ALSLBridge == null)
				fulltext = ALSL_Main.GetWorldAndFindKeys(text);
			else
				fulltext = ALSL_Main.FindKeysInString(text);
			//Debug.Log(fulltext);
		}
		else
		{
			fulltext = text;
			if (checkStats)
			{
				foreach (SLM_Stats_Block b in stats.stats)
				{
					if (!string.IsNullOrEmpty(b.ALSLkey) && fulltext.Contains(b.ALSLkey))
					{
						fulltext = fulltext.Replace(b.ALSLkey, b.value);
					}
				}
			}
		}

		if (textopt.showMode == SLM_RPGText_Text.mde.fullshow)
		{
			showtext = fulltext;

			if (!textopt.useExternalWrite)
			{
				if (textopt.text != null)
					textopt.text.text = fulltext;
			}
			else
			{
				textopt.events.OnTextWrite?.Invoke(fulltext);
			}
		}
		else
		{
			if (audioSource !=null && charvoice !=null)
			{
				audioSource.clip = charvoice;
				audioSource.Play();
			}
		}
		if (textopt.activestory)
			textopt.story.Add(fulltext);

		if (textopt.useExternalWrite)
			textopt.events.OnBeginWrite.Invoke(fulltext);
	}

	public void ShowText(string text, bool offafternext)
	{
		textopt.type = SLM_RPGText_Text.tpe.basic;
		currentState = cur.text;

		textopt.offafternext = offafternext;
		textopt.mainTextObj.SetActive(true);
		if (textopt.bg != null)
			textopt.bg.SetActive(true);
		if (useALSL)
		{
			fulltext = ALSL_Main.GetWorldAndFindKeys(text);
		}
		else
		{
			fulltext = text;
			if (checkStats)
			{
				foreach (SLM_Stats_Block b in stats.stats)
				{
					fulltext = fulltext.Replace(b.ALSLkey, b.value);
				}
			}
		}

		if (textopt.showMode == SLM_RPGText_Text.mde.fullshow)
		{
			showtext = fulltext;
			if (!textopt.useExternalWrite)
			{
				if (textopt.text != null)
					textopt.text.text = fulltext;
			}
			else
			{
				textopt.events.OnTextWrite?.Invoke(fulltext);
			}
		}
		else
		{
			if (audioSource != null && charvoice != null)
			{
				audioSource.clip = charvoice;
				audioSource.Play();
			}
		}
		if (textopt.activestory)
			textopt.story.Add(fulltext);
	}

	public void EndShowText()
	{
		EndShowText(false);
	}

	public void EndShowText(bool noRunComands=false)
	{
		if ((!(!(showtext == fulltext) || (showtext.Length < fulltext.Length))) || noRunComands)
		{
			if (textopt.type != SLM_RPGText_Text.tpe.off)
			{
				showtext = "";
				fulltext = "";
				words = new string[0];
				curentchar = -1;

				if (!textopt.useExternalWrite)
				{
					if (textopt.text != null)
						textopt.text.text = "";
				}
				else
				{
					textopt.events.OnTextWrite?.Invoke("");
				}

				timer = 0;
				currentState = cur.none;
				textHide = false;
				if (textopt.offafternext)
				{
					textopt.mainTextObj.SetActive(false);
					if (textopt.bg != null)
						textopt.bg.SetActive(false);
				}

				if (!noRunComands)
				{
					if (textopt.type == SLM_RPGText_Text.tpe.commands)
					{
						textopt.type = SLM_RPGText_Text.tpe.off;
						textopt.comm.RunNextCommand();
					}
					if (textopt.type == SLM_RPGText_Text.tpe.basic)
					{
						textopt.type = SLM_RPGText_Text.tpe.off;
						textopt.runAfterClick?.Invoke();
					}
				}
			}
		} else
		{
			showtext = fulltext;

			if (!textopt.useExternalWrite)
			{
				if (textopt.text != null)
					textopt.text.text = showtext;
			}
			else
			{
				textopt.events.OnTextWrite?.Invoke(showtext);
			}

			stimer = 0;
			curentchar = -1;
			if (audioSource != null && charvoice != null)
			{
				audioSource.Stop();
			}
		}
	}

	public void RunPoll(string text, string[] texts, int[] commands, SLM_Commands commsc, float time = -1,int defaultChoice = -2)
	{
		if (texts.Length == commands.Length)
		{
			poll.type = SLM_RPGText_Poll.tpe.commands;
			poll.commands = commands;
			RunPollGen(text, texts, null, commsc, time, defaultChoice, false, true);
		}
		else
			Debug.LogError("The number of texts and commands does not match!");
	}
	public void RunPoll(string text, string[] texts, UnityEvent[] events, SLM_Commands commsc, float time = -1, int defaultChoice=-2)
	{
		if (texts.Length == events.Length)
		{
			poll.type = SLM_RPGText_Poll.tpe.events;
			poll.events = events;
			RunPollGen(text, texts, null, commsc, time, defaultChoice, false, true);
		}
		else
			Debug.LogError("The number of texts and events does not match!");
	}

	public void RunPoll(string text, string[] texts, int[] commands, bool[] bools, SLM_Commands commsc, float time = -1, int defaultChoice = -2, bool showdisabled=false)
	{
		if (texts.Length == commands.Length && commands.Length== bools.Length)
		{
			poll.type = SLM_RPGText_Poll.tpe.commands;
			poll.commands = commands;
			RunPollGen(text, texts, bools, commsc, time, defaultChoice, true, showdisabled);
		}
		else
			Debug.LogError("The number of texts and commands does not match!");
	}

	void RunPollGen(string text, string[] texts, bool[] bools, SLM_Commands commsc, float time = -1, int defaultChoice = -2, bool boolmode = false, bool showdisabled = false)
	{
		currentState = cur.poll;
		poll.timer = time;
		if (!(time == -1))
		{
			if (poll.timershow != null)
				poll.timershow.gameObject.SetActive(true);
			if (poll.timershowText != null)
				poll.timershowText.gameObject.SetActive(true);
		}
		poll.comm = commsc;

		poll.nonRandomIntSave = poll.nonRandomChoice;
		poll.isRandomSave = poll.RandomChoiceAfterEndOfTimmer;
		if (defaultChoice == -1)
			poll.RandomChoiceAfterEndOfTimmer = true;
		if (defaultChoice >= 0)
		{
			poll.RandomChoiceAfterEndOfTimmer = false;
			poll.nonRandomChoice = defaultChoice;
		}

		poll.mainPollObj.SetActive(true);
		if (useALSL)
		{
			if (commsc.ALSLBridge == null)
				text = ALSL_Main.GetWorldAndFindKeys(text);
			else
				text = ALSL_Main.FindKeysInString(text);
		}
		else
		{
			if (checkStats)
			{
				foreach (SLM_Stats_Block b in stats.stats)
				{
					if (!string.IsNullOrEmpty(b.ALSLkey))
						text = text.Replace(b.ALSLkey, b.value);
				}
			}
		}

		if (!poll.useExternalWrite)
			poll.text.text = text;
		else
		{
			poll.logoEvents.OnLogoWrite?.Invoke(text);
		}

		int max = 0;
		if (poll.autogen)
		{
			for (int i = poll.parent.childCount - 1; i >= 0; i--)
			{
				Destroy(poll.parent.GetChild(i).gameObject);
			}

			if (texts.Length > poll.maxchoice)
				max = poll.maxchoice;
			else
				max = texts.Length;

			for (int i = 0; i < max; i++)
			{
				if (!boolmode || bools[i] || showdisabled)
				{
					GameObject obj = Instantiate(poll.spawnobj.gameObject, poll.parent);
					obj.SetActive(true);
					poll.blocks.Add(obj.GetComponent<SLM_RPGTextBlock>());

					if (useALSL && commsc.ALSLBridge == null)
						texts[i] = ALSL_Main.GetWord(texts[i]);

					/*poll.blocks[poll.blocks.Count - 1].id = i;
					poll.blocks[poll.blocks.Count - 1].mainsc = this;
					if (!poll.blocks[poll.blocks.Count - 1].useExternal)
					{
						if (poll.blocks[poll.blocks.Count - 1].text != null)
							poll.blocks[poll.blocks.Count - 1].text.text = texts[i];
					}
					else
					{
						poll.blocks[poll.blocks.Count - 1].events.OnGen?.Invoke(texts[i]);
					}

					if (boolmode && (showdisabled && !bools[i]))
						poll.blocks[poll.blocks.Count - 1].button.interactable = false;*/

					int ii = i;

					GenPollButton(poll.blocks[poll.blocks.Count - 1], ii, texts[ii], (!boolmode || !(showdisabled && !bools[ii])), commsc);
				}
			}
		}
		else
		{
			if (poll.blocks.Count > texts.Length)
				max = texts.Length;
			else
				max = poll.blocks.Count;
			for (int i = 0; i < max; i++)
			{
				if (!boolmode || bools[i] || showdisabled)
				{
					poll.blocks[i].gameObject.SetActive(true);

					if (useALSL && commsc.ALSLBridge == null)
						texts[i] = ALSL_Main.GetWord(texts[i]);

					/*poll.blocks[i].id = i;
					poll.blocks[i].mainsc = this;
					if (!poll.blocks[i].useExternal)
					{
						if (poll.blocks[i].text != null)
							poll.blocks[i].text.text = texts[i];
					}
					else
						poll.blocks[i].events.OnGen?.Invoke(texts[i]);
					if (!boolmode || (showdisabled && !bools[i]))
						poll.blocks[i].button.interactable = false;*/

					int ii = i;

					GenPollButton(poll.blocks[i], ii, texts[ii], (!boolmode || !(showdisabled && !bools[ii])), commsc);
				}
			}
		}

		poll.texts = texts;
		if (poll.useExternalWrite)
			poll.logoEvents.OnPollBegin.Invoke();
	}

	void GenPollButton(SLM_RPGTextBlock block, int id, string text, bool interactable, SLM_Commands commsc)
	{
		block.id = id;
		block.mainsc = this;

		if (!block.useExternal)
		{
			if (block.text != null)
				block.text.text = text;
		}
		else
		{
			block.events.OnGen?.Invoke(text);
		}

		if (block.button == null)
			block.button = block.GetComponent<Button>();
		block.button.interactable = interactable;
	}

	public void SelectPoll(int id)
	{
		ClosePoll();

		if (poll.useExternalWrite)
		{
			poll.logoEvents.OnPollEnd.Invoke(id);
		}

		if (poll.type == SLM_RPGText_Poll.tpe.commands)
		{
			poll.type = SLM_RPGText_Poll.tpe.off;
			poll.comm.RunCommand(poll.commands[id]);
		}
		else if (poll.type == SLM_RPGText_Poll.tpe.events)
		{
			poll.type = SLM_RPGText_Poll.tpe.off;
			poll.comm.RunCommand(id);
		}
	}

	void ClosePoll()
	{
		if (poll.autogen)
		{
			for (int i = poll.parent.childCount - 1; i >= 0; i--)
			{
				Destroy(poll.parent.GetChild(i).gameObject);
			}
			poll.blocks = new List<SLM_RPGTextBlock>();
		}
		else
		{
			for (int i=0;i<poll.blocks.Count;i++)
			{
				poll.blocks[i].gameObject.SetActive(false);
			}
		}
		if (poll.timershow != null)
			poll.timershow.gameObject.SetActive(false);
		if (poll.timershowText != null)
			poll.timershowText.gameObject.SetActive(false);
		poll.nonRandomChoice = poll.nonRandomIntSave;
		poll.RandomChoiceAfterEndOfTimmer = poll.isRandomSave;
		currentState = cur.none;
		timer = 0;
		pollHide = false;
		poll.mainPollObj.SetActive(false);
	}

	public void ForseStopPoll()
	{
		poll.type = SLM_RPGText_Poll.tpe.off;
		if (poll.autogen)
		{
			for (int i = poll.parent.childCount - 1; i >= 0; i--)
			{
				Destroy(poll.parent.GetChild(i).gameObject);
			}
			poll.blocks = new List<SLM_RPGTextBlock>();
		}
		else
		{
			for (int i = 0; i < poll.blocks.Count; i++)
			{
				poll.blocks[i].gameObject.SetActive(false);
			}
		}
		if (poll.timershow != null)
			poll.timershow.gameObject.SetActive(false);
		if (poll.timershowText != null)
			poll.timershowText.gameObject.SetActive(false);
		poll.nonRandomChoice = poll.nonRandomIntSave;
		poll.RandomChoiceAfterEndOfTimmer = poll.isRandomSave;
		currentState = cur.none;
		timer = 0;
		pollHide = false;
		poll.mainPollObj.SetActive(false);

		if (poll.useExternalWrite)
			poll.logoEvents.OnPollEnd.Invoke(-1);
	}
}

[System.Serializable]
public class SLM_RPGText_Text
{
	public GameObject mainTextObj;
	public GameObject bg;
	[ShowFromBool(nameof(useExternalWrite), false)]
	public Text text;
	[ShowFromBool(nameof(useExternalWrite), false)]
	public Text charlogo;
	public Image charIcon;
	public Image charEmotionLayer;
	public Toggle auto;
	public float timer = -1;
	public enum mde {animBySymbols,animByWords, fullshow};
	public mde showMode;
	[ShowFromEnum("showMode", 2, true)]
	public float speedShow;

	public bool useExternalWrite;

	[ShowFromBool(nameof(useExternalWrite))]
	public SLM_RPGText_TextObject_Event events;

	public bool activestory;
	public List<string> story;

	public enum tpe { off,commands, basic };
	[HideInInspector] public tpe type;
	[HideInInspector] public bool offafternext;
	[HideInInspector] public SLM_Commands comm;
	[HideInInspector] public Color colorSave;
	public UnityEvent runAfterClick;
}

[System.Serializable]
public class SLM_RPGText_TextObject
{
	public GameObject mainTextObj;
	public GameObject bg;
	//[ShowFromBool(nameof(useExternalWrite), false)]
	public Text text;
	[ShowFromBool(nameof(useExternalWrite), false)]
	public Text charlogo;
	public Image charIcon;
	public Image charEmotionLayer;
	public Toggle auto;
	public Color defaultTextColor;

	public bool useExternalWrite;

	[ShowFromBool(nameof(useExternalWrite))]
	public SLM_RPGText_TextObject_Event events;
}

[System.Serializable]
public class SLM_RPGText_TextObject_Event
{
	public UnityEvent<string> OnTextWrite;
	public UnityEvent<string, Color> OnChangeCharName;

	public UnityEvent<string> OnBeginWrite;

}

[System.Serializable]
public class SLM_RPGText_Poll
{
	public GameObject mainPollObj;
	[ShowFromBool(nameof(useExternalWrite), false)]
	public Text text;
	public Slider timershow;
	public Text timershowText;
	public int maxchoice = 10;
	public bool RandomChoiceAfterEndOfTimmer;
	public int nonRandomChoice = 0;
	[HideInInspector] public int nonRandomIntSave;
	[HideInInspector] public bool isRandomSave;
	public bool autogen;
	[ShowFromBool("autogen", false)]
	public List<SLM_RPGTextBlock> blocks;
	[ShowFromBool("autogen")]
	public Transform parent;
	[ShowFromBool("autogen")]
	public SLM_RPGTextBlock spawnobj;

	public bool useExternalWrite;

	[ShowFromBool(nameof(useExternalWrite))]
	public SLM_RPGText_Poll_Event logoEvents;

	public enum tpe { off, commands, events };
	[HideInInspector] 
	public tpe type = 0;
	[HideInInspector] 
	public float timer = -1;
	[HideInInspector] 
	public SLM_Commands comm;
	[HideInInspector] 
	public int[] commands;
	[HideInInspector] 
	public UnityEvent[] events;
	[HideInInspector]
	public string[] texts;
}

[System.Serializable]
public class SLM_RPGText_Poll_Event
{
	public UnityEvent<string> OnLogoWrite;
	public UnityEvent OnPollBegin;
	public UnityEvent<int> OnPollEnd;
}