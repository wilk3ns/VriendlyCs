using System.Collections.Generic;
using System.Reflection;
using UnityEditor.AnimatedValues;
using UnityEngine.Events;
using UnityEngine;
using UnityEditor;

public class FoldoutState
{
    public bool show = false;
    public FoldoutState(bool c) { show = c; }
}

[CustomEditor(typeof(Vriendly_RoomServer_Experimental))]
public class Vriendly_RoomServer_Experimental_Inspector : Editor
{

    AnimBool anim;
    Vriendly_RoomServer_Experimental room;

    List<FoldoutState> FoldoutStateList = new List<FoldoutState>();

    void OnEnable()
    {
        room = (Vriendly_RoomServer_Experimental)target;

        anim = new AnimBool(false);
        anim.valueChanged.AddListener(new UnityAction(base.Repaint));

        if (room._roomProfile)
        {
            for (int i = 0; i < HasProperties(room._roomProfile); i++)
            {
                FoldoutStateList.Add(new FoldoutState(false));
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        anim.target = room._roomProfile ? true : false;
        using (var group = new EditorGUILayout.FadeGroupScope(anim.faded))
        {
            if (group.visible)
            {
                if (room._roomProfile)
                {
                    Editor propertyModule = CreateEditor(room._roomProfile);

                    EditorGUILayout.Space();

                    propertyModule.DrawDefaultInspector();

                    EditorGUILayout.Space();

                    int i = 0;
                    FieldInfo[] props = GetProperties(room._roomProfile);
                    if (props.Length > 0)
                    {
                        foreach (FieldInfo inf in props)
                        {
                            Object obj = GetObjectFromProperty(room._roomProfile, inf);
                            if (obj && HasProperties(obj) > 1) DrawModule(obj, FoldoutStateList[i]);
                            i++;
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();

    }

    private Object GetObjectFromProperty(Object propertyHolder, FieldInfo inf)
    {
        return new SerializedObject(propertyHolder).FindProperty(inf.Name).objectReferenceValue;
    }

    private FieldInfo[] GetProperties(Object obj)
    {
        return obj.GetType().GetFields(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance);
    }

    private int HasProperties(Object obj)
    {
        return obj.GetType().GetFields(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance).Length;
    }

    private void DrawModule(Object obj, FoldoutState foldoutState)
    {
        if (obj)
        {
            foldoutState.show = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutState.show, obj.name+" module");
            if (foldoutState.show)
            {
                Editor otherObject = CreateEditor(obj);

                otherObject.DrawDefaultInspector();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}

