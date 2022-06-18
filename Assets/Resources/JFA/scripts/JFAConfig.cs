using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

[Serializable]
public class JFAConfig
{
    public static string SaveFileName = "Result_{0:00}";
    public static string SavePath => $"{Application.dataPath}/JFA/Results";
    [SerializeField] private int maxPasses;
    [SerializeField] private bool forceMaxPasses;
    [SerializeField] private bool recordProcess;
    [SerializeField] private GraphicsFormat format;

    public int MaxPasses
    {
        get => maxPasses;
        set => maxPasses = value;
    }

    public bool ForceMaxPasses
    {
        get => forceMaxPasses;
        set => forceMaxPasses = value;
    }

    public bool RecordProcess
    {
        get => recordProcess;
        set => recordProcess = value;
    }

    public GraphicsFormat Format
    {
        get => format;
        set => format = value;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(JFAConfig))]
public class JFAConfigEditor : Editor
{
    SerializedProperty SupportedGraphicsFormats;

    void OnEnable()
    {
        SupportedGraphicsFormats = serializedObject.FindProperty("looformatkAtPoint");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        serializedObject.Update();
        EditorGUILayout.PropertyField(SupportedGraphicsFormats);
        if (SystemInfo.IsFormatSupported((GraphicsFormat) SupportedGraphicsFormats.enumValueIndex, FormatUsage.Render))
        {
            EditorGUILayout.LabelField("(Supported!)");
        }
        else
        {
            EditorGUILayout.LabelField("(Not supported :( )");
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif