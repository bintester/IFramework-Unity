/*********************************************************************************
 *Author:         OnClick
 *Version:        0.1
 *UnityVersion:   2021.3.33f1c1
 *Date:           2024-04-25
*********************************************************************************/
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace IFramework.Localization
{
    [UnityEngine.RequireComponent(typeof(TMPro.TMP_Text))]
    [DisallowMultipleComponent]
    [AddComponentMenu("IFramework/LocalizationTMP")]
    public class LocalizationTMP_Text : LocalizationGraphic<TMPro.TMP_Text>
    {
        [System.Serializable]
        public class TMPTextActor : LocalizationActor<LocalizationTMP_Text>
        {

            public string key;
            private string _lastKey;
            public string[] formatArgs = new string[0];

            public TMPTextActor(bool enable) : base(enable)
            {
            }

            protected override void Execute(string localizationType, LocalizationTMP_Text component)
            {
                _lastKey = key;
                var format = component.GetLocalization(key);
                try
                {
                    component.graphicT.text = string.Format(format, formatArgs);
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
            public void SetKey(string key)
            {
                this.key = key;
                ((ILocalizationActor)this).enable = true;
                ((ILocalizationActor)this).Execute();
            }
            protected override bool NeedExecute(string localizationType)
            {
                var _base = base.NeedExecute(localizationType);
                bool self = _lastKey != this.key;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    self = true;
#endif
                return self || _base;
            }
        }
        [System.Serializable]
        public class TMPFontActor : LocalizationMapActor<LocalizationTMP_Text, TMP_FontAsset>
        {
            public TMPFontActor(bool enable) : base(enable)
            {
            }

            protected override void Execute(string localizationType, LocalizationTMP_Text component)
            {
                component.graphicT.font = GetValue(localizationType);
            }
        }

        [System.Serializable]
        public class TMPFontSizeActor : LocalizationMapActor<LocalizationTMP_Text, float>
        {
            public TMPFontSizeActor(bool enable) : base(enable)
            {
            }

            protected override void Execute(string localizationType, LocalizationTMP_Text component)
            {
                component.graphicT.fontSize = GetValue(localizationType);
            }
        }
        public TMPTextActor text = new TMPTextActor(true);
        public TMPFontActor font = new TMPFontActor(false);
        public TMPFontSizeActor fontSize = new TMPFontSizeActor(false);


        protected override List<ILocalizationActor> GetActors()
        {
            var _base = base.GetActors();
            _base.Add(text);
            _base.Add(font);
            _base.Add(fontSize);

            return _base;
        }

    }
}
