using SLywnow;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SLM_QTEEvents : MonoBehaviour
{
    [Header("Main")]
    public GameObject qteObject;
    public Text timerText;
    public Slider timerSlider;
    public Slider progressSlider;
    [Header("For find")]
    public RectTransform spawnArea;
    [HideInInspector] public Vector2 spawnMin;
    [HideInInspector] public Vector2 spawnMax;
    public Button spawnobj;

    [Header("For concentrate and reaction")]
    public Slider concSlider;
    public Image concSliderBackground;
    [Header("For reaction")]
    public Text lifeText;
    public Slider lifeSlider;

    [Header("Setup")]
    public List<SLM_QTEEvents_Button> buttons;
    public List<SLM_QTEEvents_Preset> presets;

    [HideInInspector] public int curid=-1;
    [HideInInspector] SLM_Commands comm;
    [HideInInspector] public int winC;
    [HideInInspector] public int loseC;
    [HideInInspector] public UnityEvent winE;
    [HideInInspector] public UnityEvent loseE;
    [HideInInspector] public List<string> pressed = new List<string>();


    float timer=0;
    int stage = 0;
    int curcount;
    float curpos;
    bool ok=false;
    bool positive;
    GameObject spawned;
    int savestage;
    List<string> butts = new List<string>();

    float rndtimer;
    float concSpeed;

    float reactPos;
    float reactMinPos;
    float reactMaxPos;
    int curlife;



    public void Start()
	{
        if (spawnArea !=null)
		{
            Vector2 curMin;
            Vector2 curMax;

            curMin.x = (-(spawnArea.sizeDelta.x / 2)) + spawnArea.anchoredPosition.x;
            curMin.y = (-(spawnArea.sizeDelta.y / 2)) + spawnArea.anchoredPosition.y;
            curMax.x = (spawnArea.sizeDelta.x / 2) + spawnArea.anchoredPosition.x;
            curMax.y = (spawnArea.sizeDelta.y / 2) + spawnArea.anchoredPosition.y;

            spawnMin.x = curMin.x <= curMax.x ? curMin.x : curMax.x;
            spawnMin.y = curMin.y <= curMax.y ? curMin.y : curMax.y;
            spawnMax.x = curMin.x > curMax.x ? curMin.x : curMax.x;
            spawnMax.y = curMin.y > curMax.y ? curMin.y : curMax.y;
        }
        if (spawnobj !=null)
            spawnobj.gameObject.SetActive(false);
        if (progressSlider !=null)
            progressSlider.gameObject.SetActive(false);
        if (timerSlider != null)
            timerSlider.gameObject.SetActive(false);
        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (concSlider !=null)
            concSlider.gameObject.SetActive(false);
        if (lifeText != null)
            lifeText.gameObject.SetActive(false);
        if (lifeSlider != null)
            lifeSlider.gameObject.SetActive(false);

        qteObject.SetActive(false);
    }

	public void Update()
	{
		if (curid>=0)
		{
            if (timer < presets[curid].time || presets[curid].QteType == SLM_QTEEvents_Preset.qtetpe.reaction)
            {
                if (presets[curid].QteType != SLM_QTEEvents_Preset.qtetpe.reaction)
                {
                    timer += Time.deltaTime;
                    if (timerSlider != null && !presets[curid].disableTimerVis)
                        timerSlider.value = 1 - (timer / presets[curid].time);

                    if (timerText != null && !presets[curid].disableTimerVis)
                        timerText.text = (int)(presets[curid].time - timer) + "";
                }

                if (presets[curid].QteType == SLM_QTEEvents_Preset.qtetpe.basic)
                {
                    if (stage < presets[curid].buttonIds.Count)
                    {
                        if (progressSlider != null && !presets[curid].disableProgressVis)
                            progressSlider.value = ((float)stage / (float)(presets[curid].buttonIds.Count - 1));

                        if (savestage != stage)
                        {
                            if (presets[curid].buttonIds[stage].Contains("random") || presets[curid].buttonIds[stage].Contains("r"))
                            {
                                butts = new List<string>();
                                int c = int.Parse(presets[curid].buttonIds[stage].Split('|')[1]);
                                if (c >= buttons.Count)
                                {
                                    for (int i = 0; i < buttons.Count; i++)
                                    { butts.Add(i.ToString()); }
                                }
                                else
                                {
                                    while (butts.Count < c)
                                    {
                                        int r = Random.Range(0, buttons.Count);
                                        if (!butts.Contains(r.ToString()) && !buttons[r].notInRandom)
                                        {
                                            //Debug.Log(r);
                                            butts.Add(r.ToString());
                                        }
                                    }
                                }

                            }
                            else
                                butts = presets[curid].buttonIds[stage].Split('|').ToList();

                            foreach (string b in butts)
                            {
                                buttons[int.Parse(b)].pressed = false;
                            }

                            savestage = stage;
                        }

                        List<int> nonbutts = new List<int>();
                        for (int i = 0; i < buttons.Count; i++)
                        {
                            if (!butts.Contains(i.ToString()))
                                nonbutts.Add(i);
                        }

                        int need = butts.Count;
                        int has = 0;
                        foreach (string b in butts)
                        {
                            SLM_QTEEvents_Button bt = buttons[int.Parse(b)];
                            if (bt.obj != null && !bt.obj.activeSelf)
                                bt.obj.SetActive(true);

                            Input.multiTouchEnabled = true;
                            if (bt.useInput)
                            {
                                if (presets[curid].mode[stage] == SLM_QTEEvents_Preset.tpe.pressTogether)
                                {
                                    if (Input.GetButtonDown(bt.name) || pressed.Contains(bt.name))
                                        has++;
                                }
                                else if (presets[curid].mode[stage] == SLM_QTEEvents_Preset.tpe.withhold)
                                {
                                    if (Input.GetButton(bt.name) || pressed.Contains(bt.name))
                                        has++;
                                }
                                else if (presets[curid].mode[stage] == SLM_QTEEvents_Preset.tpe.savePress)
                                {
                                    if (bt.pressed) has++;
                                    else if (Input.GetButton(bt.name) || pressed.Contains(bt.name))
                                    {
                                        bt.pressed = true;
                                        if (presets[curid].useColor)
                                            bt.obj.GetComponent<Image>().color = presets[curid].ColorPressed;
                                        has++;
                                    }
                                }
                            }
                            else
                            {
                                if (presets[curid].mode[stage] == SLM_QTEEvents_Preset.tpe.pressTogether)
                                {
                                    if (Input.GetKeyDown(bt.key) || pressed.Contains(bt.name))
                                        has++;
                                }
                                else if (presets[curid].mode[stage] == SLM_QTEEvents_Preset.tpe.withhold)
                                {
                                    if (Input.GetKey(bt.key) || pressed.Contains(bt.name))
                                        has++;
                                }
                                else if (presets[curid].mode[stage] == SLM_QTEEvents_Preset.tpe.savePress)
                                {
                                    if (bt.pressed) has++;
                                    else if (Input.GetKey(bt.key) || pressed.Contains(bt.name))
                                    {
                                        bt.pressed = true;
                                        if (presets[curid].useColor)
                                            bt.obj.GetComponent<Image>().color = presets[curid].ColorPressed;
                                        has++;
                                    }
                                }
                            }
                        }
                        if (presets[curid].missing != SLM_QTEEvents_Preset.tpemiss.none)
                        {
                            foreach (int b in nonbutts)
                            {
                                SLM_QTEEvents_Button bt = buttons[b];
                                //Debug.Log(bt.name);
                                bool miss = false;
                                if (bt.useInput)
                                {
                                    if (Input.GetButtonDown(bt.name) || pressed.Contains(bt.name))
                                        miss = true;
                                }
                                else
                                    if (Input.GetKeyDown(bt.key) || pressed.Contains(bt.name))
                                    miss = true;

                                if (miss)
                                {
                                    if (presets[curid].missing == SLM_QTEEvents_Preset.tpemiss.fail)
                                        EndQTE(true);
                                    else if (presets[curid].missing == SLM_QTEEvents_Preset.tpemiss.fromstart)
                                    {
                                        stage = 0;
                                        //Debug.Log(stage);
                                        ok = false;
                                        foreach (string bs in butts)
                                        {
                                            if (buttons[int.Parse(bs)].obj != null)
                                                buttons[int.Parse(bs)].obj.SetActive(false);
                                            if (presets[curid].useColor)
                                                buttons[int.Parse(bs)].obj.GetComponent<Image>().color = presets[curid].NonPressed;
                                            buttons[int.Parse(bs)].pressed = false;
                                        }
                                    }
                                    else if (presets[curid].missing == SLM_QTEEvents_Preset.tpemiss.oneback)
                                    {
                                        if (stage > 0)
                                            stage--;
                                        //Debug.Log(stage);
                                        ok = false;
                                        foreach (string bs in butts)
                                        {
                                            if (buttons[int.Parse(bs)].obj != null)
                                                buttons[int.Parse(bs)].obj.SetActive(false);
                                            if (presets[curid].useColor)
                                                buttons[int.Parse(bs)].obj.GetComponent<Image>().color = presets[curid].NonPressed;
                                            buttons[int.Parse(bs)].pressed = false;
                                        }
                                    }
                                }
                            }
                        }

                        //Debug.Log(has + "/" + need);

                        if (has >= need)
                        {
                            ok = true;
                        }
                        has = 0;
                        need = 0;

                        if (ok)
                        {
                            stage++;
                            pressed = new List<string>();
                            //Debug.Log(stage);
                            ok = false;
                            foreach (string b in butts)
                            {
                                if (buttons[int.Parse(b)].obj != null)
                                    buttons[int.Parse(b)].obj.SetActive(false);
                                if (presets[curid].useColor)
                                    buttons[int.Parse(b)].obj.GetComponent<Image>().color = presets[curid].NonPressed;
                                buttons[int.Parse(b)].pressed = false;
                            }
                        }
                    }
                    else
                    {
                        EndQTE(false);
                    }
                }
                else if (presets[curid].QteType == SLM_QTEEvents_Preset.qtetpe.find)
				{
                    if (curcount !=stage)
					{
                        stage = curcount;
                        if (spawned != null)
                            Destroy(spawned);

                        if (progressSlider != null && !presets[curid].disableProgressVis)
                            progressSlider.value = (float)(curcount-1) / (float)(presets[curid].clickCount-1);

                        //Debug.Log(curcount + " / " + presets[curid].clickCount);
                        
                        if (curcount == presets[curid].clickCount+1)
                        {
                            EndQTE(false);
                        }
                        else
                        {
                            spawned = Instantiate(spawnobj.gameObject, spawnArea);
                            spawned.SetActive(true);
                            spawned.GetComponent<Button>().onClick.AddListener(() => Press());
                            spawned.GetComponent<RectTransform>().anchoredPosition = new Vector2(Random.Range(spawnMin.x, spawnMax.x), Random.Range(spawnMin.y, spawnMax.y));
                        }
                    }
				}
                else if (presets[curid].QteType == SLM_QTEEvents_Preset.qtetpe.fast)
                {
                    if (progressSlider != null && !presets[curid].disableProgressVis)
                        progressSlider.value = curpos;
                    if (curpos >= 1)
                    {
                        EndQTE(false);
                    }
                    else
                    {
                        curpos -= presets[curid].removePerSec * Time.deltaTime;

                        if (positive)
						{
                            if (buttons[presets[curid].buttonPossitive].useInput)
                            {
                                if (Input.GetButtonDown(buttons[presets[curid].buttonPossitive].name) || pressed.Contains(buttons[presets[curid].buttonNegative].name))
                                {
                                    if (presets[curid].useColor)
                                    {
                                        buttons[presets[curid].buttonPossitive].obj.GetComponent<Image>().color = presets[curid].ColorPressed;
                                        buttons[presets[curid].buttonNegative].obj.GetComponent<Image>().color = presets[curid].NonPressed;
                                    }

                                    if (pressed.Contains(buttons[presets[curid].buttonNegative].name))
                                        pressed.Remove(buttons[presets[curid].buttonNegative].name);
                                    curpos += presets[curid].addCount;
                                    positive = false;
                                }
                            }
                            else
							{
                                if (Input.GetKeyDown(buttons[presets[curid].buttonPossitive].key) || pressed.Contains(buttons[presets[curid].buttonNegative].name))
                                {
                                    if (presets[curid].useColor)
                                    {
                                        buttons[presets[curid].buttonPossitive].obj.GetComponent<Image>().color = presets[curid].ColorPressed;
                                        buttons[presets[curid].buttonNegative].obj.GetComponent<Image>().color = presets[curid].NonPressed;
                                    }

                                    if (pressed.Contains(buttons[presets[curid].buttonNegative].name))
                                        pressed.Remove(buttons[presets[curid].buttonNegative].name);
                                    curpos += presets[curid].addCount;
                                    positive = false;
                                }
                            }
                        }
                        else
						{
                            if (buttons[presets[curid].buttonPossitive].useInput)
                            {
                                if (Input.GetButtonDown(buttons[presets[curid].buttonNegative].name) || pressed.Contains(buttons[presets[curid].buttonPossitive].name))
                                {
                                    if (presets[curid].useColor)
                                    {
                                        buttons[presets[curid].buttonPossitive].obj.GetComponent<Image>().color = presets[curid].NonPressed;
                                        buttons[presets[curid].buttonNegative].obj.GetComponent<Image>().color = presets[curid].ColorPressed;
                                    }

                                    if (pressed.Contains(buttons[presets[curid].buttonPossitive].name))
                                        pressed.Remove(buttons[presets[curid].buttonPossitive].name);
                                    curpos += presets[curid].addCount;
                                    positive = true;
                                }
                            }
                            else
                            {
                                if (Input.GetKeyDown(buttons[presets[curid].buttonNegative].key) || pressed.Contains(buttons[presets[curid].buttonPossitive].name))
                                {
                                    if (presets[curid].useColor)
                                    {
                                        buttons[presets[curid].buttonPossitive].obj.GetComponent<Image>().color = presets[curid].NonPressed;
                                        buttons[presets[curid].buttonNegative].obj.GetComponent<Image>().color = presets[curid].ColorPressed;
                                    }

                                    if (pressed.Contains(buttons[presets[curid].buttonPossitive].name))
                                        pressed.Remove(buttons[presets[curid].buttonPossitive].name);
                                    curpos += presets[curid].addCount;
                                    positive = true;
                                }
                            }
                        }

                    }
                }
                else if (presets[curid].QteType == SLM_QTEEvents_Preset.qtetpe.concentrate)
				{
                    if (rndtimer>0)
					{
                        rndtimer -= Time.deltaTime;
                        int moveind = 0;

                        if (buttons[presets[curid].buttonPossitive].useInput)
                        {
                            if (Input.GetButton(buttons[presets[curid].buttonPossitive].name) || pressed.Contains(buttons[presets[curid].buttonPossitive].name))
                            {
                                moveind = 1;
                            }
                        }
                        else
                        {
                            if (Input.GetKey(buttons[presets[curid].buttonPossitive].key) || pressed.Contains(buttons[presets[curid].buttonPossitive].name))
                            {
                                moveind = 1;
                            }
                        }

                        if (buttons[presets[curid].buttonNegative].useInput)
                        {
                            if (Input.GetButton(buttons[presets[curid].buttonNegative].name) || pressed.Contains(buttons[presets[curid].buttonNegative].name))
                            {
                                moveind = -1;
                            }
                        }
                        else
                        {
                            if (Input.GetKey(buttons[presets[curid].buttonNegative].key) || pressed.Contains(buttons[presets[curid].buttonNegative].name))
                            {
                                moveind = -1;
                            }
                        }

                        concSlider.value += (concSpeed + (moveind * presets[curid].pressSpeed)) * Time.deltaTime;

                    }
                    else
					{
                        rndtimer = Random.Range(presets[curid].rndTimeMin, presets[curid].rndTimeMax);
                        int rnd = Random.Range(1, 100);
                        if (rnd<= presets[curid].ChanceToChangeDir)
						{
                            concSpeed = concSpeed * (-1);
                        }
					}
				}
                else if (presets[curid].QteType == SLM_QTEEvents_Preset.qtetpe.reaction)
				{
                    bool fail = false;
                    if (concSlider.value < 1)
                    {
                        concSlider.value += presets[curid].headerSpeed * Time.deltaTime;

                        bool presed = false;
                        if (buttons[presets[curid].buttonReaction].useInput)
                        {
                            if (Input.GetButtonDown(buttons[presets[curid].buttonReaction].name) || pressed.Contains(buttons[presets[curid].buttonReaction].name))
                            {
                                presed = true;
                            }
                        }
                        else
                        {
                            if (Input.GetKeyDown(buttons[presets[curid].buttonReaction].key) || pressed.Contains(buttons[presets[curid].buttonReaction].name))
                            {
                                presed = true;
                            }
                        }

                        if (presed)
						{
                            if (pressed.Contains(buttons[presets[curid].buttonReaction].name))
                                pressed.Remove(buttons[presets[curid].buttonReaction].name);

                            if (concSlider.value >= reactMinPos && concSlider.value <= reactMaxPos)
                            {
                                curpos++;
                                if (progressSlider != null && !presets[curid].disableProgressVis)
                                    progressSlider.value = (curpos / (presets[curid].gamesCount - 1));

                                if (curpos >= presets[curid].gamesCount)
                                    EndQTE(false);
                                else
                                {
                                    concSlider.value = 0;
                                    GenerateRandomReactionPos();
                                }
                            }
                            else
							{
                                if (presets[curid].reactionFailMode==SLM_QTEEvents_Preset.reactFailMode.fail)
								{
                                    fail = true;
                                }
							}
                        }
                    }
					else
					{
                        fail = true;

                    }


                    if (fail)
					{
                        if (curlife == 0)
                            EndQTE(true);
                        else
                        {
                            curlife--;
                            if (lifeText != null)
                                lifeText.text = curlife + "";
                            if (lifeSlider != null)
                                lifeSlider.value = curlife;

                            concSlider.value = 0;

                            GenerateRandomReactionPos();
                        }
                    }
                }
            }
            else
            {
                if (presets[curid].QteType != SLM_QTEEvents_Preset.qtetpe.concentrate)
                    EndQTE(true);
                else
				{
                    if (presets[curid].QteType == SLM_QTEEvents_Preset.qtetpe.concentrate)
					{
                        if (concSlider.value >= 0.5 - presets[curid].safeSize && concSlider.value <= 0.5 + presets[curid].safeSize)
                            EndQTE(false);
                        else
                            EndQTE(true);
					}
                }
            }
        }
	}

    public void RunQTE(int id, SLM_Commands com, int win, int lose)
    {
        if (id >= 0 && id < presets.Count)
        {
            qteObject.SetActive(true);
            timer = 0;
            curid = id;
            stage = 0;
            curcount = 1;
            comm = com;
            positive = false;
            winC = win;
            loseC = lose;
            curpos = 0;
            savestage = -1;

            ActivateQTE();
        }
    }

    public void RunQTE(string name, SLM_Commands com, int win, int lose)
    {
        if (presets.Find(f => f.name == name) != null) 
        {
            qteObject.SetActive(true);
            timer = 0;
            curid = presets.IndexOf(presets.Find(f => f.name == name));
            stage = 0;
            curcount = 1;
            comm = com;
            positive = false;
            winC = win;
            loseC = lose;
            curpos = 0;
            savestage = -1;

            ActivateQTE();
        }
    }

    public void RunQTE(int id, UnityEvent win, UnityEvent lose)
    {
        if (id >= 0 && id < presets.Count)
        {
            qteObject.SetActive(true);
            timer = 0;
            curid = id;
            stage = 0;
            curcount = 1;
            positive = false;
            comm = null;
            winE = win;
            loseE = lose;
            curpos = 0;
            savestage = -1;

            ActivateQTE();
        }
    }

    public void RunQTE(string name, UnityEvent win, UnityEvent lose)
    {
        if (presets.Find(f => f.name == name) != null)
        {
            qteObject.SetActive(true);
            timer = 0;
            curid = presets.IndexOf(presets.Find(f => f.name == name));
            stage = 0;
            curcount = 1;
            positive = false;
            comm = null;
            winE = win;
            loseE = lose;
            curpos = 0;
            savestage = -1;

            ActivateQTE();
        }
    }

    void ActivateQTE ()
	{
        pressed = new List<string>();

        if (progressSlider != null && !presets[curid].disableProgressVis)
        {
            progressSlider.gameObject.SetActive(true);
            progressSlider.minValue = 0;
            progressSlider.maxValue = 1;
        }
        if (timerSlider != null && !presets[curid].disableTimerVis)
        {
            timerSlider.gameObject.SetActive(true);
            timerSlider.minValue = 0;
            timerSlider.maxValue = 1;
        }
        if (timerText != null && !presets[curid].disableTimerVis)
            timerText.gameObject.SetActive(true);



        if (presets[curid].QteType == SLM_QTEEvents_Preset.qtetpe.fast)
        {
            buttons[presets[curid].buttonNegative].obj.SetActive(true);
            buttons[presets[curid].buttonPossitive].obj.SetActive(true);
            if (presets[curid].useColor)
            {
                buttons[presets[curid].buttonPossitive].obj.GetComponent<Image>().color = presets[curid].ColorPressed;
                buttons[presets[curid].buttonNegative].obj.GetComponent<Image>().color = presets[curid].NonPressed;
            }
        }
        else if (presets[curid].QteType == SLM_QTEEvents_Preset.qtetpe.concentrate)
		{

            buttons[presets[curid].buttonNegative].obj.SetActive(true);
            buttons[presets[curid].buttonPossitive].obj.SetActive(true);
            concSlider.gameObject.SetActive(true);
            concSlider.value = 0.5f;
            concSlider.minValue = 0;
            concSlider.maxValue = 1;

            concSpeed = presets[curid].headerSpeed;

            concSlider.handleRect.GetComponent<Image>().color = presets[curid].HeaderColorConc;
            Gradient gen = new Gradient();
            gen.mode = GradientMode.Blend;
            GradientAlphaKey[] alph = new GradientAlphaKey[1] { new GradientAlphaKey(1,0) };
            List<GradientColorKey> cols = new List<GradientColorKey>();
            cols.Add(new GradientColorKey(presets[curid].failAreaColorConc, 0));
            cols.Add(new GradientColorKey(presets[curid].safeAreaColorConc, 0.5f- (presets[curid].safeSize/2)));
            cols.Add(new GradientColorKey(presets[curid].safeAreaColorConc/ 1.3f, 0.5f - (presets[curid].safeSize / 1.9f)));
            cols.Add(new GradientColorKey(presets[curid].safeAreaColorConc, 0.5f+ (presets[curid].safeSize/2)));
            cols.Add(new GradientColorKey(presets[curid].safeAreaColorConc / 1.3f, 0.5f + (presets[curid].safeSize / 1.9f)));
            cols.Add(new GradientColorKey(presets[curid].failAreaColorConc, 1));
            gen.SetKeys(cols.ToArray(), alph);
            concSliderBackground.sprite = UIEditor.GetSpriteWithGradient(gen, true, 1000, 10);

            if (progressSlider != null)
                progressSlider.gameObject.SetActive(false);

        }
        else if (presets[curid].QteType == SLM_QTEEvents_Preset.qtetpe.reaction)
        {
            curlife = presets[curid].lifes;
            curpos = 0;

            if (lifeText != null)
            {
                lifeText.gameObject.SetActive(true);
                lifeText.text = curlife + "";
            }
            if (lifeSlider != null)
            {
                lifeSlider.gameObject.SetActive(true);
                lifeSlider.minValue = 0;
                lifeSlider.maxValue = presets[curid].lifes;
                lifeSlider.value = curlife;
            }
            if (timerSlider != null)
                timerSlider.gameObject.SetActive(false);
            if (timerText != null )
                timerText.gameObject.SetActive(false);
            if (progressSlider != null && !presets[curid].disableProgressVis)
            {
				progressSlider.value = 0;
            }
            if (progressSlider != null && presets[curid].disableProgressVis)
                progressSlider.gameObject.SetActive(false);

			buttons[presets[curid].buttonReaction].obj.SetActive(true);
            concSlider.gameObject.SetActive(true);
            concSlider.value = 0;
            concSlider.minValue = 0;
            concSlider.maxValue = 1;
            GenerateRandomReactionPos();
        }
    }

    void GenerateRandomReactionPos()
	{
        reactPos = Random.Range(presets[curid].reactionMinPos, presets[curid].reactionMaxPos);
        reactMinPos = reactPos - presets[curid].reactionSafeZone / 2;
        reactMaxPos = reactPos + presets[curid].reactionSafeZone / 2;

        concSlider.handleRect.GetComponent<Image>().color = presets[curid].HeaderColorReact;
        Gradient gen = new Gradient();
        gen.mode = GradientMode.Blend;
        GradientAlphaKey[] alph = new GradientAlphaKey[1] { new GradientAlphaKey(1, 0) };
        List<GradientColorKey> cols = new List<GradientColorKey>();
        cols.Add(new GradientColorKey(presets[curid].failAreaColorReact, 0));
        cols.Add(new GradientColorKey(presets[curid].safeAreaColorReact, reactPos - (presets[curid].reactionSafeZone / 2)));
        cols.Add(new GradientColorKey(presets[curid].safeAreaColorReact / 1.3f, reactPos - (presets[curid].reactionSafeZone / 1.9f)));
        cols.Add(new GradientColorKey(presets[curid].safeAreaColorReact, reactPos + (presets[curid].reactionSafeZone / 2)));
        cols.Add(new GradientColorKey(presets[curid].safeAreaColorReact / 1.3f, reactPos + (presets[curid].reactionSafeZone / 1.9f)));
        cols.Add(new GradientColorKey(presets[curid].failAreaColorReact, 1));
        gen.SetKeys(cols.ToArray(), alph);
        concSliderBackground.sprite = UIEditor.GetSpriteWithGradient(gen, true, 1000, 10);
    }

    public void EndQTE(bool lose)
	{
        qteObject.SetActive(false);
        timer = 0;

        if (progressSlider != null)
            progressSlider.gameObject.SetActive(false);
        if (timerSlider != null)
            timerSlider.gameObject.SetActive(false);
        if (timerText != null)
            timerText.gameObject.SetActive(false);
        if (lifeText != null)
            lifeText.gameObject.SetActive(false);
        if (lifeSlider != null)
            lifeSlider.gameObject.SetActive(false);

        if (presets[curid].QteType == SLM_QTEEvents_Preset.qtetpe.fast)
        {
            buttons[presets[curid].buttonNegative].obj.SetActive(false);
            buttons[presets[curid].buttonPossitive].obj.SetActive(false);
            if (presets[curid].useColor)
            {
                buttons[presets[curid].buttonPossitive].obj.GetComponent<Image>().color = presets[curid].NonPressed;
                buttons[presets[curid].buttonNegative].obj.GetComponent<Image>().color = presets[curid].NonPressed;
            }
        }
        else if (presets[curid].QteType == SLM_QTEEvents_Preset.qtetpe.basic)
        {

        }
        else if (presets[curid].QteType == SLM_QTEEvents_Preset.qtetpe.concentrate)
        {
            buttons[presets[curid].buttonNegative].obj.SetActive(false);
            buttons[presets[curid].buttonPossitive].obj.SetActive(false);
            concSlider.gameObject.SetActive(false);
        }
        else if (presets[curid].QteType == SLM_QTEEvents_Preset.qtetpe.reaction)
        {
            buttons[presets[curid].buttonReaction].obj.SetActive(false);
            concSlider.gameObject.SetActive(false);
        }


        curid = -1;
        stage = 0;
        curcount = 1;
        foreach (SLM_QTEEvents_Button b in buttons)
        {
            b.obj.SetActive(false);
        }

        if (!lose)
        {
            if (comm!=null)
			{
                SLM_Commands com = comm;
                comm = null;
                com.RunCommand(winC);
            }
            else
			{
                winE.Invoke();
            }
        }
        else
		{
            if (comm != null)
            {
                SLM_Commands com = comm;
                comm = null;
                com.RunCommand(loseC);
            }
            else
            {
                loseE.Invoke();
            }
        }
    }

    public void Press()
	{
        if (stage== curcount)
            curcount++;
	}
}

[System.Serializable]
public class SLM_QTEEvents_Button
{
    public bool useInput;
    public string name;
    [ShowFromBool("useInput", false)]
    public KeyCode key;
    public GameObject obj;
    [HideInInspector] public bool pressed;
    public bool notInRandom;
}

[System.Serializable]
public class SLM_QTEEvents_Preset
{
    public string name;
    public enum qtetpe { basic, find, fast, concentrate, reaction };
    public qtetpe QteType;
    [ShowFromMultiple("QteType", new string[4] { "0", "1", "2", "3" }, "enum", ShowFromMultipleAttribute.mode.or)]
    public float time = 1;
    [ShowFromMultiple("QteType", new string[4] { "0", "1", "2", "3" }, "enum", ShowFromMultipleAttribute.mode.or)]
    public bool disableTimerVis;
    [ShowFromMultiple("QteType", new string[4] { "0", "1", "2", "4" }, "enum", ShowFromMultipleAttribute.mode.or)]
    public bool disableProgressVis;
    [ShowFromMultiple("QteType", new string[2] { "2", "0" }, "enum", ShowFromMultipleAttribute.mode.or)]
    public bool useColor;
    [ShowFromBool("useColor")]
    public Color ColorPressed = Color.green;
    [ShowFromBool("useColor")]
    public Color NonPressed = Color.white;

    //basic
    //classic qte - press buttons on the screen
    [ShowFromEnum("QteType", 0)]
    public List<string> buttonIds; // 1|2|3
    [ShowFromEnum("QteType", 0)]
    public List<int> buttonIdsTest; // 1|2|3
    [ShowFromEnum("QteType", 0)]
    public List<tpe> mode;
    public enum tpe { savePress, withhold, pressTogether };
    public enum tpemiss { none, fromstart, oneback, fail }
    [ShowFromEnum("QteType", 0)]
    public tpemiss missing;

    //find
    //you need to find button on the screen
    [ShowFromEnum("QteType", 1)]
    public int clickCount;

    //fast
    //you need to fast press buttons as fast as you can
    [ShowFromMultiple("QteType", new string[2] { "2", "3" }, "enum", ShowFromMultipleAttribute.mode.or)]
    public int buttonPossitive;
    [ShowFromMultiple("QteType", new string[2] { "2", "3" }, "enum", ShowFromMultipleAttribute.mode.or)]
    public int buttonNegative;
    [ShowFromEnum("QteType", 2)]
    [Range(0f, 1f)] public float addCount;
    [ShowFromEnum("QteType", 2)]
    [Range(0f, 1f)] public float removePerSec;

    [ShowFromMultiple("QteType", new string[2] { "3", "4" }, "enum", ShowFromMultipleAttribute.mode.or)]
    public float headerSpeed;
    [ShowFromEnum("QteType", 3)]
    public float pressSpeed;

    //concentrate
    //you need to place header on safe zone
    [ShowFromEnum("QteType", 3)]
    [Range(0f, 1f)] public float safeSize = 0.1f;
    [ShowFromEnum("QteType", 3)]
    [Min(0)] public float rndTimeMin;
    [ShowFromEnum("QteType", 3)]
    [Min(0.01f)] public float rndTimeMax;
    [ShowFromEnum("QteType", 3)]
    [Range(0, 100)] public int ChanceToChangeDir;
    [ShowFromEnum("QteType", 3)]
    public Color safeAreaColorConc = Color.green;
    [ShowFromEnum("QteType", 3)]
    public Color failAreaColorConc = Color.red;
    [ShowFromEnum("QteType", 3)]
    public Color HeaderColorConc = Color.yellow;

    //reaction
    //you need to press on safe zone before end
    [ShowFromEnum("QteType", 4)]
    public int buttonReaction;
    [ShowFromEnum("QteType", 4)]
    [Range(0f, 1f)] public float reactionMinPos;
    [ShowFromEnum("QteType", 4)]
    [Range(0f, 1f)] public float reactionMaxPos;
    [ShowFromEnum("QteType", 4)]
    [Range(0.01f, 0.98f)] public float reactionSafeZone;
    public enum reactFailMode { nothing, fail};
    [ShowFromEnum("QteType", 4)]
    public reactFailMode reactionFailMode;

    [ShowFromEnum("QteType", 4)]
    [Min(1)]public int gamesCount=1;
    [ShowFromEnum("QteType", 4)]
    public int lifes = 3;
    [ShowFromEnum("QteType", 4)]
    public Color safeAreaColorReact = Color.cyan;
    [ShowFromEnum("QteType", 4)]
    public Color failAreaColorReact = Color.yellow;
    [ShowFromEnum("QteType", 4)]
    public Color HeaderColorReact = Color.green;
}


#if UNITY_EDITOR
/*
[CustomPropertyDrawer(typeof(SLM_QTEEvents_Preset))]
public class SLM_QTEEvents_Preset_Editor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var container = new VisualElement();

        var popup = new UnityEngine.UIElements.PopupWindow();
        popup.Add(new PropertyField(property.FindPropertyRelative("name"), "Name"));
        popup.Add(new PropertyField(property.FindPropertyRelative("QteType"), "QTE Type"));
        container.Add(popup);

        return container;
    }
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{

		base.OnGUI(position, property, label);
	}
}*/
#endif