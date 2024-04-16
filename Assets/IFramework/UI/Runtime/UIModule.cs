﻿/*********************************************************************************
 *Author:         OnClick
 *Version:        0.0.1
 *UnityVersion:   2017.2.3p3
 *Date:           2019-07-02
 *Description:    IFramework
 *History:        2018.11--
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace IFramework.UI
{
    public partial class UIModule : UpdateModule
    {
        const string item_layer = "items";
        const string raycast_layer = "raycast";
        public Canvas canvas { get; private set; }
        private IGroups _groups;
        private UIAsset _asset;
        private Dictionary<UILayer, List<UIPanel>> _panelOrders;
        private Dictionary<string, RectTransform> _layers;
        private Queue<LoadPanelAsyncOperation> asyncLoadQueue;
        private ItemsPool _itemPool;
        private Dictionary<string, UIPanel> panels = new Dictionary<string, UIPanel>();
        private Empty4Raycast raycast;
        private bool _loading = false;
        private IUIDelegate del;

        protected override void Awake()
        {
            _panelOrders = new Dictionary<UILayer, List<UIPanel>>();
            _layers = new Dictionary<string, RectTransform>();
            asyncLoadQueue = new Queue<LoadPanelAsyncOperation>();
            _itemPool = new ItemsPool(this);
        }


        protected override void OnDispose()
        {
            if (_groups != null)
                _groups.Dispose();
            asyncLoadQueue.Clear();
            _layers.Clear();
            if (canvas != null)
                GameObject.Destroy(canvas.gameObject);
            _itemPool.Clear();
        }
        protected override void OnUpdate()
        {
            CheckAsyncLoad();
        }


        private RectTransform CreateLayer(string layerName)
        {
            GameObject go = new GameObject(layerName);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.SetParent(canvas.transform);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.localPosition = Vector3.zero;
            rect.sizeDelta = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            _layers.Add(layerName, rect);
            return rect;
        }
        private void CreateLayers()
        {
            foreach (UILayer item in Enum.GetValues(typeof(UILayer)))
                CreateLayer(item.ToString());
            var items = CreateLayer(item_layer);
            CanvasGroup group = items.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            var ray = CreateLayer(raycast_layer);
            raycast = ray.gameObject.AddComponent<Empty4Raycast>();
        }

        private RectTransform GetLayerRectTransform(string layer)
        {
            return _layers[layer];
        }
        private void SetOrder(string path, UIPanel panel)
        {
            UILayer layer = GetPanelLayer(path);
            if (!_panelOrders.ContainsKey(layer))
                _panelOrders.Add(layer, new List<UIPanel>());
            var list = _panelOrders[layer];
            if (list.Contains(panel)) return;
            int order = GetPanelLayerOrder(path);

            for (int i = 0; i < list.Count; i++)
            {
                if (GetPanelLayerOrder(list[i].GetPath()) > order)
                {
                    var sbindex = list[i].GetSiblingIndex();
                    panel.SetSiblingIndex(sbindex);
                    list.Insert(sbindex, panel);
                    return;
                }
            }
            list.Add(panel);
            if (del != null)
                del.OnLayerTopChange(layer, panel);
        }
        private void DestroyPanel(string path, UIPanel panel)
        {
            _panelOrders[GetPanelLayer(path)].Remove(panel);
            _asset.DestoryPanel(panel.gameObject);
        }
        private UILayer GetPanelLayer(string path)
        {
            return this._asset.GetPanelLayer(path);

        }
        private int GetPanelLayerOrder(string path)
        {
            return this._asset.GetPanelLayerOrder(path);
        }







        private void UILoadComplete(UIPanel ui, string path, Action<string, UIPanel> callback)
        {
            if (ui != null)
            {
                ui = ui.Clone(GetLayerRectTransform(GetPanelLayer(path).ToString()));
                ui.SetPath(path);
                SetOrder(path, ui);
                panels.Add(path, ui);
                _groups.Subscribe(path, ui);
                _groups.OnLoad(path);
                ui.SetState(PanelState.OnLoad);
            }
            callback?.Invoke(path, ui);
        }
        private void CheckAsyncLoad()
        {
            if (asyncLoadQueue.Count == 0)
            {
                if (_loading)
                {
                    HideRayCast();
                    _loading = false;
                }
            }
            else
            {
                ShowRaycast();
                while (asyncLoadQueue.Count > 0 && asyncLoadQueue.Peek().isDone)
                {
                    LoadPanelAsyncOperation op = asyncLoadQueue.Dequeue();
                    UILoadComplete(op.value, op.path, op.callback);
                    op.SetToDefault();
                }
            }

        }
        private void OnShowCallBack(string path, UIPanel panel, ShowPanelAsyncOperation op)
        {
            if (panel != null)
            {
                this._groups.OnShow(path);
                panel.SetState(PanelState.OnShow);
            }
            op.SetValue(true);
            op.SetToDefault();
        }
        private UIPanel Find(string path)
        {
            UIPanel ui;
            panels.TryGetValue(path, out ui);
            return ui;
        }
        private void Load(string path, Action<string, UIPanel> callback)
        {
            if (_groups == null)
                throw new Exception("Please Set IGroups First");
            if (_asset == null)
                throw new Exception("Please Set UILoader First");
            var result = _asset.LoadPanel(path);
            if (result != null)
            {
                UILoadComplete(result, path, callback);
            }
            else
            {
                LoadPanelAsyncOperation op = new LoadPanelAsyncOperation();
                op.callback = callback;
                op.path = path;
                if (_asset.LoadPanelAsync(path, op))
                {
                    _loading = true;
                    asyncLoadQueue.Enqueue(op);
                }
                else
                {
                    throw new Exception($"Can't load ui with Name: {path}");

                }
            }
        }

        public void ClearUselessItems()
        {
            _itemPool.ClearUseless();
        }

        public UIItemOperation GetItem(string path)
        {
            return _itemPool.Get(path);
        }
        public void SetItem(string path, UIItemOperation go)
        {
            _itemPool.Set(path, go);
        }
        public void SetItem(string path, GameObject go)
        {
            _itemPool.Set(path, go);
        }

        public void ShowRaycast()
        {
            raycast.raycastTarget = true;
        }
        public void HideRayCast()
        {
            raycast.raycastTarget = false;
        }
        /// <summary>
        /// 创建 画布
        /// </summary>
        public void CreateCanvas()
        {
            var canvas = _asset.GetCanvas();
            if (canvas == null)
            {
                var root = new GameObject(name);
                root.AddComponent<RectTransform>();
                this.canvas = root.AddComponent<Canvas>();
                root.AddComponent<CanvasScaler>();
                root.AddComponent<GraphicRaycaster>();
                this.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            else
            {
                this.canvas = canvas;
                this.canvas.name = name;
            }
            CreateLayers();
            HideRayCast();
        }

        /// <summary>
        /// 设置加载器
        /// </summary>
        /// <param name="asset"></param>
        public void SetAsset(UIAsset asset) => _asset = asset;
        /// <summary>
        /// 设置ui组管理器
        /// </summary>
        /// <param name="groups"></param>
        public void SetGroups(IGroups groups) => this._groups = groups;
        public void SetUIDelegate(IUIDelegate del) => this.del = del;

        /// <summary>
        /// 展示一个界面
        /// </summary>
        /// <param name="path"></param>
        public ShowPanelAsyncOperation Show(string path)
        {

            ShowPanelAsyncOperation op = new ShowPanelAsyncOperation();
            var panel = Find(path);
            if (panel == null)
                Load(path, (path, panel) =>
                {
                    OnShowCallBack(path, panel, op);
                });
            else
                OnShowCallBack(path, panel, op);
            return op;
        }
        /// <summary>
        /// 藏一个界面
        /// </summary>
        /// <param name="path"></param>
        public void Hide(string path)
        {
            var panel = Find(path);
            if (panel != null)
            {
                this._groups.OnHide(path);
                panel.SetState(PanelState.OnHide);

            }
        }
        /// <summary>
        /// 彻底关闭一个界面
        /// </summary>
        /// <param name="path"></param>
        public void Close(string path)
        {
            var panel = Find(path);

            if (panel != null)
            {
                this._groups.OnClose(path);
                panel.SetState(PanelState.OnClose);
                _groups.UnSubscribe(path);
                panels.Remove(path);
                DestroyPanel(path, panel);
            }
        }

        public void CloseAll()
        {
            var paths = panels.Keys.ToArray();
            for (int i = 0; i < paths.Length; i++)
            {
                Close(paths[i]);
            }
        }
    }
}
