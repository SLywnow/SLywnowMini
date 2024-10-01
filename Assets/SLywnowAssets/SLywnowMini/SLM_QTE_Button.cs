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

	bool active;

	void Start ()
	{
		if (qte == null)
			qte = FindObjectOfType<SLM_QTEEvents>();

		active = true;
#if !UNITY_ANDROID
		if (onlyMobile)
			active = false;
#endif
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (active)
		{
			qte.MobilePress(key);
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		if (active)
		{
			qte.MobilePressDone(key);
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