using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SLywnow;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SLM_QTE_Button : EventTrigger
{
	public SLM_QTEEvents qte;
	public string key;
	public bool onlyMobile;
	public bool canHold = true;
	[Min(1)] public int frameSaveClick = 1;

	int frame;
	bool clicked;
	bool active;

	void Start ()
	{
		active = true;
#if UNITY_STANDALONE
		if (onlyMobile)
			active = false;
#endif
	}

	public void Update ()
	{
		if (clicked)
		{
			if (frame < frameSaveClick)
				frame++;
			else
			{
				frame = 0;
				clicked = false;
				if (qte.pressed.Contains(key))
					qte.pressed.Remove(key);
			}
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (active && !canHold)
		{
			frame = 0;
			clicked = true;
			if (!qte.pressed.Contains(key))
				qte.pressed.Add(key);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (active && canHold)
		{
			if (!qte.pressed.Contains(key))
				qte.pressed.Add(key);
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		if (active && canHold)
		{
			frame = 0;
			clicked = true;
		}
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(SLM_QTE_Button))]
[CanEditMultipleObjects]
public class SLM_QTE_Button_GUI : Editor
{
	SerializedProperty qte;
	SerializedProperty key;
	SerializedProperty onlyMobile;
	SerializedProperty canHold;
	SerializedProperty frameSaveClick;

	void OnEnable()
	{
		qte = serializedObject.FindProperty("qte");
		key = serializedObject.FindProperty("key");
		onlyMobile = serializedObject.FindProperty("onlyMobile");
		canHold = serializedObject.FindProperty("canHold");
		frameSaveClick = serializedObject.FindProperty("frameSaveClick");
	}

	public override void OnInspectorGUI()
	{
		SLM_QTE_Button tg = (SLM_QTE_Button)target;

		EditorGUILayout.PropertyField(qte);
		EditorGUILayout.PropertyField(key);
		EditorGUILayout.PropertyField(onlyMobile);
		EditorGUILayout.PropertyField(canHold);
		EditorGUILayout.PropertyField(frameSaveClick);

		serializedObject.ApplyModifiedProperties();
	}
}
#endif