﻿/*********************************************************************************
 *Author:         OnClick
 *Version:        0.0.1
 *UnityVersion:   2018.3.11f1
 *Date:           2020-01-13
 *Description:    IFramework
 *History:        2018.11--
*********************************************************************************/
using UnityEditor;
using IFramework;
using UnityEngine;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using IFramework.UI;
using UnityEditor.IMGUI.Controls;

namespace IFramework.Hotfix.Lua
{
    static partial class LuaEditorTools
    {
        [Serializable]

        class Gen_Item_Code_lua : UIMoudleWindow.UIMoudleWindowTab
        {
            public enum ItemType
            {
                UIItem,
                LuaObject,
                UIObject,
            }
            private const string key = "Gen_Item_Code_lua";
            public override string name => "Lua/Gen_Item_Code_lua";
            [SerializeField] private string workFolder;
            [SerializeField] private GameObject panel;
            [SerializeField] private ItemType _type;
            [SerializeField] private TreeViewState state = new TreeViewState();

            private ScriptCreater creater = new ScriptCreater();
            private ScriptCreaterFieldsDrawer fields;
            private LuaFloderField FloderField;
            private string luaPath
            {
                get
                {
                    if (panel == null)
                    {
                        return null;
                    }
                    return workFolder.CombinePath($"{panelName}.lua.txt");
                }
            }
            private string panelName => panel == null ? "" : panel.name;

            public override void OnEnable()
            {
                var last = EditorTools.GetFromPrefs<Gen_Item_Code_lua>(key);
                if (last != null)
                {
                    this.workFolder = last.workFolder;
                    this.panel = last.panel;
                    this._type = last._type;
                    this.state = last.state;
                }
                FloderField = new LuaFloderField();
                FloderField.SetPath(workFolder);
                fields = new ScriptCreaterFieldsDrawer(creater, state);
                SetViewData();

            }
            public override void OnHierarchyChanged()
            {
                creater.ColllectMarks();
            }
            public override void OnDisable()
            {
                EditorTools.SaveToPrefs(this,key);
            }
            public override void OnGUI()
            {
                if (EditorApplication.isCompiling)
                {
                    GUILayout.Label("Editor is Compiling\nplease wait");
                    return;
                }

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Directory", GUIStyles.Get("toolbar"));
                    GUILayout.Space(20);
                    FloderField.OnGUI(EditorGUILayout.GetControlRect());
                    if (FloderField.leagal)
                        workFolder = FloderField.path;
                    else
                        FloderField.SetPath(workFolder);
                    GUILayout.EndHorizontal();
                }
                _type = (ItemType)EditorGUILayout.EnumPopup("Type", _type);

                EditorGUI.BeginChangeCheck();
                panel = EditorGUILayout.ObjectField("GameObject", panel, typeof(GameObject), true) as GameObject;
                if (EditorGUI.EndChangeCheck())
                {
                    SetViewData();
                }
                EditorGUILayout.LabelField("GenPath", luaPath);
                fields.OnGUI();

                if (GUILayout.Button("Gen"))
                {
                    if (panel == null) EditorWindow.focusedWindow.ShowNotification(new GUIContent("Set UI Panel "));
                    else
                    {
                        string err;
                        if (!creater.FieldCheck(out err)) EditorUtility.DisplayDialog("Err", err, "ok");
                        else
                        {
                            CreateView(luaPath);
                            AssetDatabase.Refresh();
                        }
                    }
                }
            }
            private void FindDir()
            {
                string total = $"{panelName}.lua.txt";
                string find = AssetDatabase.GetAllAssetPaths().ToList().Find(x => x.EndsWith(total));
                if (string.IsNullOrEmpty(find))
                {
                    FloderField.SetPath(string.Empty);
                }
                else
                {
                    FloderField.SetPath(find.Replace(total, "").ToAssetsPath());
                    string txt = File.ReadAllText(luaPath);
                    var names = Enum.GetNames(typeof(ItemType));
                    foreach (var item in names)
                    {
                        if (txt.Contains($": {item}"))
                        {
                            _type = (ItemType)Enum.Parse(typeof(ItemType), item);
                            break;
                        }
                    }
                }
            }
            private void SetViewData()
            {
                if (panel != null)
                {
                    creater.SetGameObject(panel.gameObject);
                    FindDir();
                }
                else
                {
                    creater.SetGameObject(null);
                    FloderField.SetPath(string.Empty);
                }
            }

            private string StaticUse()
            {
                List<string> usings = new List<string>();
                var marks = creater.GetMarks();
                if (marks != null)
                {
                    for (int i = 0; i < marks.Count; i++)
                    {
                        string ns = marks[i].fieldType;
                        if (!usings.Contains(ns))
                        {
                            usings.Add(ns);
                        }
                    }
                }
                string use = "";
                for (int i = 0; i < usings.Count; i++)
                {
                    use += string.Format("local {2} = StaticUsing(\"{0}\"){1}", usings[i], i == usings.Count - 1 ? "" : "\n", usings[i].Split('.').Last());
                }
                return use;
            }
            private string Fields()
            {
                var marks = creater.GetMarks();
                string f = "";
                string root_path = creater.gameObject.transform.GetPath();
                if (marks != null)
                {
                    for (int i = 0; i < marks.Count; i++)
                    {
                        string fieldType = marks[i].fieldType;
                        string fieldName = marks[i].fieldName;
                        string type = marks[i].fieldType.Split('.').Last();
                        string path = marks[i].transform.GetPath();

                        path = path.Remove(0, root_path.Length + 1);
                        f += "\t\t---@type " + fieldType + "\n";
                        f += string.Format("\t\t{0} = self:GetComponent(\"{1}\", typeof({2})),{3}", fieldName, path, type, i == marks.Count - 1 ? "" : "\n");
                    }
                }

                return f;
            }

            private void CreateView(string path)
            {
                if (File.Exists(path))
                {
                    var target = string.Format("function {0}:ctor(gameObject)", panelName);
                    var txt = File.ReadAllText(path);
                    int start = txt.IndexOf(target);
                    start = txt.IndexOf("self.Controls", start);
                    int depth = 0;
                    int end = -1;
                    for (int i = start + 1; i < txt.Length; i++)
                    {
                        char data = txt[i];
                        if (data == '{')
                        {
                            if (depth == 0)
                                start = i;
                            depth++;
                        }
                        else if (data == '}')
                            depth--;
                        else
                        {
                            continue;
                        }
                        if (depth == 0)
                        {
                            end = i;
                            break;
                        }
                    }
                    if (end == -1) return;
                    txt = txt.Remove(start, end - start + 1);
                    string fs = $"{{\n {Fields()}\n\t}}";
                    txt = txt.Insert(start, fs);
                    var flag = "---ViewUseFlag";
                    start = txt.IndexOf(flag);
                    end = txt.IndexOf(flag, start + flag.Length);
                    txt = txt.Remove(start, end - start);
                    File.WriteAllText(path, txt.Insert(start,$"{flag}\n{StaticUse()}\n"));
                }
                else
                {
                    string source = vSource;
                    if (_type == ItemType.LuaObject)
                        source = ObjSource;
                    if (_type == ItemType.UIObject)
                        source = UISource;
                    string result = source.Replace("#PanelName#", panelName)
                        .Replace(MVC_GenCodeView_Lua.ViewUseFlag, StaticUse())
                            .Replace(MVC_GenCodeView_Lua.ViewFeildFlag, Fields());
                    File.WriteAllText(path, result.ToUnixLineEndings());
                }
            }

            private static string Get_base(ItemType type)
            {
                string _base = type.ToString();
                string add = type != ItemType.UIItem ? "\tself:SetGameObject(gameObject)\n" : "";
                return
MVC_GenCodeView_Lua.head + "\n" +
"---ViewUseFlag\n" +
MVC_GenCodeView_Lua.ViewUseFlag + "\n\n" +
"---ViewUseFlag\n" +
$"---@class #PanelName# : {_base}\n" +
$"local #PanelName# = class(\"#PanelName#\",{_base})\n\n" +
"function #PanelName#:ctor(gameObject)\n" +
 add +
"\tself.Controls = {\n" +
MVC_GenCodeView_Lua.ViewFeildFlag + "\n" +
"\t}\n" +
"end\n\n";
            }
            static string vSource
            {
                get
                {
                    return Get_base(ItemType.UIItem) +
    "function #PanelName#:OnGet()\n" +
    "\t--BindUIEvent\n\n" +
    "end\n\n" +
    "function #PanelName#:OnSet()\n\n" +
    "end\n\n" +
    "return #PanelName#";
                }
            }



            static string ObjSource
            {
                get
                {
                    return Get_base(ItemType.LuaObject) + "return " + "#PanelName#";
                }
            }
            static string UISource
            {
                get
                {
                    return Get_base(ItemType.UIObject) + "return " + "#PanelName#";

                }
            }

        }

    }


}

