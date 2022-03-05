using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLywnow;
using System.IO;

[System.Serializable]
public class SLM_Container
{
	public string mainpath; //<dp> - datapath <docs> -  Documents
	public string currentproject;
    public List<SLM_Container_Block> blocks;

	public enum tpe { text, textoneline, texture, sprite };

	public void LoadBlock(int id)
	{
		string newpath = mainpath.Replace("<dp>", Application.dataPath);
		newpath = newpath.Replace("<docs>", FastFind.GetDefaultPath());
		string path = newpath + "/" + currentproject + "/"+ blocks[id].path;
		if (FilesSet.CheckDirectory(path))
		{
			if (blocks[id].singlefile)
			{
				if (FilesSet.CheckFile(path + "/" + blocks[id].single.filename)) 
				{
					if (blocks[id].single.type == tpe.text)
						blocks[id].single.containertext = FilesSet.LoadStream(path + "/" + blocks[id].single.filename, false);
					if (blocks[id].single.type == tpe.textoneline)
						blocks[id].single.containertextoneline = FilesSet.LoadStream(path + "/" + blocks[id].single.filename, false, true);
					if (blocks[id].single.type == tpe.texture)
					{
						FileStream streamimg = File.Open(path + "/" + blocks[id].single.filename, FileMode.Open);
						byte[] imgbt = new byte[streamimg.Length];
						streamimg.Read(imgbt, 0, imgbt.Length);
						streamimg.Close();
						blocks[id].single.containertexture = new Texture2D(0, 0);
						blocks[id].single.containertexture.LoadImage(imgbt);
					}
					if (blocks[id].single.type == tpe.sprite)
					{
						FileStream streamimg = File.Open(path + "/" + blocks[id].single.filename, FileMode.Open);
						byte[] imgbt = new byte[streamimg.Length];
						streamimg.Read(imgbt, 0, imgbt.Length);
						streamimg.Close();
						Texture2D imgtex = new Texture2D(0, 0);
						imgtex.LoadImage(imgbt);

						blocks[id].single.containersprite = Sprite.Create(imgtex, new Rect(0.0f, 0.0f, imgtex.width, imgtex.height), new Vector2(0.5f, 0.5f));
					}
				} else 
					Debug.LogError("File " + blocks[id].single.filename + " not found!");
			}
			else
			{
				string[] files = FilesSet.GetFilesFromdirectories(path, "", false, FilesSet.TypeOfGet.NamesOfFilesWithFormat);
				if (blocks[id].multi.type == tpe.text)
					blocks[id].multi.containertext.Clear();
				if (blocks[id].multi.type == tpe.textoneline)
					blocks[id].multi.containertextoneline.Clear();
				if (blocks[id].multi.type == tpe.texture)
					blocks[id].multi.containertexture.Clear();
				if (blocks[id].multi.type == tpe.sprite)
					blocks[id].multi.containersprite.Clear();

					for (int a = 0; a < files.Length; a++)
				{
					if (files[a].IndexOf(blocks[id].multi.filehasname) >= 0)
					{
						if (blocks[id].multi.type == tpe.text)
							blocks[id].multi.containertext.Add(FilesSet.LoadStream(path + "/" + files[a], false));
						if (blocks[id].multi.type == tpe.textoneline)
							blocks[id].multi.containertextoneline.Add(FilesSet.LoadStream(path + "/" + files[a], false, true));
						if (blocks[id].multi.type == tpe.texture)
						{
							FileStream streamimg = File.Open(path + "/" + files[a], FileMode.Open);
							byte[] imgbt = new byte[streamimg.Length];
							streamimg.Read(imgbt, 0, imgbt.Length);
							streamimg.Close();
							blocks[id].multi.containertexture.Add(new Texture2D(0, 0));
							blocks[id].multi.containertexture[blocks[id].multi.containertexture.Count-1].LoadImage(imgbt);
						}
						if (blocks[id].multi.type == tpe.sprite)
						{
							FileStream streamimg = File.Open(path + "/" + files[a], FileMode.Open);
							byte[] imgbt = new byte[streamimg.Length];
							streamimg.Read(imgbt, 0, imgbt.Length);
							streamimg.Close();
							Texture2D imgtex = new Texture2D(0, 0);
							imgtex.LoadImage(imgbt);

							blocks[id].multi.containersprite.Add(Sprite.Create(imgtex, new Rect(0.0f, 0.0f, imgtex.width, imgtex.height), new Vector2(0.5f, 0.5f)));
						}
					}
				}
			}
		}
		else
			Debug.LogError("Directory "+ path + " not found!");
	}

	public void LoadAllBlocks()
	{
		for (int i=0;i<blocks.Count;i++)
		{
			LoadBlock(i);
		}
	}

	public void ClearBlock(int id)
	{
		blocks[id].single.containertext = new string[0];
		blocks[id].single.containertextoneline = "";
		blocks[id].single.containertexture = null;
		blocks[id].single.containersprite = null;

		blocks[id].multi.containertext.Clear();
		blocks[id].multi.containertextoneline.Clear();
		blocks[id].multi.containertexture.Clear();
		blocks[id].multi.containersprite.Clear();
	}

	public void ClearAllBlocks()
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			ClearBlock(i);
		}
	}
}

[System.Serializable]
public class SLM_Container_Block
{
	public string path;
	public bool singlefile;
	[ShowFromBool("singlefile",true)]
	public SLM_Container_Block_Single single;
	[ShowFromBool("singlefile", false)]
	public SLM_Container_Block_Multi multi;
}

[System.Serializable]
public class SLM_Container_Block_Single
{
	public string filename;
	public SLM_Container.tpe type;
	public string[] containertext;
	public string containertextoneline;
	public Texture2D containertexture;
	public Sprite containersprite;
}

[System.Serializable]
public class SLM_Container_Block_Multi
{
	/// <summary>
	/// You can add peace of name or format;
	/// Set "." to all;
	/// Example: "png" "file_" "."
	/// </summary>
	public string filehasname;
	public SLM_Container.tpe type;
	public List<string[]> containertext;
	public List<string> containertextoneline;
	public List<Texture2D> containertexture;
	public List<Sprite> containersprite;
}