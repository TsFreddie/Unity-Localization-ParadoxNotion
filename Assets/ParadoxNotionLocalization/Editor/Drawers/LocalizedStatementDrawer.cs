#if NODECANVAS && UNITY_EDITOR

using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using TSF.ParadoxNotion.Localization;
using UnityEditor;
using UnityEngine.Localization;

namespace NodeCanvas.Editor
{
    ///<summary>A drawer for dialogue tree statements</summary>
    public class LocalizedStatementDrawer : ObjectDrawer<LocalizedStatement>
    {
        private LocalizedStringProxy _proxy;
        private static readonly GUIContent Text = new GUIContent("Text");

        ~LocalizedStatementDrawer()
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

        public override LocalizedStatement OnGUI(GUIContent content, LocalizedStatement instance)
        {
            instance.localizedText ??= new LocalizedString();
            if (!_proxy) _proxy = LocalizedStringProxy.Create();

            var dialogueTree = contextUnityObject as DialogueTree;
            if (dialogueTree)
            {
                LocalizedStringExtension.ArgumentCache[0] = dialogueTree.blackboard;
                instance.localizedText.Arguments = LocalizedStringExtension.ArgumentCache;
            }
            _proxy.Value = instance.localizedText;
            EditorGUILayout.PropertyField(_proxy, Text);
            _proxy.Apply();
            LocalizedStringExtension.ArgumentCache[0] = null;
            instance.localizedText.Arguments = null;

            GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true)); // Separator
            instance.audio = EditorGUILayout.ObjectField("Audio File", instance.audio, typeof(AudioClip), false) as AudioClip;
            instance.meta = EditorGUILayout.TextField("Metadata", instance.meta);
            return instance;
        }
    }
}

#endif
