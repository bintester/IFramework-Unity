using IFramework;
using IFramework.Localization;
using UnityEngine;

public class LocalizationGame : Game, ILocalizationPrefRecorder
{
    public LocalizationData data;
    public override void Init()
    {
        Localization.SetContext(data);
        Localization.SetRecorder(this);
    }

    public override void Startup()
    {

    }
    private void OnGUI()
    {
        if (GUILayout.Button("open WIndow", new GUIStyle("button") { fontSize = 40 }, GUILayout.Height(300), GUILayout.Width(300)))
        {
            IFramework.EditorTools.EditorWindowTool.Create("Localization");

        }
        var types = Localization.GetLocalizationTypes();
        var type = Localization.localizationType;
        var index = GUILayout.Toolbar(types.IndexOf(type), types.ToArray(),new GUIStyle(GUI.skin.button) { fontSize = 40 }, GUILayout.Height(100), GUILayout.Width(300));
        Localization.SetLocalizationType(types[index]);
    }

    LocalizationPref ILocalizationPrefRecorder.Read()
    {
        Log.L("Read");
  
        return null;
    }

    void ILocalizationPrefRecorder.Write(LocalizationPref pref)
    {
        Log.L("Write");
    }
}
