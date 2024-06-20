using AutoLangSLywnow;
using SLywnow;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SLM_Commands : MonoBehaviour
{
    public SLM_RPGText textsc;
    public SLM_NovelImages imagesc;
    public SLM_AudioMix audiosc;
    public SLM_CharAI charAI;
    public SLM_Stats stats;
    public SLM_QTEEvents qte;
    public SLM_ALSLBridge ALSLBridge;
    public SLM_AddonManager addonManager;
    public bool stopIfError = true;
    public bool autoRunFirstBlock = true;
    public List<SLM_Commands_Block> blocks;
    [HideInInspector] public int currentid=-1;
    [HideInInspector] public int currentcommand;

    public delegate void setError(string text);
    public setError onError;

    public delegate void setCustom(string text);
    public setError CustomCC;


    float timer=-1;
    List<int> pointid = new List<int>();
    List<string> pointContain = new List<string>();
    List<int> pointidtext = new List<int>();
    List<string> pointContaintext = new List<string>();
    [HideInInspector] public int curtext;
    [HideInInspector] public string curimglayer;
    [HideInInspector] public int curimgnum;
    [HideInInspector] public string lastpoint;
    List<int> savecurcomm = new List<int>();
    List<int> saveloopcom = new List<int>();
    List<int> loopcount = new List<int>();

    private void Start()
	{
        if (!SaveSystemAlt.IsWorking())
            SaveSystemAlt.StartWork();
        if (autoRunFirstBlock && blocks.Count > 0)
            RunBlock(0);
	}

	private void Update()
	{
        //Debug.Log(timer);
        if (timer > 0) timer -= Time.deltaTime;
        else
        {
            if (timer != -1)
            {
                timer = -1;
                RunNextCommand();
            }
        }
	}

	public void RunBlock(int id, bool onlyload=false)
	{
        currentid = id;
        currentcommand = 0;
        savecurcomm.Clear();
        saveloopcom.Clear();
        loopcount.Clear();

        if (blocks[id].files.useLoadFromFiles)
		{
            List<string> comms = new List<string>();
            List<string> txts = new List<string>();

            if (!blocks[id].files.useTextObject)
            {
                if (blocks[id].files.useSLMContainer)
                {
                    if (blocks[id].files.container.blocks.Count > 0)
                    {
                        if (blocks[id].files.container.blocks[0].singlefile && blocks[id].files.container.blocks[0].single.type == SLM_Container.tpe.text)
                        {
                            blocks[id].files.container.LoadBlock(0);
                            comms = blocks[id].files.container.blocks[0].single.containertext.ToList();
                        }
                        if (blocks[id].files.container.blocks.Count > 1)
                        {
                            if (blocks[id].files.container.blocks[1].singlefile && blocks[id].files.container.blocks[1].single.type == SLM_Container.tpe.text)
                            {
                                blocks[id].files.container.LoadBlock(1);
                                txts = blocks[id].files.container.blocks[1].single.containertext.ToList();
                            }
                        }
                    }
                }
                else
                {
                    if (!blocks[id].files.useStreamingAssets)
                    {
                        if (!string.IsNullOrEmpty(blocks[id].files.pathToCommands) && !(blocks[id].files.pathToCommands == "off"))
                        {
                            blocks[id].files.pathToCommands = blocks[id].files.pathToCommands.Replace("<docs>", FastFind.GetDefaultPath());
                            blocks[id].files.pathToCommands = blocks[id].files.pathToCommands.Replace("<dp>", Application.dataPath);
                            comms = FilesSet.LoadStream(blocks[id].files.pathToCommands, false).ToList();
                        }
                        if (!string.IsNullOrEmpty(blocks[id].files.pathToTexts) && !(blocks[id].files.pathToTexts == "off") && !blocks[id].files.useALSL)
                        {
                            blocks[id].files.pathToTexts = blocks[id].files.pathToTexts.Replace("<docs>", FastFind.GetDefaultPath());
                            blocks[id].files.pathToTexts = blocks[id].files.pathToTexts.Replace("<dp>", Application.dataPath);
                            txts = FilesSet.LoadStream(blocks[id].files.pathToTexts, false).ToList();
                        }
                    }

                    if (blocks[id].files.useALSL && !string.IsNullOrEmpty(blocks[id].files.ALSLFolder) && ALSLBridge == null)
                    {
                        string langname = ALSL_Main.alllangs[ALSL_Main.currentlang];
                        string localpath = blocks[id].files.ALSLFolder + "/" + blocks[id].files.prefixALSLFile + langname + ".lang";
                        if (ALSL_Main.IsCurrentLangOutPut())
                        {
                            string outputS = SaveSystemAlt.GetString("OutPutFiles");
                            if (!string.IsNullOrEmpty(outputS))
                                txts = FilesSet.LoadStream(outputS + "/" + localpath, false).ToList();
                        }
                    }
                }
            }
            else
            {
                if (blocks[id].files.commandObject != null)
                {
                    comms = (blocks[id].files.commandObject as TextAsset).text.Replace("\r", "").Trim((char)8203).Split('\n').ToList();
                }

                if (ALSLBridge == null && blocks[id].files.textObject != null)
                    txts = (blocks[id].files.textObject as TextAsset).text.Replace("\r", "").Trim((char)8203).Split('\n').ToList();
            }

            if (blocks[id].files.useALSL && ALSLBridge != null)
            {
                //Debug.Log("bridge");
                ALSLBridge.LoadBlock(id);
                txts = ALSLBridge.loadedBlock;
            }

            if (!(blocks[id].files.pathToCommands=="off") && comms.Count>0)
			{
                blocks[id].commands = comms;
            }
            if (!(blocks[id].files.pathToTexts == "off") && txts.Count > 0)
            {
                blocks[id].texts = txts;
            }
        }

        GetPoints();

        if (!onlyload)
            RunCommand(currentcommand);
    }

    public void GetPoints()
    {
        if (currentid != -1)
        {
            pointid.Clear();
            pointContain.Clear();
            pointidtext.Clear();
            pointContaintext.Clear();
            if (blocks[currentid].usePoints)
            {
                for (int i = 0; i < blocks[currentid].commands.Count; i++)
                {
                    if (blocks[currentid].commands[i].Contains("setpoint") && blocks[currentid].commands[i].Replace("::", "▫").Split('▫').Length == 2)
                    {
                        string comm = blocks[currentid].commands[i];

                        if (comm.IndexOf(";;") >= 0)
                            comm = comm.Remove(comm.IndexOf(";;"));
                        pointid.Add(i);
                        pointContain.Add(comm.Replace("::", "▫").Split('▫')[1]);
                    }
                }
            }
            if (blocks[currentid].useTextPoints)
            {
                for (int i = 0; i < blocks[currentid].texts.Count; i++)
                {
                    if (blocks[currentid].texts[i].Contains("setpoint") && blocks[currentid].texts[i].Replace("::", "▫").Split('▫').Length == 2)
                    {
                        string comm = blocks[currentid].texts[i];

                        if (comm.IndexOf(";;") >= 0)
                            comm = comm.Remove(comm.IndexOf(";;"));
                        pointidtext.Add(i);
                        pointContaintext.Add(comm.Replace("::", "▫").Split('▫')[1]);
                    }
                }
            }
        }
    }

    public void StopBlock()
	{
        currentid = -1;
        currentcommand = 0;
        savecurcomm.Clear();
    }

    public void RunNextCommand()
	{
        currentcommand++;
        RunCommand(currentcommand);
    }

    public void RunPoint (string name)
	{
        if (blocks[currentid].usePoints)
        {
            string point = ValueWorkString(name);
            if (pointContain.Contains(point))
            {
                lastpoint = point;
				RunCommand(pointid[pointContain.IndexOf(point)]);
            }
            else
                Debug.LogError("Block " + currentid + "using point system, but point " + point + " not found! (command: " + currentcommand + ")");
        }
        else
            Debug.LogError("Block " + currentid + " doesn't use point system, set usePoints to true! (command: " + currentcommand + ")");
    }

    public void RunCommand(int id)
	{
        if (currentid >= 0 && (id < blocks[currentid].commands.Count || blocks[currentid].pauseWhenEndFile))
        {

            if (blocks[currentid].pauseWhenEndFile && currentcommand >= blocks[currentid].commands.Count)
            {
                currentcommand = blocks[currentid].commands.Count - 1;
            }
            else
                currentcommand = id;

            string comm = blocks[currentid].commands[id];
            comm = comm.Replace("::", "▫");
            if (comm.IndexOf(";;") >= 0)
                comm = comm.Remove(comm.IndexOf(";;"));
            string[] comms = comm.Split('▫');
            timer = -1;



            try
            {
                switch (comms[0])
                {
                    case "exit":
                        {
                            StopBlock();
                            break;
                        }
                    case "runcommand":
                        {
                            RunCommand(ValueWorkCommand(comms[1]));
                            break;
                        }
                    case "runevent":
                        {
                            blocks[currentid].events[int.Parse(comms[1])].Invoke();
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "restart":
                        {
                            RunBlock(currentid);
                            break;
                        }
                    case "runblock":
                        {
                            RunBlock(int.Parse(comms[1]));
                            break;
                        }
                    case "skip":
                        {
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "setpoint":
                        {
                            lastpoint = comms[1];
							RunNextCommand();
                            break;
                        }
                    case "runpoint":
                        {
                            if (blocks[currentid].usePoints)
                            {
                                string point = ValueWorkString(comms[1]);
                                if (pointContain.Contains(point))
                                    RunCommand(pointid[pointContain.IndexOf(point)]);
                                else
                                    Debug.LogError("Block " + currentid + "using point system, but point " + point + " not found! (command: " + currentcommand + ")");
                            }
                            else
                                Debug.LogError("Block " + currentid + " doesn't use point system, set usePoints to true! (command: " + currentcommand + ")");
                            break;
                        }
                    case "runpointasfunction":
                        {
                            if (blocks[currentid].usePoints)
                            {
                                string point = ValueWorkString(comms[1]);
                                if (pointContain.Contains(point))
                                {
                                    savecurcomm.Add(currentcommand);
                                    RunCommand(pointid[pointContain.IndexOf(point)]);
                                }
                                else
                                    Debug.LogError("Block " + currentid + "using point system, but point " + point + "not found! (command: " + currentcommand + ")");
                            }
                            else
                                Debug.LogError("Block " + currentid + " doesn't use point system, set usePoints to true! (command: " + currentcommand + ")");
                            break;

                        }
                    case "repeat":
                        {
                            if (!saveloopcom.Contains(currentcommand))
                            {
                                if (ValueWorkInt(comms[1]) > 1)
                                {
                                    saveloopcom.Add(currentcommand);
                                    loopcount.Add(ValueWorkInt(comms[1]));
                                }
                            }
                            else
                            {
                                int ind = saveloopcom.IndexOf(currentcommand);
                                loopcount[ind] -= 1;
                                //Debug.Log(loopcount[ind]);

                                if (loopcount[ind] <= 1)
                                {
                                    saveloopcom.RemoveAt(ind);
                                    loopcount.RemoveAt(ind);
                                }

                            }
                            RunNextCommand();
                            break;
                        }
                    case "endrepeat":
                        {
                            if (saveloopcom.Count == 0)
                                RunNextCommand();
                            else
                                RunCommand(saveloopcom[saveloopcom.Count - 1]);
                            break;
                        }
                    case "endpoint":
                        {
                            if (savecurcomm.Count == 0) RunNextCommand();
                            else
                            {
                                int savecomm = savecurcomm[savecurcomm.Count - 1];
                                savecurcomm.RemoveAt(savecurcomm.Count - 1);
                                RunCommand(savecomm + 1);
                            }
                            break;
                        }
                    case "showtext":
                        {
                            if (comms[1] != "<prev>" && comms[1] != "<next>" && comms[1] != "<again>")
                            {
                                curtext = ValueWorkInt(comms[1]);
                                bool off = true;
                                if (comms.Length == 3)
                                    if (!string.IsNullOrEmpty(comms[2]))
                                        off = bool.Parse(comms[2]);
                                textsc.ShowText(blocks[currentid].texts[ValueWorkInt(comms[1])], this, off);
                            }
                            else
                            {
                                if (comms[1] == "<prev>")
                                    curtext -= 1;
                                if (comms[1] == "<next>")
                                    curtext += 1;

                                bool off = true;
                                if (comms.Length == 3)
                                    if (!string.IsNullOrEmpty(comms[2]))
                                        off = bool.Parse(comms[2]);
                                textsc.ShowText(blocks[currentid].texts[curtext], this, off);
                            }
                            break;
                        }
                    case "runpoll":
                        {
                            //Debug.Log("running!");
                            comms[2] = comms[2].Replace("||", "◌");
                            string[] texts = comms[2].Split('◌');
                            for (int i = 0; i < texts.Length; i++)
                                texts[i] = blocks[currentid].texts[ValueWorkInt(texts[i])];
                            comms[3] = comms[3].Replace("||", "◌");
                            string[] commands = comms[3].Split('◌');
                            List<int> commint = new List<int>();
                            for (int i = 0; i < commands.Length; i++)
                                commint.Add(ValueWorkCommand(commands[i]));
                            float timer = -1;
                            if (comms.Length >= 5 && !string.IsNullOrEmpty(comms[4])) timer = ValueWorkFloat(comms[4]);
                            int def = -2;
                            if (comms.Length >= 6 && !string.IsNullOrEmpty(comms[5])) def = ValueWorkInt(comms[5]);

                            if (commint.Count() != texts.Count())
                            {
                                Debug.LogError("The number of texts and commands does not match!  (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[id] + "\" )");
                                onError?.Invoke("The number of texts and commands does not match!  (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[id] + "\" )");
                                return;
                            }

                            textsc.RunPoll(blocks[currentid].texts[ValueWorkInt(comms[1])], texts, commint.ToArray(), this, timer, def);
                            break;
                        }
                    case "rundpoll":
                        {
                            //Debug.Log("running!");
                            comms[2] = comms[2].Replace("||", "◌");
                            string[] texts = comms[2].Split('◌');
                            for (int i = 0; i < texts.Length; i++)
                                texts[i] = blocks[currentid].texts[ValueWorkInt(texts[i])];
                            comms[3] = comms[3].Replace("||", "◌");
                            string[] commands = comms[3].Split('◌');
                            comms[4] = comms[4].Replace("||", "◌");
                            string[] bools = comms[4].Split('◌');
                            List<int> commint = new List<int>();
                            List<bool> boolsreal = new List<bool>();
                            for (int i = 0; i < commands.Length; i++)
                                commint.Add(ValueWorkCommand(commands[i]));
                            for (int i = 0; i < bools.Length; i++)
                                boolsreal.Add(ValueWorkBool(bools[i]));
                            float timer = -1;
                            if (comms.Length >= 6 && !string.IsNullOrEmpty(comms[5])) timer = ValueWorkFloat(comms[5]);
                            int def = -2;
                            if (comms.Length >= 7 && !string.IsNullOrEmpty(comms[6])) def = ValueWorkInt(comms[6]);

                            if (commint.Count() != texts.Count())
                            {
                                Debug.LogError("The number of texts and commands does not match!  (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[id] + "\" )");
                                onError?.Invoke("The number of texts and commands does not match!  (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[id] + "\" )");
                                return;
                            }
                            if (commint.Count() != boolsreal.Count())
                            {
                                Debug.LogError("The number of bools and commands does not match!  (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[id] + "\" )");
                                onError?.Invoke("The number of bools and commands does not match!  (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[id] + "\" )");
                                return;
                            }

                            textsc.RunPoll(blocks[currentid].texts[ValueWorkInt(comms[1])], texts, commint.ToArray(), boolsreal.ToArray(), this, timer, def, false);
                            break;
                        }
                    case "runbpoll":
                        {
                            //Debug.Log("running!");
                            comms[2] = comms[2].Replace("||", "◌");
                            string[] texts = comms[2].Split('◌');
                            for (int i = 0; i < texts.Length; i++)
                                texts[i] = blocks[currentid].texts[ValueWorkInt(texts[i])];
                            comms[3] = comms[3].Replace("||", "◌");
                            string[] commands = comms[3].Split('◌');
                            comms[4] = comms[4].Replace("||", "◌");
                            string[] bools = comms[4].Split('◌');
                            List<int> commint = new List<int>();
                            List<bool> boolsreal = new List<bool>();
                            for (int i = 0; i < commands.Length; i++)
                                commint.Add(ValueWorkCommand(commands[i]));
                            for (int i = 0; i < bools.Length; i++)
                                boolsreal.Add(ValueWorkBool(bools[i]));
                            float timer = -1;
                            if (comms.Length >= 6 && !string.IsNullOrEmpty(comms[5])) timer = ValueWorkFloat(comms[5]);
                            int def = -2;
                            if (comms.Length >= 7 && !string.IsNullOrEmpty(comms[6])) def = ValueWorkInt(comms[6]);

                            if (commint.Count() != texts.Count())
                            {
                                Debug.LogError("The number of texts and commands does not match!  (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[id] + "\" )");
                                onError?.Invoke("The number of texts and commands does not match!  (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[id] + "\" )");
                                return;
                            }
                            if (commint.Count() != boolsreal.Count())
                            {
                                Debug.LogError("The number of bools and commands does not match!  (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[id] + "\" )");
                                onError?.Invoke("The number of bools and commands does not match!  (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[id] + "\" )");
                                return;
                            }

                            textsc.RunPoll(blocks[currentid].texts[ValueWorkInt(comms[1])], texts, commint.ToArray(), boolsreal.ToArray(), this, timer, def, true);
                            break;
                        }
                    case "random":
                        {
                            //Debug.Log("running!");
                            comms[1] = comms[1].Replace("||", "◌");
                            string[] coms = comms[1].Split('◌');
                            RunCommand(ValueWorkCommand(coms[UnityEngine.Random.Range(0, coms.Length)]));
                            break;
                        }
                    case "randomchance":
                        {
                            //randomchance::v1||v2::rnd1||rnd2;;
                            comms[1] = comms[1].Replace("||", "◌");
                            comms[2] = comms[2].Replace("||", "◌");
                            string[] vals = comms[1].Split('◌');
                            string[] coms = comms[2].Split('◌');

                            List<int> valsInt = new List<int>();
                            for (int i=0;i<vals.Length;i++)
							{
                                valsInt.Add(ValueWorkInt(vals[i]));
                            }

                            int maxcount = 0;
                            foreach (int i in valsInt)
                                maxcount += i;

                            int rnd = UnityEngine.Random.Range(0, maxcount + 1);

                            int runid = 0;

                            maxcount = 0;
                            for (int i = 0; i < valsInt.Count; i++)
                            {
                                maxcount += valsInt[i];
                                if (i==0)
								{
                                    if (rnd <= maxcount)
                                    {
                                        //Debug.Log(rnd + " 0 " + maxcount);
                                        runid = i;
                                        break;
                                    }

                                }
                                else if (i== valsInt.Count-1)
								{
                                    //Debug.Log(rnd + " " + (maxcount - valsInt[i]) + " " + maxcount);
                                    runid = i;
                                    break;
                                }
                                else
								{
                                    if (rnd<= maxcount)
									{
                                        if (rnd > maxcount - valsInt[i])
										{
                                            //Debug.Log(rnd + " "+ (maxcount - valsInt[i]) + " " + maxcount);
                                            runid = i;
                                            break;
                                        }
									}
								}
                            }


                            RunCommand(ValueWorkCommand(coms[runid]));
                            break;
                        }
                    case "debug":
                        {
                            Debug.Log(ValueWorkString(comms[1]));
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "debugerror":
                        {
                            Error(ValueWorkString(comms[1]));
                            break;
                        }
                    case "addstat":
                        {
                            stats.AddValue(comms[1], ValueWorkFloat(comms[2]));
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "subtractstat":
                        {
                            stats.SubtractValue(comms[1], ValueWorkFloat(comms[2]));
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "setstatmin":
                        {
                            stats.SetValueMin(comms[1], ValueWorkFloat(comms[2]));
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "setstatmax":
                        {
                            stats.SetValueMax(comms[1], ValueWorkFloat(comms[2]));
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "setstatminmax":
                        {
                            stats.SetValueMinMax(comms[1], ValueWorkFloat(comms[2]), ValueWorkFloat(comms[3]));
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "setstat":
                        {
                            if (comms[3] == "string")
                            {
                                stats.SetValue(comms[1], ValueWorkString(comms[2]));
                            }
                            if (comms[3] == "int")
                            {
                                stats.SetValue(comms[1], ValueWorkInt(comms[2]));
                            }
                            if (comms[3] == "float")
                            {
                                stats.SetValue(comms[1], ValueWorkFloat(comms[2]));
                            }
                            if (comms[3] == "bool")
                            {
                                stats.SetValue(comms[1], ValueWorkBool(comms[2]));
                            }
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "setssa":
                        {
                            if (!SaveSystemAlt.IsWorking()) SaveSystemAlt.StartWork();

                            if (comms[3] == "string")
                            {
                                SaveSystemAlt.SetString(comms[1], stats.GetValue(comms[2], ""));
                            }
                            if (comms[3] == "int")
                            {
                                SaveSystemAlt.SetInt(comms[1], stats.GetValue(comms[2], 0));
                            }
                            if (comms[3] == "float")
                            {
                                SaveSystemAlt.SetFloat(comms[1], stats.GetValue(comms[2], 0f));
                            }
                            if (comms[3] == "bool")
                            {
                                SaveSystemAlt.SetBool(comms[1], stats.GetValue(comms[2], false));
                            }
                            SaveSystemAlt.SaveUpdatesNotClose();

                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "setssavalue":
                        {
                            if (!SaveSystemAlt.IsWorking()) SaveSystemAlt.StartWork();

                            if (comms[3] == "string")
                            {
                                SaveSystemAlt.SetString(comms[1], ValueWorkString(comms[2]));
                            }
                            if (comms[3] == "int")
                            {
                                SaveSystemAlt.SetInt(comms[1], ValueWorkInt(comms[2]));
                            }
                            if (comms[3] == "float")
                            {
                                SaveSystemAlt.SetFloat(comms[1], ValueWorkFloat(comms[2]));
                            }
                            if (comms[3] == "bool")
                            {
                                SaveSystemAlt.SetBool(comms[1], ValueWorkBool(comms[2]));
                            }
                            SaveSystemAlt.SaveUpdatesNotClose();

                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "isstat":
                        {
                            bool isok = false;

                            if(comms[4] == "any")
							{
                                SLM_Stats_Block b = stats.GetStat(comms[1]);
                                if (b.type == SLM_Stats.tpe.boolType)
                                    comms[4] = "bool";
                                else if (b.type == SLM_Stats.tpe.floatType)
                                    comms[4] = "float";
                                else if (b.type == SLM_Stats.tpe.intType)
                                    comms[4] = "int";
                                else if (b.type == SLM_Stats.tpe.stringType)
                                    comms[4] = "string";
                            }
                            if (comms[4] == "string" || comms[4] == "bool")
                            {
                                string val = "";
                                if (comms[4] == "string")
                                    val = stats.GetValue(comms[1], "");
                                else if (comms[4] == "bool")
                                {
                                    val = stats.GetValue(comms[1], false).ToString().ToLower();
                                    comms[3] = comms[3].ToLower();
                                }

                                if (comms[4] == "string")
                                {
                                    if (comms[2] == "==")
                                        if (val == ValueWorkString(comms[3]).ToString()) isok = true;
                                    if (comms[2] == "!=")
                                        if (!(val == ValueWorkString(comms[3]).ToString())) isok = true;
                                }
                                else if (comms[4] == "bool")
                                {

                                    if (comms[2] == "==")
                                        if (val == ValueWorkBool(comms[3]).ToString().ToLower()) isok = true;
                                    if (comms[2] == "!=")
                                        if (!(val == ValueWorkBool(comms[3]).ToString().ToLower())) isok = true;
                                }
                            }
							if (comms[4] == "int")
							{
								float val = 0f;
								if (comms[4] == "float")
									val = stats.GetValue(comms[1], 0f);
								else if (comms[4] == "int")
									val = stats.GetValue(comms[1], 0);

								if (comms[2] == "==")
									if (val == ValueWorkInt(comms[3])) isok = true;
								if (comms[2] == "!=")
									if (!(val == ValueWorkInt(comms[3]))) isok = true;
								if (comms[2] == ">")
									if (val > ValueWorkInt(comms[3])) isok = true;
								if (comms[2] == ">=")
									if (val >= ValueWorkInt(comms[3])) isok = true;
								if (comms[2] == "<")
									if (val < ValueWorkInt(comms[3])) isok = true;
								if (comms[2] == "<=")
									if (val <= ValueWorkInt(comms[3])) isok = true;
								if (comms[2] == "<>")
								{
									comms[3] = comms[3].Replace("||", "◌");
									string[] coms = comms[3].Split('◌');
									if (val > ValueWorkInt(coms[0]))
										if (val < ValueWorkInt(coms[1]))
											isok = true;
								}
								if (comms[2] == "<=>")
								{
									comms[3] = comms[3].Replace("||", "◌");
									string[] coms = comms[3].Split('◌');
									if (val >= ValueWorkInt(coms[0]))
										if (val <= ValueWorkInt(coms[1]))
											isok = true;
								}
							}

							if (comms[4] == "float")
                            {
                                float val = 0f;
                                if (comms[4] == "float")
                                    val = stats.GetValue(comms[1], 0f);
                                else if (comms[4] == "int")
                                    val = stats.GetValue(comms[1], 0);

                                if (comms[2] == "==")
                                    if (val == ValueWorkFloat(comms[3])) isok = true;
                                if (comms[2] == "!=")
                                    if (!(val == ValueWorkFloat(comms[3]))) isok = true;
                                if (comms[2] == ">")
                                    if (val > ValueWorkFloat(comms[3])) isok = true;
                                if (comms[2] == ">=")
                                    if (val >= ValueWorkFloat(comms[3])) isok = true;
                                if (comms[2] == "<")
                                    if (val < ValueWorkFloat(comms[3])) isok = true;
                                if (comms[2] == "<=")
                                    if (val <= ValueWorkFloat(comms[3])) isok = true;
                                if (comms[2] == "<>")
                                {
                                    comms[3] = comms[3].Replace("||", "◌");
                                    string[] coms = comms[3].Split('◌');
                                    if (val > ValueWorkFloat(coms[0]))
                                        if (val < ValueWorkFloat(coms[1]))
                                            isok = true;
                                }
                                if (comms[2] == "<=>")
                                {
                                    comms[3] = comms[3].Replace("||", "◌");
                                    string[] coms = comms[3].Split('◌');
                                    if (val >= ValueWorkFloat(coms[0]))
                                        if (val <= ValueWorkFloat(coms[1]))
                                            isok = true;
                                }
                            }

                            if (isok)
                                RunCommand(ValueWorkCommand(comms[5]));
                            else
                                RunCommand(ValueWorkCommand(comms[6]));
                            break;
                        }
                    case "isvalue":
                        {
                            bool isok = false;

                            if (comms[4] == "string" || comms[4] == "bool")
                            {
                                if (comms[4] == "string")
                                {
                                    if (comms[2] == "==")
                                        if (ValueWorkString(comms[1]).ToString().ToLower() == ValueWorkString(comms[3]).ToString().ToLower()) isok = true;
                                    if (comms[2] == "!=")
                                        if (!(ValueWorkString(comms[1]).ToString().ToLower() == ValueWorkString(comms[3]).ToString().ToLower())) isok = true;
                                }
                                if (comms[4] == "bool")
                                {
                                    if (comms[2] == "==")
                                        if (ValueWorkBool(comms[1]).ToString().ToLower() == ValueWorkBool(comms[3]).ToString().ToLower()) isok = true;
                                    if (comms[2] == "!=")
                                        if (!(ValueWorkBool(comms[1]).ToString().ToLower() == ValueWorkBool(comms[3]).ToString().ToLower())) isok = true;
                                }
                            }
                            if (comms[4] == "float")
                            {
                                if (comms[2] == "==")
                                    if (ValueWorkFloat(comms[1]) == ValueWorkFloat(comms[3])) isok = true;
                                if (comms[2] == "!=")
                                    if (!(ValueWorkFloat(comms[1]) == ValueWorkFloat(comms[3]))) isok = true;
                                if (comms[2] == ">")
                                    if (ValueWorkFloat(comms[1]) > ValueWorkFloat(comms[3])) isok = true;
                                if (comms[2] == ">=")
                                    if (ValueWorkFloat(comms[1]) >= ValueWorkFloat(comms[3])) isok = true;
                                if (comms[2] == "<")
                                    if (ValueWorkFloat(comms[1]) < ValueWorkFloat(comms[3])) isok = true;
                                if (comms[2] == "<=")
                                    if (ValueWorkFloat(comms[1]) <= ValueWorkFloat(comms[3])) isok = true;
                                if (comms[2] == "<>")
                                {
                                    comms[3] = comms[3].Replace("||", "◌");
                                    string[] coms = comms[3].Split('◌');
                                    if (ValueWorkFloat(comms[1]) > ValueWorkFloat(coms[0]))
                                        if (ValueWorkFloat(comms[1]) < ValueWorkFloat(coms[1]))
                                            isok = true;
                                }
                                if (comms[2] == "<=>")
                                {
                                    comms[3] = comms[3].Replace("||", "◌");
                                    string[] coms = comms[3].Split('◌');
                                    if (ValueWorkFloat(comms[1]) >= ValueWorkFloat(coms[0]))
                                        if (ValueWorkFloat(comms[1]) <= ValueWorkFloat(coms[1]))
                                            isok = true;
                                }
                            }
							if (comms[4] == "int")
							{
								if (comms[2] == "==")
									if (ValueWorkInt(comms[1]) == ValueWorkInt(comms[3])) isok = true;
								if (comms[2] == "!=")
									if (!(ValueWorkInt(comms[1]) == ValueWorkInt(comms[3]))) isok = true;
								if (comms[2] == ">")
									if (ValueWorkInt(comms[1]) > ValueWorkInt(comms[3])) isok = true;
								if (comms[2] == ">=")
									if (ValueWorkInt(comms[1]) >= ValueWorkInt(comms[3])) isok = true;
								if (comms[2] == "<")
									if (ValueWorkInt(comms[1]) < ValueWorkInt(comms[3])) isok = true;
								if (comms[2] == "<=")
									if (ValueWorkInt(comms[1]) <= ValueWorkInt(comms[3])) isok = true;
								if (comms[2] == "<>")
								{
									comms[3] = comms[3].Replace("||", "◌");
									string[] coms = comms[3].Split('◌');
									if (ValueWorkInt(comms[1]) > ValueWorkInt(coms[0]))
										if (ValueWorkInt(comms[1]) < ValueWorkInt(coms[1]))
											isok = true;
								}
								if (comms[2] == "<=>")
								{
									comms[3] = comms[3].Replace("||", "◌");
									string[] coms = comms[3].Split('◌');
									if (ValueWorkInt(comms[1]) >= ValueWorkInt(coms[0]))
										if (ValueWorkInt(comms[1]) <= ValueWorkInt(coms[1]))
											isok = true;
								}
							}

							if (isok)
                                RunCommand(ValueWorkCommand(comms[5]));
                            else
                                RunCommand(ValueWorkCommand(comms[6]));
                            break;
                        }
                    case "isstattostat":
                        {
                            SLM_Stats_Block block1 = stats.GetStat(comms[1]);
                            SLM_Stats_Block block2 = stats.GetStat(comms[3]);
                            bool isok = false;
                            if (!(block1 == null || block2 == null))
                            {
                                if ((block1.type == SLM_Stats.tpe.floatType || block1.type == SLM_Stats.tpe.intType) && (block2.type == SLM_Stats.tpe.floatType || block2.type == SLM_Stats.tpe.intType))
                                {
                                    float val1 = float.Parse(block1.value);
                                    float val2 = float.Parse(block2.value);

                                    if (comms[2] == "==")
                                        if (val1 == val2) isok = true;
                                    if (comms[2] == "!=")
                                        if (!(val1 == val2)) isok = true;
                                    if (comms[2] == ">")
                                        if (val1 > val2) isok = true;
                                    if (comms[2] == ">=")
                                        if (val1 >= val2) isok = true;
                                    if (comms[2] == "<")
                                        if (val1 < val2) isok = true;
                                    if (comms[2] == "<=")
                                        if (val1 <= val2) isok = true;
                                }
                                if (block1.type == SLM_Stats.tpe.stringType && block2.type == SLM_Stats.tpe.stringType)
                                {
                                    if (comms[2] == "==")
                                        if (block1.value == block2.value) isok = true;
                                    if (comms[2] == "!=")
                                        if (!(block1.value == block2.value)) isok = true;
                                }
                                if (block1.type == SLM_Stats.tpe.boolType && block2.type == SLM_Stats.tpe.boolType)
                                {
                                    if (comms[2] == "==")
                                        if (block1.value == block2.value) isok = true;
                                    if (comms[2] == "!=")
                                        if (!(block1.value == block2.value)) isok = true;
                                }
                            }

                            if (isok)
                                RunCommand(ValueWorkCommand(comms[4]));
                            else
                                RunCommand(ValueWorkCommand(comms[5]));
                            break;
                        }
                    case "pause":
                        {
                            break;
                        }
                    case "wait":
                        {
                            if (blocks[currentid].autorunnextcommand)
                                timer = ValueWorkFloat(comms[1]);
                            break;
                        }
                    case "showimage":
                        {
                            
                            try
                            {
                                imagesc.SetImage(comms[1], ValueWorkInt(comms[2]));
                            }
                            catch
                            {
                                imagesc.SetImage(comms[1], ValueWorkString(comms[2]));
                            }
                            curimglayer = comms[1];
                            curimgnum = imagesc.lastimage;
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "imagecolor":
                        {
                            imagesc.SetColor(comms[1], ValueWorkString(comms[2]));

                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "imageanim":
                        {
                            float s = 0;
                            if (comms.Length > 3)
                                s = ValueWorkFloat(comms[3]);
                            imagesc.SetAnim(comms[1], ValueWorkInt(comms[2]), s);

                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "disablelayer":
                        {
                            imagesc.DisableLayer(comms[1]);
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "showlayer":
                        {
                            try
                            {
                                int idd = -1;
                                if (comms.Length > 2)
                                    if (!string.IsNullOrEmpty(comms[2]))
                                        idd = ValueWorkInt(comms[2]);
                                imagesc.ShowLayer(comms[1], idd);
                            }
                            catch
                            {
                                string idd = "";
                                if (comms.Length > 2)
                                    if (!string.IsNullOrEmpty(comms[2]))
                                        idd = ValueWorkString(comms[2]);
                                imagesc.ShowLayer(comms[1], idd);
                            }
                            curimglayer = comms[1];
                            curimgnum = imagesc.lastimage;

                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "deselectchars":
                        {
                            charAI.DeselectChars();
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "selectchar":
                        {
                            charAI.SelectChar(ValueWorkString(comms[1]));
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "setcharemotion":
                        {
                            if (comms[1] == "nml")
                                charAI.SetEmotionId(0);
                            else if (comms[1] == "agr")
                                charAI.SetEmotionId(1);
                            else if (comms[1] == "emb")
                                charAI.SetEmotionId(2);
                            else if (comms[1] == "sad")
                                charAI.SetEmotionId(3);
                            else if (comms[1] == "hap")
                                charAI.SetEmotionId(4);
                            else if (comms[1].Contains("cus"))
                            {
                                comms[1] = comms[1].Replace("||", "◌");
                                string[] coms = comms[1].Split('◌');
                                charAI.SetCustomEmotion(int.Parse(coms[1]));
                            }

                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "setcharemotionbyid":
                        {
                            charAI.SetEmotionId(int.Parse(comms[1]));
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "audio":
                        {
                            string sub = comms[1];

                            if (sub == "clip")

                                try
                                {
                                    audiosc.SelectClip(ValueWorkString(comms[2]), ValueWorkInt(comms[3]));
                                }
                                catch
                                {
									audiosc.SelectClip(ValueWorkString(comms[2]), ValueWorkString(comms[3]));
								}
                            else if (sub == "play")
                                audiosc.StartPlay(ValueWorkString(comms[2]));
                            else if (sub == "setplay")
                            {
								try
								{
									audiosc.SelectClip(ValueWorkString(comms[2]), ValueWorkInt(comms[3]));
								}
								catch
								{
									audiosc.SelectClip(ValueWorkString(comms[2]), ValueWorkString(comms[3]));
								}
								audiosc.StartPlay(ValueWorkString(comms[2]));
                            }
                            else if (sub == "stop")
                                audiosc.StopPlay(ValueWorkString(comms[2]));
                            else if (sub == "volume")
                                audiosc.SetVolume(ValueWorkString(comms[2]), ValueWorkFloat(comms[3]));
                            else if (sub == "mute")
                                audiosc.SetMute(ValueWorkString(comms[2]), ValueWorkBool(comms[3]));
                            else if (sub == "loop")
                                audiosc.SetLoop(ValueWorkString(comms[2]), ValueWorkBool(comms[3]));
                            else if (sub == "pan")
                                audiosc.SetStereoPan(ValueWorkString(comms[2]), ValueWorkFloat(comms[3]));
                            else if (sub == "pitch")
                                audiosc.SetPitch(ValueWorkString(comms[2]), ValueWorkFloat(comms[3]));
                            else if (sub == "priority")
                                audiosc.SetPriority(ValueWorkString(comms[2]), ValueWorkInt(comms[3]));
                            else if (sub == "mixer")
                            {
                                float time = 0;
                                if (comms.Length > 4)
                                    time = ValueWorkFloat(comms[4]);
                                audiosc.SetSnapshot(ValueWorkString(comms[2]), ValueWorkInt(comms[3]), time);
                            }
                            //else if (sub == "stopmixer")
                            //  audiosc.DisableMixer(ValueWorkString(comms[2]));


                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "qte":
                        {
                            try
                            {
                                qte.RunQTE(ValueWorkInt(comms[1]), this, ValueWorkCommand(comms[2]), ValueWorkCommand(comms[3]));
                            }
                            catch
                            {
                                qte.RunQTE(ValueWorkString(comms[1]), this, ValueWorkCommand(comms[2]), ValueWorkCommand(comms[3]));
                            }
                            break;
                        }
                    case "setpollpreset":
                        {
                            textsc.SetPreset(false, int.Parse(comms[1]));
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "settextpreset":
                        {
                            textsc.SetPreset(true, int.Parse(comms[1]));
                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    case "switch":
                        {
                            int count = int.Parse(comms[1]);

                            string statname = comms[2];

                            comms[4] = comms[4].Replace("||", "◌");
                            string[] vals = comms[4].Split('◌');
                            comms[5] = comms[5].Replace("||", "◌");
                            string[] commands = comms[5].Split('◌');

                            if (count == -1)
                                count = vals.Length;

                            if (vals.Length != commands.Length)
                            {
                                Debug.LogError("The number of variables does not correspond to the number of commands! (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[id] + "\" )");
                                onError?.Invoke("The number of variables does not correspond to the number of commands! (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[id] + "\" )");
                                if (blocks[currentid].autorunnextcommand && !stopIfError)
                                    RunNextCommand();
                                return;
                            }

                            int runthis = -1;

                            if (comms[3]=="any")
							{
                                SLM_Stats_Block b = stats.GetStat(statname);
                                if (b.type == SLM_Stats.tpe.boolType)
                                    comms[3] = "bool";
                                else if (b.type == SLM_Stats.tpe.floatType)
                                    comms[3] = "float";
                                else if (b.type == SLM_Stats.tpe.intType)
                                    comms[3] = "int";
                                else if (b.type == SLM_Stats.tpe.stringType)
                                    comms[3] = "string";
                            }

                            for (int i = 0; i < count; i++)
                            {
                                if (comms[3] == "string")
                                {
                                    string val = stats.GetValue(statname, "");

                                    if (ValueWorkString(vals[i]) == val)
                                    {
                                        runthis = i;
                                        break;
                                    }
                                }
                                if (comms[3] == "int")
                                {
                                    int val = stats.GetValue(statname, 0);
                                    if (ValueWorkInt(vals[i]) == val)
                                    {
                                        runthis = i;
                                        break;
                                    }
                                }
                                if (comms[3] == "float")
                                {
                                    float val = stats.GetValue(statname, 0);
                                    if (ValueWorkFloat(vals[i]) == val)
                                    {
                                        runthis = i;
                                        break;
                                    }
                                }
                                if (comms[3] == "bool")
                                {
                                    bool val = stats.GetValue(statname, false);
                                    if (ValueWorkBool(vals[i]) == val)
                                    {
                                        runthis = i;
                                        break;
                                    }
                                }
                            }
                            if (runthis == -1)
                            {
                                if (comms.Length > 6)
                                    RunCommand(ValueWorkCommand(comms[6]));
                                else
                                    RunCommand(ValueWorkCommand(commands[0]));
                            }
                            else
                                RunCommand(ValueWorkCommand(commands[runthis]));
                            break;

                        }
                    case "cc":
                        {
                            if (blocks[currentid].customCommands.Find(f=> f.name == comms[1]) !=null)
                            {
                                int idcc = blocks[currentid].customCommands.IndexOf(blocks[currentid].customCommands.Find(f => f.name == comms[1]));

                                if (blocks[currentid].customCommands[idcc].type == SLM_Commands_CustomCommand.tpe.both)
                                {
                                    blocks[currentid].events[blocks[currentid].customCommands[idcc].eventid].Invoke();
                                    blocks[currentid].customCommands[idcc].customEvent.Invoke();
                                }
                                if (blocks[currentid].customCommands[idcc].type == SLM_Commands_CustomCommand.tpe.useEvent)
                                {
                                    blocks[currentid].customCommands[idcc].customEvent.Invoke();
                                }
                                if (blocks[currentid].customCommands[idcc].type == SLM_Commands_CustomCommand.tpe.useId)
                                {
                                    blocks[currentid].events[blocks[currentid].customCommands[idcc].eventid].Invoke();
                                }

                                if (blocks[currentid].autorunnextcommand && blocks[currentid].customCommands[idcc].runNextCommand)
                                    RunNextCommand();
                            }
                            else
                            {
                                throw new Exception("Custom command " + comms[1] + " not found!");
                            }
                            break;
                        }
                    case "ccc":
                        {
                            if (comms.Length > 1)
                                CustomCC(comms[1]);
                            else
                                CustomCC(null);

                            if (blocks[currentid].autorunnextcommand)
                                RunNextCommand();
                            break;
                        }
                    default:
                        {
                            if (addonManager != null)
                            {
                                List<string> args = comms.ToList();
                                args.RemoveAt(0);
                                if (addonManager.RunAddon(comms[0], args.ToArray()))
                                {
                                    if (blocks[currentid].autorunnextcommand && addonManager.runnextcommandInfo)
                                        RunNextCommand();
                                }
                                else
                                {
                                    throw new Exception("Command " + comms[0] + " not found or had syntax errors!");
                                }
                            }
                            else
                            {
                                throw new Exception("Command " + comms[0] + " not found!");
                            }
                            break;
                        }

                }
            }
            catch (System.Exception err)
            {
                Debug.LogError("Command line error! (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[id] + "\") Error:" + err);
                onError?.Invoke("Command found, but it has problem with read, check the spelling! (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[id] + "\") Error:" + err);
                if (blocks[currentid].autorunnextcommand && !stopIfError)
                    RunNextCommand();
            }
        }
        else
        {
            Debug.LogError("A single block is not running, call the RunBlock(id)");
            onError?.Invoke("A single block is not running, call the RunBlock(id)");
        }

    }

    public void Error(string text)
	{
        Debug.LogError("Error! (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[currentcommand] + "\" ) Error: " + text);
        onError?.Invoke("Error! (block: " + currentid + "; command: " + currentcommand + " string: \"" + blocks[currentid].commands[currentcommand] + "\" ) Error: " + text);
    }

   public int ValueWorkCommand(string input)
   {
      int output = 0;
      if (input.IndexOf("vw") >= 0)
      {
         input = input.Replace("//", "◌");
         string[] parts = input.Split('◌');
         if (parts[1] == "random")
            output = UnityEngine.Random.Range(int.Parse(parts[2]), int.Parse(parts[3] + 1));
         else if (parts[1] == "randomvalues")
         {
            int r = UnityEngine.Random.Range(0, int.Parse(parts[2]));
            output = int.Parse(parts[r + 3]);
         }
         else if (parts[1] == "getstat")
            output = stats.GetValue(parts[2], 0);
         else if (parts[1] == "getssa")
         {
            int def = 0;
            if (parts.Length > 3)
               def = int.Parse(parts[3]);
            output = SaveSystemAlt.GetInt(parts[2], def);
         }
         else if (parts[1] == "getpoint")
         {
            int offset = 0;
            if (parts.Length == 4)
               offset = int.Parse(parts[3]);
            if (pointContain.Contains(parts[2]))
               output = pointid[pointContain.IndexOf(parts[2])] + offset;
            else
            {
               Debug.LogError("Error! (block: " + currentid + "; command: " + currentcommand + ") Point " + parts[2] + " not found!");
               onError?.Invoke("Error! (block: " + currentid + "; command: " + currentcommand + ") Point " + parts[2] + " not found!");
               output = -1;
            }
         }
         else if (parts[1] == "randompoints")
         {
            int r = UnityEngine.Random.Range(0, int.Parse(parts[2]));

            string p = parts[r + 3];

            if (pointContain.Contains(p))
               output = pointid[pointContain.IndexOf(p)];
            else
            {
               Debug.LogError("Point not found!");
               onError?.Invoke("Point not found!");
               output = -1;
            }
         }
         else if (parts[1] == "next")
         {
            int offset = 0;
            //Debug.Log(input);
            if (parts.Length >= 3)
               offset = int.Parse(parts[2]);

            output = currentcommand + 1 + offset;

         }
         else if (parts[1] == "mathv2")
         {
            List<string> vars = new List<string>();
            List<float> varsV = new List<float>();

            for (int i = 3; i < parts.Length; i++)
            {
               vars.Add(parts[i]);
               varsV.Add(stats.GetValue(parts[i], 0f));
            }

            string math = parts[2];

            for (int i = 0; i < vars.Count; i++)
            {
               math = math.Replace(vars[i], varsV[i].ToString());
            }

            math = math.Replace(',', '.');
            output = (int)float.Parse(new DataTable().Compute(math, "").ToString());
         }
      }
      else
      {
         output = int.Parse(input);
      }

      return output;
   }

   public int ValueWorkInt(string input)
   {
      int output = 0;

      if (input.IndexOf("vw") >= 0)
      {
         input = input.Replace("//", "◌");
         string[] parts = input.Split('◌');
         if (parts[1] == "random")
         {
            output = UnityEngine.Random.Range(int.Parse(parts[2]), int.Parse(parts[3]));
         }
         else if (parts[1] == "randomvalues")
         {
            int r = UnityEngine.Random.Range(0, int.Parse(parts[2]));
            output = int.Parse(parts[r + 3]);
         }
         else if (parts[1] == "randombystats")
         {
            //vw//randombystats//statMin//statMax;;
            int min = stats.GetValue(parts[2], 0);
            int max = stats.GetValue(parts[3], 0);
            output = UnityEngine.Random.Range(min, max);
         }
         else if (parts[1] == "getstat")
            output = stats.GetValue(parts[2], 0);
         else if (parts[1] == "getstatwithinvalues")
         {
            output = stats.GetValue(parts[2], 0);
            if (output < int.Parse(parts[3])) output = int.Parse(parts[3]);
            if (output > int.Parse(parts[4])) output = int.Parse(parts[4]);
         }
         else if (parts[1] == "getstatmath")
         {
            string math = parts[3].Replace(parts[2], stats.GetValue(parts[2], 0f).ToString());
            output = (int)float.Parse(new DataTable().Compute(math, "").ToString());
         }
         else if (parts[1] == "getssa")
         {
            int def = 0;
            if (parts.Length > 3)
               def = int.Parse(parts[3]);
            output = SaveSystemAlt.GetInt(parts[2], def);
         }
         else if (parts[1] == "getssawithinvalues")
         {
            output = SaveSystemAlt.GetInt(parts[2], 0);
            if (output < int.Parse(parts[3])) output = int.Parse(parts[3]);
            if (output > int.Parse(parts[4])) output = int.Parse(parts[4]);
         }
         else if (parts[1] == "getssamath")
         {
            string math = parts[3].Replace(parts[2], SaveSystemAlt.GetInt(parts[2], 0).ToString());
            //Debug.Log(math);
            output = (int)float.Parse(new DataTable().Compute(math, "").ToString());
            //Debug.Log(output);
         }
         else if (parts[1] == "getpoint")
         {
            int offset = 0;
            if (parts.Length == 4)
               offset = int.Parse(parts[3]);
            if (pointContain.Contains(parts[2]))
               output = pointid[pointContain.IndexOf(parts[2])] + offset;
            else
            {
               Debug.LogError("Point not found!");
               onError?.Invoke("Point not found!");
               output = -1;
            }
         }
         else if (parts[1] == "gettextpoint")
         {
            int offset = 0;
            if (parts.Length == 4)
               offset = int.Parse(parts[3]);
            if (pointContaintext.Contains(parts[2]))
               output = pointidtext[pointContaintext.IndexOf(parts[2])] + offset;
            else
            {
               Debug.LogError("Text point not found!");
               onError?.Invoke("Text point not found!");
               output = -1;
            }
         }
         else if (parts[1] == "next")
         {
            int offset = 0;
            //Debug.Log(input);
            if (parts.Length >= 3)
               offset = int.Parse(parts[2]);

            output = currentcommand + 1 + offset;

         }
         else if (parts[1] == "mathv2")
         {
            List<string> vars = new List<string>();
            List<float> varsV = new List<float>();

            for (int i = 3; i < parts.Length; i++)
            {
               vars.Add(parts[i]);
               varsV.Add(stats.GetValue(parts[i], 0f));
            }

            string math = parts[2];

            for (int i = 0; i < vars.Count; i++)
            {
               math = math.Replace(vars[i], varsV[i].ToString());
            }

            math = math.Replace(',', '.');
            output = (int) float.Parse(new DataTable().Compute(math, "").ToString());
         }
      }
      else
      {
         output = int.Parse(input);
      }

      return output;
   }

    public bool ValueWorkBool(string input)
    {
        bool output = false;

        if (input.IndexOf("vw") >= 0)
        {
            input = input.Replace("//", "◌");
            string[] parts = input.Split('◌');
            if (parts[1] == "getstat")
                output = stats.GetValue(parts[2], false);
            else if (parts[1] == "getssa")
            {
                bool def = false;
                if (parts.Length > 3)
                    def = bool.Parse(parts[3]);
                output = SaveSystemAlt.GetBool(parts[2], def);
            }
            else if (parts[1] == "randomvalues")
            {
                int r = UnityEngine.Random.Range(0, int.Parse(parts[2]));
                output = bool.Parse(parts[r + 3]);
            }
            else if (parts[1] == "getstatinvert")
                output = !stats.GetValue(parts[2], false);
            else if (parts[1] == "getssainvert")
                output = !SaveSystemAlt.GetBool(parts[2], bool.Parse(parts[3]));
            else if (parts[1] == "checkstatint")
            {
                string mode = "==";
                if (parts.Length>4)
                {
                    mode = parts[4];
				}

				if (mode == "==")
                    output = (stats.GetValue(parts[2], (int)0) == int.Parse(parts[3]));
                else if (mode == ">=")
					output = (stats.GetValue(parts[2], (int)0) >= int.Parse(parts[3]));
				else if (mode == ">")
					output = (stats.GetValue(parts[2], (int)0) > int.Parse(parts[3]));
				else if (mode == "<=")
					output = (stats.GetValue(parts[2], (int)0) <= int.Parse(parts[3]));
				else if (mode == "<")
					output = (stats.GetValue(parts[2], (int)0) < int.Parse(parts[3]));
				else if (mode == "!=")
					output = (stats.GetValue(parts[2], (int)0) != int.Parse(parts[3]));
			}
            else if (parts[1] == "checkstatfloat")
            {
				string mode = "==";
				if (parts.Length > 4)
				{
					mode = parts[4];
				}
                if (mode == "==")
                    output = (stats.GetValue(parts[2], (int)0) == float.Parse(parts[3]));
                else if (mode == ">=")
                    output = (stats.GetValue(parts[2], (int)0) >= float.Parse(parts[3]));
                else if (mode == ">")
                    output = (stats.GetValue(parts[2], (int)0) > float.Parse(parts[3]));
                else if (mode == "<=")
                    output = (stats.GetValue(parts[2], (int)0) <= float.Parse(parts[3]));
                else if (mode == "<")
                    output = (stats.GetValue(parts[2], (int)0) < float.Parse(parts[3]));
                else if (mode == "!=")
                    output = (stats.GetValue(parts[2], (int)0) != float.Parse(parts[3]));

			}
        }
        else
        {
            try {
                output = bool.Parse(input);
            }
            catch { };
        }

        return output;
    }

    public string ValueWorkString(string input)
    {
        string output = "";

        if (input.IndexOf("vw") >= 0)
        {
            input = input.Replace("//", "◌");
            string[] parts = input.Split('◌');
            if (parts[1] == "getstat")
                output = stats.GetValue(parts[2], "");
            else if (parts[1] == "getssa")
            {
                string def = "";
                if (parts.Length > 3)
                    def = parts[3];
                output = SaveSystemAlt.GetString(parts[2], def);
            }
            else if (parts[1] == "getalsl")
            {
                if (ALSLBridge == null)
                    output = ALSL_Main.GetWorldAndFindKeys(parts[2]);
                else
                    output = ALSLBridge.GetString(parts[2]);
            }
            else if (parts[1] == "getralsl")
                output = ALSL_Main.GetReplaceFromKey(parts[2]);
            else if (parts[1] == "randomvalues")
            {
                int r = UnityEngine.Random.Range(0, int.Parse(parts[2]));
                output = parts[r + 3];
            }
            else if (parts[1] == "gettextpoint")
            {
                int offset = 0;
                if (parts.Length == 4)
                    offset = int.Parse(parts[3]);
                if (pointContaintext.Contains(parts[2]))
                    output = blocks[currentid].texts[pointidtext[pointContaintext.IndexOf(parts[2])] + offset];
                else
                {
                    Debug.LogError("Text point not found!");
                    onError?.Invoke("Text point not found!");
                    output = "Text point not found!";
                }
            }
			else if (parts[1] == "gettext")
			{
            try
            {
               int offset = 0;
               if (parts.Length == 4)
                  offset = int.Parse(parts[3]);
               int textnum = int.Parse(parts[2]);
               output = blocks[currentid].texts[textnum + offset];
            }
            catch 
				{
					Debug.LogError("Can't convert into int!");
					onError?.Invoke("Can't convert into int!");
					output = "Can't convert into int!";
				}
			}
			else if (parts[1] == "gettextfromstat")
			{
            int textnum = stats.GetValue(parts[2], 0);
					output = blocks[currentid].texts[textnum];
			}
		}
        else
        {
            try
            {
                output = input;
            }
            catch { };
        }

            return output;
    }

    public float ValueWorkFloat(string input)
    {
        float output = 0;

        if (input.IndexOf("vw") == 0)
        {
            input = input.Replace("//", "◌");
            string[] parts = input.Split('◌');
            if (parts[1] == "random")
                output = UnityEngine.Random.Range(float.Parse(parts[2]), float.Parse(parts[3]));
            else if (parts[1] == "randomint")
                output = UnityEngine.Random.Range(int.Parse(parts[2]), int.Parse(parts[3] + 1));
            else if (parts[1] == "randomvalues")
            {
                int r = UnityEngine.Random.Range(0, int.Parse(parts[2]));
                output = int.Parse(parts[r + 3]);
            }
			else if (parts[1] == "randombystats")
			{
				//vw//randombystats//statMin//statMax;;
				float min = stats.GetValue(parts[2], 0);
				float max = stats.GetValue(parts[3], 0);
				output = UnityEngine.Random.Range(min, max);
			}
			else if (parts[1] == "getstat")
                output = stats.GetValue(parts[2], 0f);
            else if (parts[1] == "getstatwithinvalues")
            {
                output = stats.GetValue(parts[2], 0f);
                if (output < float.Parse(parts[3])) output = float.Parse(parts[3]);
                if (output > float.Parse(parts[4])) output = float.Parse(parts[4]);
            }
            else if (parts[1] == "getstatround")
                output = (float)Math.Round(stats.GetValue(parts[2], 0f), int.Parse(parts[3]));
            else if (parts[1] == "getstatmath")
            {
                string math = parts[3].Replace(parts[2], stats.GetValue(parts[2], 0f).ToString());
                output = float.Parse(new DataTable().Compute(math, "").ToString());
            }
            else if (parts[1] == "getssa")
            {
                float def = 0f;
                if (parts.Length > 3)
                    def = float.Parse(parts[3]);
                output = SaveSystemAlt.GetFloat(parts[2], def);
            }
            else if (parts[1] == "getssawithinvalues")
            {
                output = SaveSystemAlt.GetFloat(parts[2], 0f);
                if (output < float.Parse(parts[3])) output = float.Parse(parts[3]);
                if (output > float.Parse(parts[3])) output = float.Parse(parts[3]);
            }
            else if (parts[1] == "getssaround")
                output = (float)Math.Round(SaveSystemAlt.GetFloat(parts[2], 0f), int.Parse(parts[3]));
            else if (parts[1] == "getssamath")
            {
                string math = parts[3].Replace(parts[2], SaveSystemAlt.GetFloat(parts[2], 0f).ToString());
                output = float.Parse(new DataTable().Compute(math, "").ToString());
            }
			else if (parts[1] == "mathv2")
			{
            List<string> vars = new List<string>();
				List<float> varsV = new List<float>();

            for (int i =3;i<parts.Length;i++)
            {
               vars.Add(parts[i]);
               varsV.Add(stats.GetValue(parts[i], 0f));
            }

				string math = parts[2];

            for (int i=0;i<vars.Count;i++)
            {
					math = math.Replace(vars[i], varsV[i].ToString());
            }

            math = math.Replace(',', '.');
				output = float.Parse(new DataTable().Compute(math, "").ToString());
			}
		}
        else
        {
            output = float.Parse(input);
        }

            return output;
    }

    public void Wait(float time)
	{
        timer = time;
    }
}

[System.Serializable]
public class SLM_Commands_Block
{
    [HideInInspector]
    public bool autorunnextcommand=true;
    public bool pauseWhenEndFile;
    public bool usePoints;
    public bool useTextPoints;
    public SLM_Commands_BlockLoad files;
    public List<SLM_Commands_CustomCommand> customCommands;
    public List<string> commands;
    /*
     * format command::attribute:::attribute2 etc
     * commands:
     * showtext::id::offtext=false + +
     * runevent::id + +
     * runblock::id + +
     * runcommand::id + +
     * exit - stop command block + +
     * debug::text + +
     * restart - restart this block + +
     * addstat::statname::value + +
     * subtractstat::statname::value + + 
     * setstat::statname::value::type (int\float\bool\string) + +
     * isstat::statname::>/</==/<=/>=/!=::value::type::command1(true)::command2(False) + +
     * isvalue::value1::>/</==/<=/>=/!=::value2::type::command1(true)::command2(False) +
     * isstattostat::statname::>/</==/<=/>=/!=::statname2::command1(true)::command2(False) + +
     * runpoll::poll text::text1||text2::command1||command2::timer(-1 is unlimited)::defaultChoice(-1 - random, -2 - default) + +
     * setpollpreset::id + +
     * settextpreset::id + +
     * pause - stop running command + +
     * skip - skip current command and go next + +
     * wait::secs + +
     * showimage::layer::id + +
     * disablelayer::layer + +
     * showlayer::layer + +
     * cc::customCommand + +
     * selectchar::name + +
     * setcharemotion::nml/agr/emb/sad/hap/cus||id
     * setcharemotionbyid::id
     * deselectchars + +
     * qte::id::win::lose + +
     * random::id1||id2 etc + +
     * audio::clip::name::id + 
     * audio::play::name + +
     * audio::stop::name + +
     * audio::mute::name::state + +
     * audio::loop::name::state + +
     * audio::pan::name:state + +
     * audio::priority::name::state + +
     * audio::volume::name::state + +
     * audio::pitch::name::state + +
     * setssa::ssakey(string)::statname(string)::type + +
     * setpoint::name;; comment + +
     * runpoint::name + +
     * switch::count(int)::var::values::type::output::dafault;; + +
     * runpointasfunction::name + + 
     * endpoint + +
     * repeat::count + +
     * imagecolor::name::color
     * imageanim::name::type::sec
     * endrepeat
     */
    [MultilineAttribute] public List<string> texts;
    public List<UnityEvent> events;
}

[System.Serializable]
public class SLM_Commands_BlockLoad
{
    public bool useLoadFromFiles = false;
    public bool useTextObject;
    [ShowFromBool("useTextObject", false)]
    public bool useStreamingAssets = false;
    [ShowFromBool("useTextObject", false)]
    public bool useSLMContainer = false;

    [ShowFromBool("useTextObject")]
    public UnityEngine.Object commandObject;
    [ShowFromBool("useTextObject")]
    public UnityEngine.Object textObject;
    [ShowFromBool("useTextObject",false)]
    public string pathToCommands;
    [ShowFromBool("useTextObject", false)]
    public string pathToTexts;
    [ShowFromBool("useTextObject", false)]
    public bool useALSL;
    [ShowFromBool("useTextObject", false)]
    public string prefixALSLFile;
    [ShowFromBool("useTextObject", false)]
    public string ALSLFolder;
    /*
     * <str> - streaming assets
     * <docs> - docs folder
     * <dp> - DataPatch
     */
    [ShowFromBool("useSLMContainer")]
    public SLM_Container container;
}

[System.Serializable]
public class SLM_Commands_CustomCommand
{
    public string name;
    public bool runNextCommand = true;
    public enum tpe { useId, useEvent, both };
    public tpe type;
    [ShowFromMultiple(new string[2] { "type", "type" }, new string[2] { "0", "2" }, new string[2] { "enum", "enum" }, ShowFromMultipleAttribute.mode.or)]
    public int eventid = -1;
    public UnityEvent customEvent;
}