﻿/*********************************************************************************
 *Author:         OnClick
 *Version:        0.0.1
 *UnityVersion:   2017.2.3p3
 *Date:           2019-07-30
 *Description:    IFramework
 *History:        2018.11--
*********************************************************************************/
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
namespace IFramework.UI
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public class ScriptCreatorContext : MonoBehaviour
    {
        [HideInInspector] public bool containsChildren;
        [HideInInspector] public List<string> ignorePaths = new List<string>();
        [HideInInspector] public List<MarkContext> marks = new List<MarkContext>();
        private const string flag = "@sm";

        [System.Serializable]
        public class MarkContext
        {
            public GameObject gameObject;
            public string fieldName;
            public string fieldType;
        }
        public MarkContext AddMark(GameObject go, string type)
        {
            var find = marks.Find(x => x.gameObject == go);

            if (find == null)
            {
                find = new MarkContext() { gameObject = go, fieldType = type };
                AddMarkFlag(go);
                marks.Add(find);
            }
            else
            {
                find.fieldType = type;
            }
            ValidateMarkFieldName(find);
            return find;
        }
        private void AddMarkFlag(GameObject go)
        {
            if (go == gameObject) return;
            string name = go.name;
            if (name.Contains(flag)) return;
            go.name += flag;
        }
        public void RemoveMark(GameObject go)
        {
            RemoveMarkFlag(go);
            marks.RemoveAll(x => x.gameObject == go);
        }
        private void RemoveMarkFlag(GameObject go)
        {
            string name = go.name;
            if (!name.Contains(flag)) return;
            go.name = name.Replace(flag, "");
        }
        private static bool IsLegalFieldName(string self)
        {
            if (string.IsNullOrEmpty(self)) return false;
            return Regex.IsMatch(self, @"^[_a-zA-Z][_a-zA-Z0-9]*$");
        }
        private void ValidateMarkFieldName(MarkContext mark)
        {

            if (!IsLegalFieldName(mark.fieldName)) mark.fieldName = mark.gameObject.name.Replace(flag, "");
            var m = Regex.Matches(mark.fieldName, "[_a-zA-Z0-9]");
            var list = m.Where(x => x.Success).Select(x => x.Value).ToList();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                if (i == 0)
                {
                    int _val;
                    if (int.TryParse(list[i], out _val))
                    {
                        sb.Append("_");
                    }
                }
                sb.Append(list[i]);
            }
            mark.fieldName = sb.ToString();

        }

        public bool HandleSameFieldName(out string same)
        {
            same = "";
            bool exist = false;
            var prefab_list = new List<string>();
            if (this.containsChildren)
            {
                var all = GetAllMarks();
                all.RemoveAll(x => marks.Contains(x));
                prefab_list = all.Select(x => x.fieldName).ToList();
            }
            Dictionary<string, List<MarkContext>> map = new Dictionary<string, List<MarkContext>>();
            for (int i = 0; i < marks.Count; i++)
            {
                var cur = marks[i];
                if (!map.ContainsKey(cur.fieldName))
                    map.Add(cur.fieldName, new List<MarkContext>());
                map[cur.fieldName].Add(cur);
            }
            foreach (var item in map)
            {
                if (item.Value.Count > 1 || prefab_list.Contains(item.Key))
                {
                    exist = true;
                    var list = item.Value;
                    same += $"{list[0].fieldName}  ";

                    for (int i = 0; i < list.Count; i++)
                    {
                        var cur = list[i];
                        list[i].fieldName += $"_{i}";
                        var sameParrent = list.FindAll(x => x.gameObject.transform.parent == cur.gameObject.transform.parent);
                        if (sameParrent.Count != 1)
                            for (int i2 = 0; i2 < sameParrent.Count; i2++)
                            {
                                var __same = sameParrent[i2];
                                __same.gameObject.transform.name = __same.fieldName + flag;
                            }
                    }
                }
            }
            return exist;
        }
        public void DestroyMarks()
        {
            for (int i = 0; i < marks.Count; i++)
            {
                RemoveMarkFlag(marks[i].gameObject);
            }
            marks.Clear();
        }

        public List<MarkContext> GetAllMarks()
        {

            var creators = gameObject.GetComponentsInChildren<ScriptCreatorContext>(true);
            List<MarkContext> result = new List<MarkContext>(this.marks);
            foreach (var creator in creators)
            {
                foreach (var mark in creator.marks)
                {
                    if (result.Any(x => x.gameObject == mark.gameObject)) continue;
                    result.Add(mark);
                }
            }
            return result.Distinct().ToList();
        }

        public MarkContext GetMark(GameObject go)
        {
            return marks.Find(x => x.gameObject == go);
        }

        public void RemoveEmpty()
        {
            marks.RemoveAll(x => x.gameObject == null);
        }
    }
}
