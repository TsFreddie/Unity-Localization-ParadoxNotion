using UnityEngine;
using System.Collections.Generic;
using ParadoxNotion.Design;
using NodeCanvas.Framework;
using NodeCanvas.DialogueTrees;
using UnityEditor;
using UnityEngine.Localization;
using IconAttribute = ParadoxNotion.Design.IconAttribute;

namespace TSF.ParadoxNotion.Localization
{
    [Category("Dialogue")]
    [Icon("Dialogue")]
    [Description("A random localized statement will be chosen each time for the actor to say")]
    [Name("Say Random Localized")]
    public class SayRandomLocalized : ActionTask<IDialogueActor>
    {
        public List<LocalizedStatement> statements = new List<LocalizedStatement>();

        protected override void OnExecute()
        {
            var index = Random.Range(0, statements.Count);
            var statement = statements[index];
            var tempStatement = statement.BlackboardReplace(blackboard);
            var info = new SubtitlesRequestInfo(agent, tempStatement, EndAction);
            DialogueTree.RequestSubtitles(info);
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR
        private List<LocalizedStringProxy> _proxies;
        private static readonly GUIContent Text = new GUIContent("Text");

        ~SayRandomLocalized()
        {
            if (_proxies != null)
            {
                foreach (var proxy in _proxies)
                {
                    if (proxy)
                    {
                        EditorApplication.delayCall += () =>
                        {
                            Object.DestroyImmediate(proxy);
                        };
                    }
                }
            }
            _proxies = null;
        }

        protected override void OnTaskInspectorGUI()
        {
            var options = new EditorUtils.ReorderableListOptions();
            options.allowAdd = true;
            options.allowRemove = true;
            options.unityObjectContext = ownerSystem.contextObject;

            if (_proxies == null || _proxies.Count != statements.Count)
            {
                _proxies = new List<LocalizedStringProxy>();
                foreach (var _ in statements)
                {
                    var proxy = LocalizedStringProxy.Create();
                    _proxies.Add(proxy);
                }
            }

            EditorUtils.ReorderableList(statements, options, (i, picked) =>
            {
                statements[i] ??= new LocalizedStatement();
                var statement = statements[i];
                statement.localizedText ??= new LocalizedString();
                if (blackboard != null)
                {
                    LocalizedStringExtension.ArgumentCache[0] = blackboard;
                    statement.localizedText.Arguments = LocalizedStringExtension.ArgumentCache;
                }
                var proxy = _proxies[i];
                proxy.Value = statement.localizedText;
                EditorGUILayout.PropertyField(proxy, Text);
                proxy.Apply();
                GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true)); // Separator
                statement.audio = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", statement.audio, typeof(AudioClip), false);
                statement.meta = EditorGUILayout.TextField("Meta", statement.meta);
                EditorUtils.Separator();
            });
        }
#endif

    }
}
