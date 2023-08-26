#if UNITY_EDITOR

using ParadoxNotion.Design;
using TSF.ParadoxNotion.Localization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;

namespace NodeCanvas.Editor
{
    public class LocalizedStringDrawer : ObjectDrawer<LocalizedString>
    {
        private LocalizedStringProxy _proxy;
        private static readonly GUIContent Text = new GUIContent("Text");

        ~LocalizedStringDrawer()
        {
            if (_proxy)
            {
                var proxy = _proxy;
                EditorApplication.delayCall += () =>
                {
                    Object.DestroyImmediate(proxy);
                };
            }
            _proxy = null;
        }

        public override LocalizedString OnGUI(GUIContent content, LocalizedString instance)
        {
            if (instance == null) instance = new LocalizedString();
            if (!_proxy) _proxy = LocalizedStringProxy.Create();
            _proxy.Value = instance;
            EditorGUILayout.PropertyField(_proxy, Text);
            _proxy.Apply();
            return instance;
        }
    }
}

#endif
