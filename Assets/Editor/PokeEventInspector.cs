using Oculus.Interaction;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(InteractableUnityEventWrapper))]
public class PokeEventInspector : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        var testSpeed = new FloatField("テスト球速（km/h）") { value = 100f };
        if (FindNormalListeners().Count > 0) root.Add(testSpeed);

        root.Add(new HelpBox(
            "Play Modeでボタンを押すと、Poke押下時のWhen Selectイベントを実行します。通常／最大モードはこのオブジェクトのイベント設定に従います。",
            HelpBoxMessageType.Info));

        var press = new Button(() =>
        {
            var wrapper = target as InteractableUnityEventWrapper;
            if (!EditorApplication.isPlaying || EditorApplication.isPaused
                || wrapper == null || !wrapper.isActiveAndEnabled) return;
            serializedObject.ApplyModifiedProperties();
            float speed = testSpeed.value;
            if (float.IsNaN(speed) || float.IsInfinity(speed) || speed <= 0f)
            {
                Debug.LogWarning("テスト球速には0より大きい有限の数値を指定してください。", wrapper);
                return;
            }
            var listeners = FindNormalListeners();
            var previous = new List<float?>();
            foreach (var listener in listeners)
            {
                previous.Add(listener.InspectorNormalSpeedKmh);
                listener.InspectorNormalSpeedKmh = speed;
            }
            try { wrapper.WhenSelect?.Invoke(); }
            finally
            {
                for (int i = 0; i < listeners.Count; i++)
                    if (listeners[i] != null) listeners[i].InspectorNormalSpeedKmh = previous[i];
            }
        }) { text = "Poke押下を実行" };
        press.style.marginTop = 6;
        root.Add(press);

        void UpdateButton()
        {
            var wrapper = target as InteractableUnityEventWrapper;
            press.SetEnabled(EditorApplication.isPlaying && !EditorApplication.isPaused
                && wrapper != null && wrapper.isActiveAndEnabled);
        }
        UpdateButton();
        root.schedule.Execute(UpdateButton).Every(100);
        return root;
    }

    List<DisplaySpeed> FindNormalListeners()
    {
        var result = new List<DisplaySpeed>();
        var wrapper = target as InteractableUnityEventWrapper;
        if (wrapper == null || (wrapper.gameObject.scene.name != "DevelopScene"
            && wrapper.gameObject.scene.name != "MainScene")) return result;
        serializedObject.Update();
        var calls = serializedObject.FindProperty("_whenSelect.m_PersistentCalls.m_Calls");
        if (calls == null) return result;
        for (int i = 0; i < calls.arraySize; i++)
        {
            var call = calls.GetArrayElementAtIndex(i);
            var display = call.FindPropertyRelative("m_Target").objectReferenceValue as DisplaySpeed;
            if (display != null && call.FindPropertyRelative("m_MethodName").stringValue == "ReadyText"
                && !call.FindPropertyRelative("m_Arguments.m_BoolArgument").boolValue
                && !result.Contains(display)) result.Add(display);
        }
        return result;
    }
}

