using AutoLangSLywnow;
using SLywnow;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SLM_ALSLBridge : MonoBehaviour
{
    public SLM_Commands commands;
    [Tooltip("Force Activate ALSL, need if you want test scene without loading (Works on in Editor)")]
    public bool ALSLDebugMode=true;
    public enum fileMode {_old, _new };
    public fileMode jsonMode;
    public SLM_ALSLBridge_SetUp setup;

    public SLM_ALSLBridge_LoaderStrings setupStrings;
    public SLM_ALSLBridge_LoaderStringsV2 setupStringsV2;

    [HideInInspector] 
    public List<string> loadedBlock;
    [HideInInspector] 
    public SLM_ALSLBridge_LoaderStringsV2 loadedStrings;
    [HideInInspector] public List<SLM_BridgeText> texts;

    [HideInInspector] public SLM_ALSLBridge_LoaderStrings forceLoadedStrings = null;
    [HideInInspector] public List<string> forceLoadedText = null;
    [HideInInspector] public bool useOnlyForce = false;

    public void Start()
	{
        if (ALSLDebugMode)
        {
            StartingSLAL.Restart();
        }
        if (setup.useCustomSetUp)
        {
            
        }

        LoadStrings();
	}

   [Button("Add all exist langs", ButtonMode.DisabledInPlayMode, ButtonSpacing.Before)]
    public void AddAllLangs()
	{
        List<string> l = new List<string>();
        for (int i = 0; i < setup.customSets.Count; i++)
            l.Add(setup.customSets[i].langName);

        ALSL_ToSaveJSON keys = new ALSL_ToSaveJSON();
        keys = JsonUtility.FromJson<ALSL_ToSaveJSON>(FilesSet.LoadStream(Application.streamingAssetsPath + "/ALSL", "keys", "alsldata", false, false));
        List<string> langs = keys.alllangs;

        for (int i = 0; i < langs.Count; i++)
        {
            if (!l.Contains(langs[i]))
            {
                setup.customSets.Add(new SLM_ALSLBridge_LanfSetUp());
                setup.customSets[setup.customSets.Count - 1].langName = langs[i];
                setup.customSets[setup.customSets.Count - 1].textFiles = new List<Object>(commands.blocks.Count);
                for (int a = 0; a < commands.blocks.Count; a++)
                    setup.customSets[setup.customSets.Count - 1].textFiles.Add(null); 
            }
            else
			{
                int id = l.IndexOf(langs[i]);
                int c = setup.customSets[id].textFiles.Count;
                if (commands.blocks.Count> setup.customSets[id].textFiles.Count)
                    for (int a = c; a < commands.blocks.Count; a++)
                        setup.customSets[id].textFiles.Add(null);

            }
        }
    }

    [Button("Create new files", ButtonMode.DisabledInPlayMode, ButtonSpacing.Before)]
    public void CreateFiles()
    {
        string sv = "";
        if (jsonMode==fileMode._old)
            sv = JsonUtility.ToJson(setupStrings, true);
        else 
            sv = JsonUtility.ToJson(setupStringsV2, true);

		List<List<string>> svblocks = new List<List<string>>();
        if (commands != null)
        {
            for (int i = 0; i < commands.blocks.Count; i++)
            {
                svblocks.Add(commands.blocks[i].texts);
                for (int a = 0; a < svblocks[i].Count; a++)
                    svblocks[i][a] = svblocks[i][a].Replace("\n", setup.newLineSymbol);
            }

        }

		List<string> langs = new List<string>();
#if UNITY_EDITOR
		List<ALSL_Perfab_Lang> lngs = (Resources.Load("alsl_perfab", typeof(GameObject)) as GameObject).GetComponent<ALSL_Perfab>().langFiles;

		foreach (ALSL_Perfab_Lang l in lngs)
			langs.Add(l.name);
#else
     langs = ALSL_Main.alllangs;
#endif

		string form = setup.format;
        if (!setup.useCustomSetUp)
            form = "txt";

            string pth = "";
        if (setup.useALSLFolders)
        {
            pth = Application.streamingAssetsPath + "/ALSL";
            if (!string.IsNullOrEmpty(setup.SceneProject))
                pth += "/" + setup.SceneProject;
            if (pth[pth.Length - 1] == '/' || pth[pth.Length - 1] == '\\')
                pth = pth.Remove(pth.Length - 1);
        }
        else
        {
            pth = setup.GlobalFolder;
            pth = pth.Replace("<docs>", FastFind.GetDefaultPath());
            pth = pth.Replace("<dp>", Application.dataPath);
            if (pth[pth.Length - 1] == '/' || pth[pth.Length - 1] == '\\')
                pth = pth.Remove(pth.Length - 1);

            if (!string.IsNullOrEmpty(setup.SceneProject))
                pth += "/" + setup.SceneProject;
            if (pth[pth.Length - 1] == '/' || pth[pth.Length - 1] == '\\')
                pth = pth.Remove(pth.Length - 1);
        }

        for (int l = 0; l < langs.Count; l++)
        {
            if (!FilesSet.CheckFile(pth, setup.stringsPrefix + langs[l], form))
                FilesSet.SaveStream(pth, setup.stringsPrefix + langs[l], form, sv, false, false);
            for (int i = 0; i < svblocks.Count; i++)
                if (!FilesSet.CheckFile(pth, setup.commandsTextPrefix + langs[l] + "_" + i, form))
                    FilesSet.SaveStream(pth, setup.commandsTextPrefix + langs[l] + "_" + i, form, svblocks[i].ToArray(), false, false);
        }

    }
    [Button("Update all files", ButtonMode.DisabledInPlayMode, ButtonSpacing.Before)]
    public void ForceRecreateFiles()
	{
		string sv = "";
        if (jsonMode == fileMode._old)
        {
            sv = JsonUtility.ToJson(setupStrings, true);
        }
        else
        {
            sv = JsonUtility.ToJson(setupStringsV2, true);
        }

		List<List<string>> svblocks = new List<List<string>>();
        if (commands != null)
        {
            for (int i = 0; i < commands.blocks.Count; i++) 
            {
                svblocks.Add(new List<string>());
                svblocks[i].AddRange(commands.blocks[i].texts);
                for (int a = 0; a < svblocks[i].Count; a++)
                    svblocks[i][a] = svblocks[i][a].Replace("\n", setup.newLineSymbol);
            }
                
        }

        List<string> langs = new List<string>();
#if UNITY_EDITOR
		List<ALSL_Perfab_Lang> lngs = (Resources.Load("alsl_perfab", typeof(GameObject)) as GameObject).GetComponent<ALSL_Perfab>().langFiles;

      foreach (ALSL_Perfab_Lang l in lngs)
         langs.Add(l.name);
#else
     langs = ALSL_Main.alllangs;
#endif

		if (!setup.useCustomSetUp)
        {
            string pth = "";
            if (setup.useALSLFolders)
            {
                pth = Application.streamingAssetsPath + "/ALSL";
                if (!string.IsNullOrEmpty(setup.SceneProject))
                    pth += "/" + setup.SceneProject;
                if (pth[pth.Length - 1] == '/' || pth[pth.Length - 1] == '\\')
                    pth = pth.Remove(pth.Length - 1);
            }
            else
            {
                pth = setup.GlobalFolder;
                pth = pth.Replace("<docs>", FastFind.GetDefaultPath());
                pth = pth.Replace("<dp>", Application.dataPath);
                if (pth[pth.Length - 1] == '/' || pth[pth.Length - 1] == '\\')
                    pth = pth.Remove(pth.Length - 1);

                if (!string.IsNullOrEmpty(setup.SceneProject))
                    pth += "/" + setup.SceneProject;
                if (pth[pth.Length - 1] == '/' || pth[pth.Length - 1] == '\\')
                    pth = pth.Remove(pth.Length - 1);
            }

            for (int l = 0; l < langs.Count; l++)
            {
                FilesSet.SaveStream(pth, setup.stringsPrefix + langs[l], setup.format, sv, false, false);
                for (int i = 0; i < svblocks.Count; i++)
                    FilesSet.SaveStream(pth, setup.commandsTextPrefix + langs[l] + "_" + i, setup.format, svblocks[i].ToArray(), false, false);
            }
        }
        else
		{
#if UNITY_EDITOR
            List<string> l = new List<string>();
            List<string> p = new List<string>();
            List<List<string>> st = new List<List<string>>();
            for (int i = 0; i < setup.customSets.Count; i++)
            {
                l.Add(setup.customSets[i].langName);

                if (setup.customSets[i].stringsFile != null)
                    p.Add(UnityEditor.AssetDatabase.GetAssetPath(setup.customSets[i].stringsFile));
                else
                    p.Add(null);
                //Debug.Log(AssetDatabase.GetAssetPath(setup.customSets[i].stringsFile));
                st.Add(new List<string>());
                for (int c = 0; c < setup.customSets[i].textFiles.Count; c++)
                    if (setup.customSets[i].textFiles[c] != null)
                        st[st.Count - 1].Add(UnityEditor.AssetDatabase.GetAssetPath(setup.customSets[i].textFiles[c]));
                    else
                        st[st.Count - 1].Add(null);
            }

            for (int i = 0; i < l.Count; i++)
            {

                if (!string.IsNullOrEmpty(p[i]) && !setup.dontUpdateStrings)
                {
                    if (FilesSet.CheckFile(p[i]) && jsonMode == fileMode._new)
                    {
                        SLM_ALSLBridge_LoaderStringsV2 curfile = JsonUtility.FromJson<SLM_ALSLBridge_LoaderStringsV2>(FilesSet.LoadStream(p[i], false, false));

                        SLM_ALSLBridge_LoaderStringsV2 tosave = setupStringsV2.Copy();

                        if (curfile == null || curfile.data == null)
                        {
                            curfile = new SLM_ALSLBridge_LoaderStringsV2();
                            curfile.data = new List<SLM_ALSLBridge_LoaderStringsV2_Key>();
                        }

                        for (int s = 0; s < tosave.data.Count; s++)
                        {
                            SLM_ALSLBridge_LoaderStringsV2_Key key = curfile.data.Find(f => f.key == tosave.data[s].key);

                            if (key != null)
                                tosave.data[s].word = key.word;
                        }

                        sv = JsonUtility.ToJson(tosave, true);

                    }
                    FilesSet.SaveStream(p[i], sv, false, false);
                }


                for (int a = 0; a < st[i].Count; a++)
                    if (!string.IsNullOrEmpty(st[i][a]) && !setup.dontUpdateTexts)
                        FilesSet.SaveStream(st[i][a], svblocks[a].ToArray(), false, false);
            }

#endif
            }

    }

    [Button("Get texts from files", ButtonMode.DisabledInPlayMode, ButtonSpacing.Before)]
    public void GetTextToCommand()
	{
        if (setup.useCustomSetUp)
		{
            if (setup.customSets[0].stringsFile != null)
            {
                setupStrings = JsonUtility.FromJson<SLM_ALSLBridge_LoaderStrings>((setup.customSets[0].stringsFile as TextAsset).text);
                setupStringsV2 = JsonUtility.FromJson<SLM_ALSLBridge_LoaderStringsV2>((setup.customSets[0].stringsFile as TextAsset).text);
			}
            for (int i=0;i<commands.blocks.Count;i++)
			{
                if (setup.customSets[0].textFiles[i] !=null)
                    commands.blocks[i].texts = (setup.customSets[0].textFiles[i] as TextAsset).text.Split('\n').ToList();
            }
        }
	}

   [Button("Update strings format to new", ButtonMode.DisabledInPlayMode, ButtonSpacing.After)]
   public void ConvertStringsFromOldToNew()
   {
      if (jsonMode == fileMode._old)
      {
         setupStringsV2 = new SLM_ALSLBridge_LoaderStringsV2();
         setupStringsV2.data = new List<SLM_ALSLBridge_LoaderStringsV2_Key>();
         for (int i = 0; i < setupStrings.words.Count; i++)
         {
            setupStringsV2.data.Add(new SLM_ALSLBridge_LoaderStringsV2_Key() { key = setupStrings.keys[i], word = setupStrings.words[i] });
         }

         jsonMode = fileMode._new;


      }
   }

    public string GetStringEditor(string key)
	{
        string ret = "";

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(key))
        {
            if (jsonMode == fileMode._old)
            {
                if (setupStrings.keys.Contains(key))
                {
                    if (setupStrings.words.Count > setupStrings.keys.IndexOf(key))
                        return setupStrings.words[setupStrings.keys.IndexOf(key)];
                }
            }
            else if (jsonMode == fileMode._new)
            {
                if (setupStringsV2.data.Find(f => f.key == key) != null)
                    return setupStringsV2.data.Find(f => f.key == key).word;

			}
        }
#endif

        return ret;
	}

    public string GetString (string key)
	{
		string ret = "";
		bool appPlays = true;
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
            appPlays = false;
#endif
        if (appPlays)
        {
            if (key.Contains("vw//gettextpoint//"))
                ret = commands.ValueWorkString(key);
            else if (loadedStrings != null && loadedStrings.data.Find(f => f.key == key) != null)
                ret = loadedStrings.data.Find(f => f.key == key).word;
            ret = ALSL_Main.FindKeysInString(ret);
        }
        else
        {
            if (jsonMode == fileMode._old)
            {
                if (setupStrings != null && setupStrings.keys.Contains(key))
                    ret = setupStrings.words[setupStrings.keys.IndexOf(key)];
            }
            else if (jsonMode == fileMode._new)
            {
                if (setupStringsV2 != null && setupStringsV2.data != null && setupStringsV2.data.Find(f => f.key == key) != null)
                    ret = setupStringsV2.data.Find(f => f.key == key).word;
			}
		}
        return ret;
	}

    public void LoadStrings()
    {
        if ((forceLoadedStrings == null || forceLoadedStrings.keys.Count==0) && !useOnlyForce)
        {
            loadedStrings = new SLM_ALSLBridge_LoaderStringsV2();

            if (setup.useALSLFolders)
            {
                string pth = "";
                if (!string.IsNullOrEmpty(setup.SceneProject))
                    pth += "/" + setup.SceneProject;
                if (pth[pth.Length - 1] == '/' || pth[pth.Length - 1] == '\\')
                    pth = pth.Remove(pth.Length - 1);
                string curr = pth + "/" + setup.stringsPrefix + ALSL_Main.alllangs[ALSL_Main.currentlang] + "." + setup.format;
                string def = pth + "/" + setup.stringsPrefix + ALSL_Main.alllangs[ALSL_Main.deflang] + "." + setup.format;

                string loaded = "";
                if (ALSL_Main.IsCurrentLangOutPut())
                {
                    string outputS = SaveSystemAlt.GetString("OutPutFiles");
                    if (!string.IsNullOrEmpty(outputS))
                    {
                        if (FilesSet.CheckFile(outputS + "/" + curr))
                            loaded = FilesSet.LoadStream(outputS + "/" + curr, false, false);
                        else
                            loaded = (setup.customSets.Find(f => f.langName == ALSL_Main.alllangs[ALSL_Main.deflang]).stringsFile as TextAsset).text;
                    }
                }
                else
                {
                    if (setup.useCustomSetUp)
                    {
                        if (setup.customSets.Find(f => f.langName == ALSL_Main.alllangs[ALSL_Main.currentlang])!=null)
                            loaded = (setup.customSets.Find(f => f.langName == ALSL_Main.alllangs[ALSL_Main.currentlang]).stringsFile as TextAsset).text;
                        else
                            loaded = (setup.customSets.Find(f => f.langName == ALSL_Main.alllangs[ALSL_Main.currentlang]).stringsFile as TextAsset).text;
                    }

                    if (!string.IsNullOrEmpty(SaveSystemAlt.GetString("OutPutFiles")))
                    {
                        if (!FilesSet.CheckDirectory(SaveSystemAlt.GetString("OutPutFiles") + "/" + pth))
                            FilesSet.CreateDirectory(SaveSystemAlt.GetString("OutPutFiles") + "/" + pth);
                        FilesSet.SaveStream(
                            SaveSystemAlt.GetString("OutPutFiles") + "/" + pth + "/" + setup.stringsPrefix + "Example" + "." + setup.format, loaded, false, false);
                    }
                }

                if (!string.IsNullOrEmpty(loaded))
                {
                    if (jsonMode == fileMode._old)
                        loadedStrings = Convertor(JsonUtility.FromJson<SLM_ALSLBridge_LoaderStrings>(loaded));
                    else if (jsonMode == fileMode._new)
                        loadedStrings = JsonUtility.FromJson<SLM_ALSLBridge_LoaderStringsV2>(loaded);
				}
            }
            else
            {
                string pth = setup.GlobalFolder;
                pth = pth.Replace("<docs>", FastFind.GetDefaultPath());
                pth = pth.Replace("<dp>", Application.dataPath);
                if (pth[pth.Length - 1] == '/' || pth[pth.Length - 1] == '\\')
                    pth = pth.Remove(pth.Length - 1);

                if (!string.IsNullOrEmpty(setup.SceneProject))
                    pth += "/" + setup.SceneProject;

                if (pth[pth.Length - 1] == '/' || pth[pth.Length - 1] == '\\')
                    pth = pth.Remove(pth.Length - 1);

                string curr = pth + "/" + setup.stringsPrefix + ALSL_Main.alllangs[ALSL_Main.currentlang] + "." + setup.format;
                string def = pth + "/" + setup.stringsPrefix + ALSL_Main.alllangs[ALSL_Main.deflang] + "." + setup.format;

                string loaded = "";

                if (setup.useCustomSetUp)
                {
                    if (setup.customSets.Find(f => f.langName == ALSL_Main.alllangs[ALSL_Main.currentlang]) != null)
                        loaded = (setup.customSets.Find(f => f.langName == ALSL_Main.alllangs[ALSL_Main.currentlang]).stringsFile as TextAsset).text;
                    else
                        loaded = (setup.customSets.Find(f => f.langName == ALSL_Main.alllangs[ALSL_Main.currentlang]).stringsFile as TextAsset).text;
                }
                else
                {
                    if (FilesSet.CheckFile(curr))
                        loaded = FilesSet.LoadStream(curr, false, false);
                    else
                        loaded = FilesSet.LoadStream(def, false, false);
                }

                if (!string.IsNullOrEmpty(loaded))
				{
					if (jsonMode == fileMode._old)
						loadedStrings = Convertor(JsonUtility.FromJson<SLM_ALSLBridge_LoaderStrings>(loaded));
					else if (jsonMode == fileMode._new)
						loadedStrings = JsonUtility.FromJson<SLM_ALSLBridge_LoaderStringsV2>(loaded);
				}
			}

            foreach (SLM_BridgeText bt in texts)
                bt.UpdateText();
        }
        else
            loadedStrings = Convertor(forceLoadedStrings);
    }
    
    public SLM_ALSLBridge_LoaderStringsV2 Convertor(SLM_ALSLBridge_LoaderStrings input)
    {
        SLM_ALSLBridge_LoaderStringsV2 ret = new SLM_ALSLBridge_LoaderStringsV2();

        ret.data = new List<SLM_ALSLBridge_LoaderStringsV2_Key>();


		for (int i=0;i<input.keys.Count;i++)
        {
            ret.data.Add(new SLM_ALSLBridge_LoaderStringsV2_Key() { key = input.keys[i], word = input.words[i] });
		}

        return ret;

	}

	public void LoadBlock(int id)
    {
        if ((forceLoadedText == null || forceLoadedText.Count==0) && !useOnlyForce)
        {
            if (setup.useALSLFolders)
            {
                string pth = "";
                if (!string.IsNullOrEmpty(setup.SceneProject))
                    pth += "/" + setup.SceneProject;
                if (pth[pth.Length - 1] == '/' || pth[pth.Length - 1] == '\\')
                    pth = pth.Remove(pth.Length - 1);
                string curr = pth + "/" + setup.commandsTextPrefix + ALSL_Main.alllangs[ALSL_Main.currentlang] + "_" + id + "." + setup.format;
                string def = pth + "/" + setup.commandsTextPrefix + ALSL_Main.alllangs[ALSL_Main.deflang] + "_" + id + "." + setup.format;

                if (ALSL_Main.IsCurrentLangOutPut())
                {
                    string outputS = SaveSystemAlt.GetString("OutPutFiles");
                    if (!string.IsNullOrEmpty(outputS))
                    {
                        if (FilesSet.CheckFile(outputS + "/" + curr))
                        {
                            List<string> ld = new List<string>();
                            ld = FilesSet.LoadStream(outputS + "/" + curr, false).ToList();

                            for (int i = 0; i < ld.Count; i++)
                                ld[i] = ld[i].Replace(setup.newLineSymbol, "\n");
                            loadedBlock = ld;
                        }
                        else
                        {
                            loadedBlock = (setup.customSets.Find(f => f.langName == ALSL_Main.alllangs[ALSL_Main.deflang]).textFiles[id] as TextAsset).text.Split('\n').ToList(); ;
                            for (int s = 0; s < loadedBlock.Count; s++)
                                loadedBlock[s] = loadedBlock[s].Replace(setup.newLineSymbol, "\n");
                        }

                    }
                }
                else
                {
                    string outputS = "ALSL";

                    if (setup.useCustomSetUp)
                    {
                        if (setup.customSets.Find(f => f.langName == ALSL_Main.alllangs[ALSL_Main.currentlang]) != null)
                            loadedBlock = (setup.customSets.Find(f => f.langName == ALSL_Main.alllangs[ALSL_Main.currentlang]).textFiles[id] as TextAsset).text.Split('\n').ToList();
                        else
                            loadedBlock = (setup.customSets.Find(f => f.langName == ALSL_Main.alllangs[ALSL_Main.currentlang]).textFiles[id] as TextAsset).text.Split('\n').ToList();
                    }

                    if (!string.IsNullOrEmpty(SaveSystemAlt.GetString("OutPutFiles")) && loadedBlock.Count > 0)
                    {
                        if (!FilesSet.CheckDirectory(SaveSystemAlt.GetString("OutPutFiles") + "/" + pth))
                            FilesSet.CreateDirectory(SaveSystemAlt.GetString("OutPutFiles") + "/" + pth);
                        string saved = "";

                        foreach (string s in loadedBlock)
                            saved += s + "\n";

                        FilesSet.SaveStream(
                            SaveSystemAlt.GetString("OutPutFiles") + "/" + pth + "/" + setup.commandsTextPrefix + "Example" + "_" + id + "." + setup.format, saved, false, false);
                    }
                }
            }
            else
            {
                string pth = setup.GlobalFolder;
                pth = pth.Replace("<docs>", FastFind.GetDefaultPath());
                pth = pth.Replace("<dp>", Application.dataPath);
                if (pth[pth.Length - 1] == '/' || pth[pth.Length - 1] == '\\')
                    pth = pth.Remove(pth.Length - 1);

                if (!string.IsNullOrEmpty(setup.SceneProject))
                    pth += "/" + setup.SceneProject;

                if (pth[pth.Length - 1] == '/' || pth[pth.Length - 1] == '\\')
                    pth = pth.Remove(pth.Length - 1);

                string curr = pth + "/" + setup.commandsTextPrefix + ALSL_Main.alllangs[ALSL_Main.currentlang] + "_" + id + "." + setup.format;
                string def = pth + "/" + setup.commandsTextPrefix + ALSL_Main.alllangs[ALSL_Main.deflang] + "_" + id + "." + setup.format;

                if (setup.useCustomSetUp)
                {
                    if (setup.customSets.Find(f => f.langName == ALSL_Main.alllangs[ALSL_Main.currentlang]) != null)
                        loadedBlock = (setup.customSets.Find(f => f.langName == ALSL_Main.alllangs[ALSL_Main.currentlang]).textFiles[id] as TextAsset).text.Split('\n').ToList();
                    else
                        loadedBlock = (setup.customSets.Find(f => f.langName == ALSL_Main.alllangs[ALSL_Main.currentlang]).textFiles[id] as TextAsset).text.Split('\n').ToList();
                }
                else
                {
                    List<string> ld = new List<string>();
                    if (FilesSet.CheckFile(curr))
                        ld = FilesSet.LoadStream(curr, false).ToList();
                    else
                        ld = FilesSet.LoadStream(def, false).ToList();

                    for (int i = 0; i < ld.Count; i++)
                        ld[i] = ld[i].Replace(setup.newLineSymbol, "\n");
                    loadedBlock = ld;
                }
            }
        }
        else
            loadedBlock = forceLoadedText;

        /*
        for (int i = 0; i < loadedBlock.Count; i++)
            {
                loadedBlock[i] = ALSL_Main.FindKeysInString(loadedBlock[i]);
            }]
        */

            for (int s = 0; s < loadedBlock.Count; s++)
            {
                loadedBlock[s] = loadedBlock[s].Replace(setup.newLineSymbol, "\n");
            }

            commands.blocks[id].texts = loadedBlock;
        }
    
}

[System.Serializable]
public class SLM_ALSLBridge_SetUp
{
    public bool useCustomSetUp;
    public bool useALSLFolders;
    public string GlobalFolder;
    public string SceneProject;
    public string newLineSymbol = "</n>";
    public string commandsTextPrefix="texts_";
    public string stringsPrefix="strings_";
    public string format = "lang";

    public bool dontUpdateStrings;
    public bool dontUpdateTexts;
    public List<SLM_ALSLBridge_LanfSetUp> customSets;
}

[System.Serializable]
public class SLM_ALSLBridge_LanfSetUp
{
    public string langName;
    public UnityEngine.Object stringsFile;
    public List<Object> textFiles;
}

[System.Serializable]
public class SLM_ALSLBridge_LoaderStrings
{
    public List<string> words;
    public List<string> keys;
}

[System.Serializable]
public class SLM_ALSLBridge_LoaderStringsV2
{
	public List<SLM_ALSLBridge_LoaderStringsV2_Key> data;

    public SLM_ALSLBridge_LoaderStringsV2 Copy()
    {
        SLM_ALSLBridge_LoaderStringsV2 ret = new SLM_ALSLBridge_LoaderStringsV2();

        ret.data = new List<SLM_ALSLBridge_LoaderStringsV2_Key>();

        foreach (SLM_ALSLBridge_LoaderStringsV2_Key key in data)
            ret.data.Add(key.Copy());

        return ret;
	}
}

[System.Serializable]
public class SLM_ALSLBridge_LoaderStringsV2_Key
{
	public string key;
	public string word;

    public SLM_ALSLBridge_LoaderStringsV2_Key Copy()
    {
        SLM_ALSLBridge_LoaderStringsV2_Key ret = new SLM_ALSLBridge_LoaderStringsV2_Key();

        ret.key = key;
        ret.word = word;

		return ret;

	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(SLM_ALSLBridge))]
[CanEditMultipleObjects]
public class SLM_ALSLBridgeEditor : Editor
{
    SerializedProperty commands;
    SerializedProperty jsonMode;
    SerializedProperty setupStringsV2;
    SerializedProperty ALSLDebugMode;
    SerializedProperty setup;

    void OnEnable()
    {
        commands = serializedObject.FindProperty("commands");
        ALSLDebugMode = serializedObject.FindProperty("ALSLDebugMode");
        setup = serializedObject.FindProperty("setup");
		jsonMode = serializedObject.FindProperty("jsonMode");
		setupStringsV2 = serializedObject.FindProperty("setupStringsV2");
    }

    public override void OnInspectorGUI()
    {
        SLM_ALSLBridge tg = (SLM_ALSLBridge)target;

        if (!Application.isPlaying)
        {
            if (GUILayout.Button("Add all exist langs"))
            {
                tg.AddAllLangs();
            }
            if (GUILayout.Button("Create new files"))
            {
                tg.CreateFiles();
            }
            if (GUILayout.Button("Update all files"))
            {
                tg.ForceRecreateFiles();
            }
            if (GUILayout.Button("Get texts from files"))
            {
                tg.GetTextToCommand();
            }
            if (GUILayout.Button("Update strings format to new"))
            {
                tg.ConvertStringsFromOldToNew();
            }
        }

		EditorGUILayout.PropertyField(commands);
        EditorGUILayout.PropertyField(jsonMode);
        EditorGUILayout.PropertyField(ALSLDebugMode);
        EditorGUILayout.PropertyField(setup);
        serializedObject.ApplyModifiedProperties();

        if (tg.jsonMode == SLM_ALSLBridge.fileMode._new)
        {
            EditorGUILayout.PropertyField(setupStringsV2);
			serializedObject.ApplyModifiedProperties();
		}
        else if (tg.jsonMode == SLM_ALSLBridge.fileMode._old)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Keys");
            for (int i = 0; i < tg.setupStrings.keys.Count; i++)
            {
                int id = i;

                tg.setupStrings.keys[id] = EditorGUILayout.TextField(tg.setupStrings.keys[id], GUILayout.Height(20));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Words");
            for (int i = 0; i < tg.setupStrings.words.Count; i++)
            {
                int id = i;

                tg.setupStrings.words[id] = EditorGUILayout.TextField(tg.setupStrings.words[id], GUILayout.Height(20));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("");
            for (int i = 0; i < tg.setupStrings.words.Count; i++)
            {
                int id = i;
                if (GUILayout.Button("-", GUILayout.Height(20)))
                {
                    tg.setupStrings.keys.RemoveAt(id);
                    tg.setupStrings.words.RemoveAt(id);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("");
            for (int i = 0; i < tg.setupStrings.words.Count; i++)
            {
                if (i != 0)
                {
                    int id = i;
                    int to = id - 1;
                    if (GUILayout.Button("▲", GUILayout.Height(20)))
                    {
                        if (to >= 0)
                        {
                            tg.setupStrings.keys.Move(id, to);
                            tg.setupStrings.words.Move(id, to);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }
                    }
                }
                else
                    GUILayout.Label("", GUILayout.Height(20));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("");
            for (int i = 0; i < tg.setupStrings.words.Count; i++)
            {
                if (i != tg.setupStrings.words.Count - 1)
                {
                    int id = i;
                    int to = id + 1;
                    if (GUILayout.Button("▼", GUILayout.Height(20)))
                    {
                        if (to < tg.setupStrings.keys.Count)
                        {
                            tg.setupStrings.keys.Move(id, to);
                            tg.setupStrings.words.Move(id, to);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }
                    }
                }
                else
                    GUILayout.Label("", GUILayout.Height(20));

            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Add"))
            {
                tg.setupStrings.keys.Add("");
                tg.setupStrings.words.Add("");
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
}
#endif