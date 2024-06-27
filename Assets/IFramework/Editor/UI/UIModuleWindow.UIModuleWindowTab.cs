﻿/*********************************************************************************
 *Author:         OnClick
 *Version:        0.0.1
 *UnityVersion:   2018.3.11f1
 *Date:           2020-01-13
 *Description:    IFramework
 *History:        2018.11--
*********************************************************************************/

namespace IFramework.UI
{
    public partial class UIModuleWindow
    {
        public abstract class UIModuleWindowTab
        {
            public abstract string name { get; }
            public abstract void OnGUI();
            public virtual void OnEnable() { }
            public virtual void OnDisable() { }
            public virtual void OnHierarchyChanged() { }

            public virtual string GetPanelScriptName(string name)
            {
                return string.Empty;
            }
        }
    }
}
