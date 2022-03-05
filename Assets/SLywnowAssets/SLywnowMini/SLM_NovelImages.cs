using SLywnow;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SLM_NovelImages : MonoBehaviour
{
	public List<SLM_NovelImages_Block> layers;

	[HideInInspector] public int lastimage;

	List<int> anims=new List<int>();

/*	[Header("AutoSet")]
	public string path;
	public string contain;
	public string format;
	public int layerId;
	public bool add;
	[Button("AutoSet Images")]
	public void AutoSetImages()
	{
		bool can = false;
#if UNITY_EDITOR
		can = true;
#endif
		List<string> paths = FilesSet.GetFilesFromdirectories(path, format, false, FilesSet.TypeOfGet.NamesOfFilesWithFormat).ToList();

	}*/

	public void Update()
	{
		if (anims.Count > 0)
		{
			try
			{
				foreach (int a in anims)
				{
					layers[a].timer += Time.deltaTime;
					if (!layers[a].showLayer && !layers[a].hideLayer)
					{
						if (layers[a].animation == SLM_NovelImages_Block.animtpe.replace)
						{

							layers[a].imageMask.color =
								new Color(1, 1, 1, 1 - (layers[a].timer / layers[a].durationInSec));

							if (layers[a].timer / layers[a].durationInSec >= 1)
							{
								layers[a].timer = 0;
								layers[a].animrun = false;
								layers[a].imageMask.color = new Color(0, 0, 0, 0);

								layers[a].onEndAnimation.Invoke();
								anims.RemoveAt(anims.IndexOf(a));
							}
						}
						if (layers[a].animation == SLM_NovelImages_Block.animtpe.pingpong)
						{
							layers[a].imageobj.color =
								new Color(1, 1, 1, ((layers[a].timer / layers[a].durationInSec) - 0.5f) * 2);

							layers[a].imageMask.color =
								new Color(1, 1, 1, (0.5f - (layers[a].timer / layers[a].durationInSec)) * 2);

							if (layers[a].timer / layers[a].durationInSec >= 1)
							{
								layers[a].timer = 0;
								layers[a].animrun = false;
								layers[a].imageobj.color = new Color(1, 1, 1, 1);
								layers[a].imageMask.color = new Color(0, 0, 0, 0);

								layers[a].onEndAnimation.Invoke();
								anims.RemoveAt(anims.IndexOf(a));
							}
						}
					}
					else
					{
						if (layers[a].hideLayer)
						{
							if (layers[a].imageobj !=null)
							layers[a].imageobj.color =
								new Color(1, 1, 1, 1 - (layers[a].timer / layers[a].durationInSec));
							if (layers[a].group != null)
								layers[a].group.alpha = 1 - (layers[a].timer / layers[a].durationInSec);

							if (layers[a].timer / layers[a].durationInSec >= 1)
							{
								layers[a].timer = 0;
								layers[a].animrun = false;
								layers[a].hideLayer = false;
								if (layers[a].imageobj != null)
									layers[a].imageobj.color = new Color(0, 0, 0, 0);
								if (layers[a].group != null)
								{
									layers[a].group.alpha = 0;
									layers[a].group.blocksRaycasts = false;
								}

								layers[a].onEndAnimation.Invoke();
								anims.RemoveAt(anims.IndexOf(a));
							}
						}
						if (layers[a].showLayer)
						{
							if (layers[a].imageobj != null)
								layers[a].imageobj.color =
								new Color(1, 1, 1, (layers[a].timer / layers[a].durationInSec));
							if (layers[a].group != null)
								layers[a].group.alpha = (layers[a].timer / layers[a].durationInSec);

							if (layers[a].timer / layers[a].durationInSec >= 1)
							{
								layers[a].timer = 0;
								layers[a].animrun = false;
								layers[a].showLayer = false;
								if (layers[a].imageobj != null)
									layers[a].imageobj.color = new Color(1, 1, 1, 1);
								if (layers[a].group != null)
								{
									layers[a].group.alpha = 1;
									layers[a].group.blocksRaycasts = true;
								}

								layers[a].onEndAnimation.Invoke();
								anims.RemoveAt(anims.IndexOf(a));
							}
						}
					}
				}
			} catch (System.Exception ex)
			{
				//Debug.Log(ex);
			}
		}
	}

	public void Awake()
	{
		foreach (SLM_NovelImages_Block l in layers)
		{
			if (!l.showAtStart)
			{
				l.ImageColor = new Color(1, 1, 1, 1);
				if (l.imageobj != null)
					l.imageobj.color = new Color(0, 0, 0, 0);
				if (l.group != null)
				{
					l.group.alpha = 0;
					l.group.blocksRaycasts = false;
				}
			}
			else
			{
				l.ImageColor = new Color(1, 1, 1, 1);
				if (l.imageobj != null)
					l.imageobj.color = new Color(1, 1, 1, 1);
				if (l.group != null)
				{
					l.group.alpha = 1;
					l.group.blocksRaycasts = true;
				}
			}
			if (l.animation !=SLM_NovelImages_Block.animtpe.none && l.imageMask !=null)
				l.imageMask.color = new Color(0, 0, 0, 0);
		}
	}

	public SLM_NovelImages_Block GetLayer(string layerName)
	{
		SLM_NovelImages_Block ret = layers.Find(f => f.name == layerName);
		return ret;
	}

	public void SetImage(string layerName, int id)
	{
		
		if (layers.Find(f => f.name == layerName) !=null)
		{
			int layer = layers.IndexOf(layers.Find(f => f.name == layerName));
			if (layers[layer].imageobj != null)
			{
				layers[layer].imageobj.color = layers[layer].ImageColor;
				layers[layer].curid = id;
				lastimage = id;
				if (layers[layer].animation != SLM_NovelImages_Block.animtpe.none)
				{

					if (!layers[layer].animrun)
						anims.Add(layer);

					layers[layer].imageMask.sprite = layers[layer].imageobj.sprite;
					layers[layer].imageMask.color = new Color(1, 1, 1, 1);
					layers[layer].animrun = true;
					layers[layer].timer = 0;
				}
				else
					layers[layer].onEndAnimation.Invoke();

				layers[layer].imageobj.sprite = layers[layer].sprites[id];
			}
		}
	}

	public void SetImage(string layerName, string spriteName)
	{
		if (layers.Find(f => f.name == layerName) != null)
		{
			int layer = layers.IndexOf(layers.Find(f => f.name == layerName));
			if (layers[layer].imageobj != null)
			{
				layers[layer].imageobj.color = layers[layer].ImageColor;
				if ((spriteName != "<next>") && (spriteName != "<prev>"))
				{
					layers[layer].curid = layers[layer].sprites.IndexOf(layers[layer].sprites.Find(f => f.name == spriteName));
					//layers[layer].curid = layers[layer].spriteNames.IndexOf(spriteName);
				}
				else
				{
					if (spriteName == "<next>")
					{
						layers[layer].curid += 1;
					}
					if (spriteName == "<prev>")
					{
						layers[layer].curid -= 1;
					}
				}
				if (layers[layer].animation != SLM_NovelImages_Block.animtpe.none)
				{
					if (!layers[layer].animrun)
						anims.Add(layer);
					layers[layer].imageMask.sprite = layers[layer].imageobj.sprite;
					layers[layer].imageMask.color = new Color(1, 1, 1, 1);
					layers[layer].animrun = true;
					layers[layer].timer = 0;
				}
				else
					layers[layer].onEndAnimation.Invoke();

				lastimage = layers[layer].curid;

				if ((spriteName != "<next>") && (spriteName != "<prev>"))
				{
					//layers[layer].imageobj.sprite = layers[layer].sprites[layers[layer].spriteNames.IndexOf(spriteName)];
					layers[layer].imageobj.sprite = layers[layer].sprites.Find(f => f.name == spriteName);
				}
				else
				{
					layers[layer].imageobj.sprite = layers[layer].sprites[layers[layer].curid];
				}
			}
		}
	}

	public void DisableLayer(string layerName)
	{
		if (layers.Find(f => f.name == layerName) != null)
		{
			int layer = layers.IndexOf(layers.Find(f => f.name == layerName));

			if ((layers[layer].imageobj != null && layers[layer].imageobj.color != new Color(0, 0, 0, 0)) || (layers[layer].group != null && layers[layer].group.alpha != 0))
			{
				if (layers[layer].imageobj != null)
					layers[layer].ImageColor = layers[layer].imageobj.color;

				if (layers[layer].animation != SLM_NovelImages_Block.animtpe.none)
				{
					if (layers[layer].imageMask != null)
						layers[layer].imageMask.color = new Color(0, 0, 0, 0);
					if (layers[layer].group != null)
					{
						layers[layer].group.alpha = 0;
						layers[layer].group.blocksRaycasts = false;
					}

					if (!layers[layer].animrun)
						anims.Add(layer);
					if (!layers[layer].hideLayer)
						layers[layer].timer = 0;
					layers[layer].hideLayer = true;
					layers[layer].showLayer = false;
					layers[layer].animrun = true;
				}
				else
				{
					if (layers[layer].imageobj != null)
						layers[layer].imageobj.color = new Color(0, 0, 0, 0);
					if (layers[layer].group != null)
					{
						layers[layer].group.alpha = 0;
						layers[layer].group.blocksRaycasts = false;
					}

					layers[layer].onEndAnimation.Invoke();
				}
			}
		}
	}

	public void ShowLayer(string layerName, int imageid=-1)
	{
		if (layers.Find(f => f.name == layerName) != null)
		{
			int layer = layers.IndexOf(layers.Find(f => f.name == layerName));
			if ((layers[layer].imageobj != null && layers[layer].imageobj.color != new Color(1, 1, 1, 1)) || (layers[layer].group != null && layers[layer].group.alpha != 1))
			{
				if (imageid >= 0)
				{
					layers[layer].imageobj.sprite = layers[layer].sprites[imageid];
					layers[layer].curid = imageid;
					lastimage = layers[layer].curid;
				}

				if (layers[layer].animation != SLM_NovelImages_Block.animtpe.none)
				{
					if (layers[layer].imageMask != null)
						layers[layer].imageMask.color = new Color(0, 0, 0, 0);
					if (layers[layer].imageobj != null)
						layers[layer].imageobj.color = new Color(layers[layer].ImageColor.r, layers[layer].ImageColor.g, layers[layer].ImageColor.b, 0);
					if (layers[layer].group != null)
					{
						layers[layer].group.alpha = 0;
						layers[layer].group.blocksRaycasts = true;
					}

					if (!layers[layer].animrun)
						anims.Add(layer);
					if (!layers[layer].showLayer)
						layers[layer].timer = 0;
					layers[layer].showLayer = true;
					layers[layer].hideLayer = false;
					layers[layer].animrun = true;
				}
				else
				{
					if (layers[layer].imageobj != null)
						layers[layer].imageobj.color = layers[layer].ImageColor;
					if (layers[layer].group != null)
					{
						layers[layer].group.alpha = 1;
						layers[layer].group.blocksRaycasts = true;
					}

					layers[layer].onEndAnimation.Invoke();
				}
			}
			else if (imageid >= 0)
				SetImage(layerName, imageid);
		}
	}

	public void ShowLayer(string layerName, string spriteName = "")
	{
		if (layers.Find(f => f.name == layerName) != null)
		{
			int layer = layers.IndexOf(layers.Find(f => f.name == layerName));
			if ((layers[layer].imageobj != null && layers[layer].imageobj.color != new Color(1, 1, 1, 1)) || (layers[layer].group != null && layers[layer].group.alpha != 1))
			{
				if (!string.IsNullOrEmpty(spriteName) && !((spriteName =="<next>") || (spriteName == "<prev>")))
				{
					if (layers[layer].imageobj != null)
						layers[layer].imageobj.sprite = layers[layer].sprites.Find(f => f.name == spriteName);
					layers[layer].curid = layers[layer].sprites.IndexOf(layers[layer].sprites.Find(f => f.name == spriteName));
				}
				else
				{
					if (spriteName == "<next>")
					{
						layers[layer].curid += 1;
						if (layers[layer].imageobj != null)
							layers[layer].imageobj.sprite = layers[layer].sprites[layers[layer].curid];
					}
					if (spriteName == "<prev>")
					{
						layers[layer].curid -= 1;
						if (layers[layer].imageobj != null)
							layers[layer].imageobj.sprite = layers[layer].sprites[layers[layer].curid];
					}
				}
				lastimage = layers[layer].curid;

				if (layers[layer].animation != SLM_NovelImages_Block.animtpe.none)
				{
					if (layers[layer].imageMask != null)
						layers[layer].imageMask.color = new Color(0, 0, 0, 0);
					if (layers[layer].imageobj != null)
						layers[layer].imageobj.color = new Color(layers[layer].ImageColor.r, layers[layer].ImageColor.g, layers[layer].ImageColor.b, 0);
					if (layers[layer].group != null)
					{
						layers[layer].group.alpha = 0;
						layers[layer].group.blocksRaycasts = true;
					}
					if (!layers[layer].animrun)
						anims.Add(layer);
					if (!layers[layer].showLayer)
						layers[layer].timer = 0;
					layers[layer].showLayer = true;
					layers[layer].hideLayer = false;
					layers[layer].animrun = true;
				}
				else
				{
					if (layers[layer].imageobj != null)
						layers[layer].imageobj.color = layers[layer].ImageColor;
					if (layers[layer].group != null)
					{
						layers[layer].group.alpha = 1;
						layers[layer].group.blocksRaycasts = true;
					}

					layers[layer].onEndAnimation.Invoke();
				}
			}
			else if (!string.IsNullOrEmpty(spriteName))
				SetImage(layerName, spriteName);
		}
	}

	public void SetAnim(string layerName, int type, float time)
	{
		if (layers.Find(f => f.name == layerName) != null)
		{
			int layer = layers.IndexOf(layers.Find(f => f.name == layerName));
			switch (type)
			{
				case 0:
					{
						layers[layer].animation = SLM_NovelImages_Block.animtpe.none;
						if (time > 0)
							layers[layer].durationInSec = time;
						break;
					}
				case 1:
					{
						layers[layer].animation = SLM_NovelImages_Block.animtpe.replace;
						if (time > 0)
							layers[layer].durationInSec = time;
						break;
					}
				case 2:
					{
						layers[layer].animation = SLM_NovelImages_Block.animtpe.pingpong;
						if (time > 0)
							layers[layer].durationInSec = time;
						break;
					}
				default:
					{
						layers[layer].durationInSec = time;
						break;
					}
			}
		}
	}

	public void SetColor(string layerName, string color)
	{
		if (layers.Find(f => f.name == layerName) != null)
		{
			int layer = layers.IndexOf(layers.Find(f => f.name == layerName));
			Color c = Color.white;
			if (ColorUtility.TryParseHtmlString(color, out c)) ;

			layers[layer].imageobj.color = c;
		}
	}
}

[System.Serializable]
public class SLM_NovelImages_Block
{
	public string name;
	public bool showAtStart = true;
	public CanvasGroup group;
	public Image imageobj;
	[HideInInspector] public Color ImageColor;
	[HideInInspector] public bool animrun;
	[HideInInspector] public float timer;
	[HideInInspector] public bool showLayer;
	[HideInInspector] public bool hideLayer;
	public enum animtpe { none, replace, pingpong};
	public animtpe animation;
	public Image imageMask;
	public float durationInSec;
	public List<Sprite> sprites;
	[HideInInspector] public int curid;
	[HideInInspector] public UnityEvent onEndAnimation;
}