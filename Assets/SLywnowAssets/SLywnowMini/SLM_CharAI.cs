using AutoLangSLywnow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SLywnow;

public class SLM_CharAI : MonoBehaviour
{
    public bool useALSL;
    public SLM_ALSLBridge ALSLBridge;
    public SLM_RPGText textsc;
    public List<SLM_CharAI_Block> chars;

    public int currentId = -1;

    private void Awake()
    {
        textsc.textopt.colorSave = textsc.textopt.text.color;
        currentId = -1;
    }

    public int GetCharId(string name)
    {
        SLM_CharAI_Block b = chars.Find(f => f.name == name);
        if (b != null)
            return chars.IndexOf(b);
        return -1;
    }

    public void SelectChar(string name)
    {
        SLM_CharAI_Block b = chars.Find(f => f.name == name);

        if (b !=null)
        {

            if (currentId == -1)
                textsc.textopt.colorSave = textsc.textopt.text.color;

            currentId = chars.IndexOf(b);
            textsc.charcolor = chars[currentId].textColor;
            textsc.textopt.text.color = chars[currentId].textColor;
            textsc.textopt.charlogo.color = chars[currentId].textColor;
            if (textsc.textopt.charlogo != null)
            {
                if (useALSL)
                {
                    if (ALSLBridge == null)
                    {
                        textsc.textopt.charlogo.text = ALSL_Main.GetReplaceFromKey(chars[currentId].ALSLkey);
                    }
                    else
                    {
                        textsc.textopt.charlogo.text = ALSLBridge.GetString(chars[currentId].ALSLkey);
                    }
                }
                else
                    textsc.textopt.charlogo.text = chars[currentId].name;
            }

            if (chars[currentId].charIcon != null && textsc.textopt.charIcon != null)
            {
                textsc.textopt.charIcon.sprite = chars[currentId].charIcon;
                textsc.textopt.charIcon.color = new Color(1, 1, 1, 1);
            }

            UpdateEmotion();

            textsc.charvoice = chars[currentId].voice;
        }
    }

    public void DeselectChars()
    {
        textsc.textopt.text.color = textsc.textopt.colorSave;
        textsc.textopt.charlogo.color = textsc.textopt.colorSave;
        textsc.charcolor = new Color(0, 0, 0, 0);
        if (textsc.textopt.charlogo != null)
        {
            textsc.textopt.charlogo.text = "";
        }
        if (textsc.textopt.charIcon != null)
        {
            textsc.textopt.charIcon.sprite = null;
            textsc.textopt.charIcon.color = new Color(0, 0, 0, 0);
        }
        if (textsc.textopt.charEmotionLayer != null)
        {
            textsc.textopt.charEmotionLayer.sprite = null;
            textsc.textopt.charEmotionLayer.color = new Color(0, 0, 0, 0);
        }

        textsc.charvoice = null;

        currentId = -1;
    }

    public void UpdateEmotion()
	{
        if (currentId >= 0)
        {
            if (chars[currentId].useEmotions && (textsc.textopt.charEmotionLayer != null || chars[currentId].emotionType != SLM_CharAI_Block.emoTpe.layer)
                    && (textsc.textopt.charIcon != null || chars[currentId].emotionType != SLM_CharAI_Block.emoTpe.oneSprite))
            {
                Sprite toset = null;
                switch (chars[currentId].currentEmotion)
                {
                    case 0:
                        toset = chars[currentId].normal;
                        break;

                    case 1:
                        toset = chars[currentId].agressive;
                        break;

                    case 2:
                        toset = chars[currentId].embarrassed;
                        break;

                    case 3:
                        toset = chars[currentId].sad;
                        break;

                    case 4:
                        toset = chars[currentId].happy;
                        break;

                    default:
                        if (chars[currentId].currentEmotion > 4 && (chars[currentId].currentEmotion - 5) < chars[currentId].custom.Count && chars[currentId].currentEmotion - 5 >= 0)
                        {
                            toset = chars[currentId].custom[chars[currentId].currentEmotion - 5];
                        }
                        break;
                }

                if (toset != null)
                {
                    if (chars[currentId].emotionType == SLM_CharAI_Block.emoTpe.oneSprite)
                    {
                        textsc.textopt.charIcon.sprite = toset;
                        textsc.textopt.charIcon.color = new Color(1, 1, 1, 1);
                    }
                    if (chars[currentId].emotionType == SLM_CharAI_Block.emoTpe.layer)
                    {
                        textsc.textopt.charEmotionLayer.sprite = toset;
                        textsc.textopt.charEmotionLayer.color = new Color(1, 1, 1, 1);
                    }
                }
            }
        }
    }


    public void SetEmotion(string name)
    {
        if (currentId >= 0)
        {
            switch (name)
            {
                case "normal":
                    chars[currentId].currentEmotion = 0;
                    break;

                case "agressive":
                    chars[currentId].currentEmotion = 1;
                    break;

                case "embarrassed":
                    chars[currentId].currentEmotion = 2;
                    break;

                case "sad":
                    chars[currentId].currentEmotion = 3;
                    break;

                case "happy":
                    chars[currentId].currentEmotion = 4;
                    break;
            }

            UpdateEmotion();
        }
    }

    public void SetCustomEmotion(int id)
    {
        if (currentId >= 0)
        {
            if (chars[currentId].custom.Count > id)
                chars[currentId].currentEmotion = id + 5;
            UpdateEmotion();
        }
    }

    public void SetEmotionId(int id)
    {
        if (currentId >= 0)
        {
            chars[currentId].currentEmotion = id;
            UpdateEmotion();
        }
    }
}

[System.Serializable]
public class SLM_CharAI_Block
{
    public string name;
    public string ALSLkey;
    public Color textColor=Color.white;
    public Sprite charIcon;
    public AudioClip voice;
    public bool useEmotions;

    public enum emoTpe {oneSprite,layer };
    [ShowFromBool("useEmotions")]
    public emoTpe emotionType;
    [ShowFromBool("useEmotions")]
    public Sprite normal; //0
    [ShowFromBool("useEmotions")]
    public Sprite agressive; //1
    [ShowFromBool("useEmotions")]
    public Sprite embarrassed; //2
    [ShowFromBool("useEmotions")]
    public Sprite sad; //3
    [ShowFromBool("useEmotions")]
    public Sprite happy; //4
    [ShowFromBool("useEmotions")]
    public List<Sprite> custom; //n-5
    [ShowFromBool("useEmotions")]
    public int currentEmotion;
}