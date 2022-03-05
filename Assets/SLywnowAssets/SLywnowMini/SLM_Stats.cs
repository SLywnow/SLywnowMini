using AutoLangSLywnow;
using SLywnow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

public class SLM_Stats : MonoBehaviour
{
    public enum guiupd { off, everysecond, onUpdate};
    public guiupd GUIUpdate;
    public float timer;
    public bool useALSL;
    public List<SLM_Stats_Block> stats;
    public enum tpe { intType, floatType, stringType, boolType };

	private void Start()
	{
        for (int i = 0; i < stats.Count; i++)
            UpdateGUI(i);
    }

	float time;
    public void Update()
    {
        if (GUIUpdate == guiupd.onUpdate)
        {
            for (int i = 0; i < stats.Count; i++)
                UpdateGUI(i);
        }
        if (GUIUpdate == guiupd.onUpdate)
        {
            if (time < timer) timer += Time.deltaTime;
            else
			{
                for (int i = 0; i < stats.Count; i++)
                    UpdateGUI(i);
                time = 0;
            }
        }

    }

    public int GetStatID(string name)
	{
        SLM_Stats_Block b = stats.Find(f => f.name == name);
        if (b !=null)
            return stats.IndexOf(b);
        else
            return -1;
    }

    public SLM_Stats_Block GetStat(string name)
	{
        return stats.Find(f => f.name == name);
    }

    public void AddValue(string name, float value)
	{
        SLM_Stats_Block b = stats.Find(f => f.name == name);
        if (b != null)
        {
            int id = stats.IndexOf(b);
            if (stats[id].type==tpe.floatType || stats[id].type==tpe.intType)
			{
                string outnew="";
                if (stats[id].type == tpe.intType) 
                {
                    int newint = (int)Mathf.Clamp(int.Parse(stats[id].value) + (int)value, stats[id].min, stats[id].max);
                    outnew = newint+"";
                }
                if (stats[id].type == tpe.floatType)
                {
                    float newfloat = Mathf.Clamp(float.Parse(stats[id].value) + value, stats[id].min, stats[id].max);
                    outnew = newfloat + "";
                }
                stats[id].value = outnew;
                UpdateGUI(id);
            }
		}
    }

    public void SubtractValue(string name, float value)
	{
        SLM_Stats_Block b = stats.Find(f => f.name == name);
        if (b != null)
        {
            int id = stats.IndexOf(b);
            if (stats[id].type == tpe.floatType || stats[id].type == tpe.intType)
            {
                string outnew = "";
                if (stats[id].type == tpe.intType)
                {
                    int newint = (int)Mathf.Clamp(int.Parse(stats[id].value) - (int)value, stats[id].min, stats[id].max);
                    outnew = newint + "";
                }
                if (stats[id].type == tpe.floatType)
                {
                    float newfloat = Mathf.Clamp(float.Parse(stats[id].value) - value, stats[id].min, stats[id].max);
                    outnew = newfloat + "";
                }
                stats[id].value = outnew;
                UpdateGUI(id);
            }
        }
    }

    public void SetValue(string name, int value)
    {
        SLM_Stats_Block b = stats.Find(f => f.name == name);
        if (b != null)
        {
            int id = stats.IndexOf(b);
            if (stats[id].type == tpe.intType || stats[id].type == tpe.floatType)
            {
                stats[id].value = Mathf.Clamp(value, stats[id].min, stats[id].max) + "";
            }
            UpdateGUI(id);
        }
        else
		{
            stats.Add(new SLM_Stats_Block());
            stats[stats.Count - 1].name = name;
            stats[stats.Count - 1].type = tpe.intType;
            stats[stats.Count - 1].min = (float) int.MinValue;
            stats[stats.Count - 1].max = (float) int.MaxValue;
            stats[stats.Count - 1].value = value.ToString();
            stats[stats.Count - 1].gui = new SLM_Stats_GUI();
            UpdateGUI(stats.Count - 1);
        }
    }
    public void SetValue(string name, float value)
    {
        SLM_Stats_Block b = stats.Find(f => f.name == name);
        if (b != null)
        {
            int id = stats.IndexOf(b);
            if (stats[id].type == tpe.floatType)
            {
                stats[id].value = Mathf.Clamp(value, stats[id].min, stats[id].max) + "";
            }
            UpdateGUI(id);
        }
        else
        {
            stats.Add(new SLM_Stats_Block());
            stats[stats.Count - 1].name = name;
            stats[stats.Count - 1].type = tpe.floatType;
            stats[stats.Count - 1].min = float.MinValue;
            stats[stats.Count - 1].max = float.MaxValue;
            stats[stats.Count - 1].value = value.ToString();
            stats[stats.Count - 1].gui = new SLM_Stats_GUI();
            UpdateGUI(stats.Count - 1);
        }
    }
    public void SetValue(string name, bool value)
    {
        SLM_Stats_Block b = stats.Find(f => f.name == name);
        if (b != null)
        {
            int id = stats.IndexOf(b);
            if (stats[id].type == tpe.boolType)
            {
                stats[id].value = value.ToString();
            }
            UpdateGUI(id);
        }
        else
        {
            stats.Add(new SLM_Stats_Block());
            stats[stats.Count - 1].name = name;
            stats[stats.Count - 1].type = tpe.boolType;
            stats[stats.Count - 1].value = value.ToString();
            stats[stats.Count - 1].gui = new SLM_Stats_GUI();
            UpdateGUI(stats.Count - 1);
        }
    }
    public void SetValue(string name, string value)
    {
        SLM_Stats_Block b = stats.Find(f => f.name == name);
        if (b != null)
        {
            int id = stats.IndexOf(b);
            if (stats[id].type == tpe.stringType)
            {
                stats[id].value = value;
            }
            UpdateGUI(id);
        }
        else
        {
            stats.Add(new SLM_Stats_Block());
            stats[stats.Count - 1].name = name;
            stats[stats.Count - 1].type = tpe.stringType;
            stats[stats.Count - 1].value = value.ToString();
            stats[stats.Count - 1].gui = new SLM_Stats_GUI();
            UpdateGUI(stats.Count - 1);
        }
    }

    public void SetValueMax(string name, float value)
    {
        SLM_Stats_Block b = stats.Find(f => f.name == name);
        if (b != null)
        {
            int id = stats.IndexOf(b);
            if (stats[id].type == tpe.intType)
            {
                stats[id].max = (int)value;
            }
            else
                stats[id].max = value;

            UpdateGUI(id);
        }
    }
    public void SetValueMin(string name, float value)
    {
        SLM_Stats_Block b = stats.Find(f => f.name == name);
        if (b != null)
        {
            int id = stats.IndexOf(b);
            if (stats[id].type == tpe.intType)
            {
                stats[id].min = (int)value;
            }
            else
                stats[id].min = value;

            UpdateGUI(id);
        }
    }

    public void SetValueMinMax(string name, float min, float max)
    {
        SLM_Stats_Block b = stats.Find(f => f.name == name);
        if (b != null)
        {
            int id = stats.IndexOf(b);
            if (stats[id].type == tpe.intType)
            {
                stats[id].min = (int)min;
                stats[id].max = (int)max;
            }
            else
            {
                stats[id].min = min;
                stats[id].max = max;
            }

            UpdateGUI(id);
        }
    }

    public int GetValue(string name, int def=0)
	{
        int ret = def;
        SLM_Stats_Block b = stats.Find(f => f.name == name);
        if (b != null)
        {
            int id = stats.IndexOf(b);
            if (stats[id].type==tpe.intType)
			{
                ret = int.Parse(stats[id].value);
            }
        }
        return ret;
	}

    public float GetValue(string name, float def = 0)
    {
        float ret = def;

        SLM_Stats_Block b = stats.Find(f => f.name == name);
        if (b != null)
        {
            int id = stats.IndexOf(b);
            if (stats[id].type == tpe.floatType || stats[id].type == tpe.intType)
            {
                ret = float.Parse(stats[id].value);
            }
        }
        return ret;
    }

    public string GetValue(string name, string def="")
    {
        string ret = def;

        SLM_Stats_Block b = stats.Find(f => f.name == name);
        if (b != null)
        {
            int id = stats.IndexOf(b);
            if (stats[id].type == tpe.stringType)
            {
                ret = stats[id].value;
            }
        }
        return ret;
    }
    public bool GetValue(string name, bool def = false)
    {
        bool ret = def;

        SLM_Stats_Block b = stats.Find(f => f.name == name);
        if (b != null)
        {
            int id = stats.IndexOf(b);
            if (stats[id].type == tpe.boolType)
            {
                ret = bool.Parse(stats[id].value);
            }
        }
        return ret;
    }

    public void UpdateALSL(int id)
	{
        if (useALSL && !string.IsNullOrEmpty(stats[id].ALSLkey))
		{
            if (!ALSL_Main.keysR_alsl.Contains(stats[id].ALSLkey))
            {
                ALSL_Main.keysR_alsl.Add(stats[id].ALSLkey);
                ALSL_Main.repickR_alsl.Add(stats[id].value);
            }
            else
			{
                ALSL_Main.SetReplaceWord(stats[id].ALSLkey, stats[id].value);
            }
		}
	}

    private void UpdateGUI(int id)
    {
        UpdateALSL(id);
        SLM_Stats_GUI g = stats[id].gui;
        if (stats[id].type == tpe.intType || stats[id].type == tpe.floatType)
        {
            if (g.type == SLM_Stats_GUI.tpe.slider)
            {
                g.slider.value = float.Parse(stats[id].value) / stats[id].max;
            }
            else if (g.type == SLM_Stats_GUI.tpe.image)
            {
                g.image.fillAmount = float.Parse(stats[id].value) / stats[id].max;
            }
        }
        if (stats[id].type == tpe.boolType && g.type != SLM_Stats_GUI.tpe.off)
            g.forBool.isOn = bool.Parse(stats[id].value);

        if ((stats[id].type == tpe.boolType|| stats[id].type == tpe.stringType) && g.textType != SLM_Stats_GUI.txttpe.off)
            g.text.text = g.prefix + stats[id].value + g.suffix;

        float val = 0;
        if (!(g.textType==SLM_Stats_GUI.txttpe.off || g.textType == SLM_Stats_GUI.txttpe.percent))
		{
            if (g.typeOfValue == SLM_Stats_GUI.txtval.intType)
                val = (int) (float.Parse(stats[id].value));
            else if (g.typeOfValue == SLM_Stats_GUI.txtval.FloatType)
			{
                val = float.Parse(stats[id].value);
                val = (float)Math.Round(val, g.round);
            }
		}

        if (g.textType == SLM_Stats_GUI.txttpe.percent)
		{
            int per = (int)((float.Parse(stats[id].value) / stats[id].max) * 100);
            g.text.text = g.prefix+ per + "%"+ g.suffix;
        }
        else if (g.textType == SLM_Stats_GUI.txttpe.curVal)
		{
            g.text.text = g.prefix + val + g.suffix;
        }
        else if (g.textType == SLM_Stats_GUI.txttpe.curMaxVal)
        {
            g.text.text = g.prefix + val + g.separator + stats[id].max + g.suffix;
        }
    }
}

[System.Serializable]
public class SLM_Stats_Block
{
    public string name;
    public string ALSLkey;
    public SLM_Stats.tpe type;
    public string value;
    [ShowFromMultiple("type", new string[2] { "0", "1" }, "enum", ShowFromMultipleAttribute.mode.or)]
    public float min=0;
    [ShowFromMultiple("type", new string[2] { "0", "1" }, "enum" , ShowFromMultipleAttribute.mode.or)]
    public float max=100;
    public SLM_Stats_GUI gui;
}

[System.Serializable]
public class SLM_Stats_GUI
{
    public enum tpe { off, slider, image };
    public tpe type;
    [ShowFromEnum("type", 1)]
    public Slider slider;
    [ShowFromEnum("type", 2)]
    public Image image;

    public Toggle forBool;
    public enum txttpe { off, curMaxVal, curVal, percent };
    public txttpe textType;
    [ShowFromEnum("textType", 1)]
    public string separator = "/";
    [ShowFromEnum("textType", 0, true)]
    public string prefix;
    [ShowFromEnum("textType", 0, true)]
    public string suffix;
    public enum txtval { intType, FloatType };
    public txtval typeOfValue;
    [ShowFromEnum("typeOfValue", 1)]
    public int round = 2;
    [ShowFromEnum("textType", 1, true)]
    public Text text;
}