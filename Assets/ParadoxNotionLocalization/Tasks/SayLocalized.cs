#if NODECANVAS

using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;

namespace TSF.ParadoxNotion.Localization
{
    [Category("Dialogue")]
    [Description("Blackboard variables will be passed as Smart String arguments, make sure to follow the Smart String format.")]
    [global::ParadoxNotion.Design.Icon("Dialogue")]
    [Name("Say Localized")]
    public class SayLocalized : ActionTask<IDialogueActor>
    {
        public LocalizedStatement statement = new LocalizedStatement();

        protected override string info => statement.localizedText?.ToHumanString() ?? "None";

        protected override void OnExecute()
        {
            var tempStatement = statement.BlackboardReplace(blackboard);
            DialogueTree.RequestSubtitles(new SubtitlesRequestInfo(agent, tempStatement, EndAction));
        }

#if UNITY_EDITOR
        private LocalizedStringProxy _proxy;
        private static readonly GUIContent Text = new GUIContent("Text");
        
        ~SayLocalized()
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
        
        protected override void OnTaskInspectorGUI()
        {
            statement.localizedText ??= new LocalizedString();
            if (!_proxy) _proxy = LocalizedStringProxy.Create();
            
            if (blackboard != null)
            {
                LocalizedStringExtension.ArgumentCache[0] = blackboard;
                statement.localizedText.Arguments = LocalizedStringExtension.ArgumentCache;
            }
            _proxy.Value = statement.localizedText;
            EditorGUILayout.PropertyField(_proxy, Text);
            _proxy.Apply();
            LocalizedStringExtension.ArgumentCache[0] = null;
            statement.localizedText.Arguments = null;

            GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true)); // Separator
            statement.audio = EditorGUILayout.ObjectField("Audio File", statement.audio, typeof(AudioClip), false) as AudioClip;
            statement.meta = EditorGUILayout.TextField("Metadata", statement.meta);
        }
#endif
    }
}

#endif