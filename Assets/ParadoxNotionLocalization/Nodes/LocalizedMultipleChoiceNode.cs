using System.Collections.Generic;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using IconAttribute = ParadoxNotion.Design.IconAttribute;
using Logger = ParadoxNotion.Services.Logger;


namespace TSF.ParadoxNotion.Localization
{
    [Icon("List")]
    [Name("Localized Multiple Choice")]
    [Category("Branch")]
    [Description("Prompt a Dialogue Multiple Choice with localization strings. A choice will be available if the choice condition(s) are true or there is no choice conditions. The Actor selected is used for the condition checks and will also Say the selection if the option is checked.")]
    [Color("b3ff7f")]
    public class LocalizedMultipleChoiceNode : DTNode
    {

        [System.Serializable]
        public class Choice
        {
            public bool isUnfolded = true;
            public LocalizedStatement statement;
            public ConditionTask condition;
            public Choice() { }
            public Choice(LocalizedStatement statement)
            {
                this.statement = statement;
            }
        }

        ///----------------------------------------------------------------------------------------------
        [SliderField(0f, 10f)]
        public float availableTime;
        public bool saySelection;

        [SerializeField, AutoSortWithChildrenConnections]
        private List<Choice> availableChoices = new List<Choice>();

        public override int maxOutConnections { get { return availableChoices.Count; } }
        public override bool requireActorSelection { get { return true; } }

        protected override Status OnExecute(Component agent, IBlackboard bb)
        {

            if (outConnections.Count == 0)
            {
                return Error("There are no connections to the Multiple Choice Node!");
            }

            var finalOptions = new Dictionary<IStatement, int>();

            for (var i = 0; i < availableChoices.Count; i++)
            {
                var condition = availableChoices[i].condition;
                if (condition == null || condition.CheckOnce(finalActor.transform, bb))
                {
                    var tempStatement = availableChoices[i].statement.BlackboardReplace(bb);
                    finalOptions[tempStatement] = i;
                }
            }

            if (finalOptions.Count == 0)
            {
                Logger.Log("Multiple Choice Node has no available options. Dialogue Ends.", LogTag.EXECUTION, this);
                DLGTree.Stop(false);
                return Status.Failure;
            }

            var optionsInfo = new MultipleChoiceRequestInfo(finalActor, finalOptions, availableTime, OnOptionSelected);
            optionsInfo.showLastStatement = true;
            DialogueTree.RequestMultipleChoices(optionsInfo);
            return Status.Running;
        }

        void OnOptionSelected(int index)
        {

            status = Status.Success;

            System.Action Finalize = () => { DLGTree.Continue(index); };

            if (saySelection)
            {
                var tempStatement = availableChoices[index].statement.BlackboardReplace(graphBlackboard);
                var speechInfo = new SubtitlesRequestInfo(finalActor, tempStatement, Finalize);
                DialogueTree.RequestSubtitles(speechInfo);
            }
            else
            {
                Finalize();
            }
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR
        private readonly GUIContent _content = new GUIContent();
        private static readonly GUIContent Text = new GUIContent("Text");
        
        private List<LocalizedStringProxy> _proxies;

        ~LocalizedMultipleChoiceNode()
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


        public override void OnConnectionInspectorGUI(int i)
        {
            if (i >= 0) { DoChoiceGUI(i, availableChoices[i]); }
        }

        public override string GetConnectionInfo(int i)
        {
            if (i >= availableChoices.Count)
            {
                return "NOT SET";
            }
            var text = string.Format("'{0}'", availableChoices[i].statement.text);
            if (availableChoices[i].condition == null)
            {
                return text;
            }
            return string.Format("{0}\n{1}", text, availableChoices[i].condition.summaryInfo);
        }

        protected override void OnNodeGUI()
        {

            if (availableChoices.Count == 0)
            {
                GUILayout.Label("No Options Available");
                return;
            }

            for (var i = 0; i < availableChoices.Count; i++)
            {
                var choice = availableChoices[i];
                var connection = i < outConnections.Count ? outConnections[i] : null;
                GUILayout.BeginHorizontal(Styles.roundedBox);
                GUILayout.BeginVertical();
                GUILayout.Label(choice.statement.ToString());
                GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
                if (LocalizationSettings.SelectedLocale != null)
                {
                    var preview = choice.statement.localizedText.GetLocalizedStringBlackboard(graphBlackboard);
                    _content.text = string.Format("{0} {1}", connection != null ? "■" : "□", preview.CapLength(30));
                    _content.tooltip = preview;
                    GUILayout.Label(_content, Styles.leftLabel);
                }
                else
                {
                    _content.text = "No active locale";
                    _content.tooltip = "To enable preview, select an active locale via the Localization Scene Controls Window (Window/Asset Management).";
                    GUILayout.Label(_content, EditorStyles.centeredGreyMiniLabel);
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            if (availableTime > 0)
            {
                GUILayout.Label(availableTime + "' Seconds");
            }
            if (saySelection)
            {
                GUILayout.Label("Say Selection");
            }
            GUILayout.EndHorizontal();
        }

        protected override void OnNodeInspectorGUI()
        {
            base.OnNodeInspectorGUI();

            if (GUILayout.Button("Add Choice"))
            {
                availableChoices.Add(new Choice(new LocalizedStatement()));
            }

            if (availableChoices.Count == 0)
            {
                return;
            }
            
            if (_proxies == null || _proxies.Count != availableChoices.Count)
            {
                _proxies = new List<LocalizedStringProxy>();
                foreach (var _ in availableChoices)
                {
                    var proxy = LocalizedStringProxy.Create();
                    _proxies.Add(proxy);
                }
            }

            EditorUtils.ReorderableList(availableChoices, (i, picked) =>
            {
                var choice = availableChoices[i];
                GUILayout.BeginHorizontal("box");

                var text = string.Format("{0} {1}", choice.isUnfolded ? "▼ " : "► ", choice.statement.ToString());
                if (GUILayout.Button(text, (GUIStyle)"label", GUILayout.Width(0), GUILayout.ExpandWidth(true)))
                {
                    choice.isUnfolded = !choice.isUnfolded;
                }

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    availableChoices.RemoveAt(i);
                    if (i < outConnections.Count)
                    {
                        graph.RemoveConnection(outConnections[i]);
                    }
                }

                GUILayout.EndHorizontal();

                if (choice.isUnfolded)
                {
                    DoChoiceGUI(i, choice);
                }
            });

        }
        
        void DoChoiceGUI(int i, Choice choice)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical("box");

            choice.statement ??= new LocalizedStatement();
            choice.statement.localizedText ??= new LocalizedString();
            var proxy = _proxies[i];
            proxy.Value = choice.statement.localizedText;
            UnityEditor.EditorGUILayout.PropertyField(proxy, Text);
            proxy.Apply();
            GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true)); // Separator
            choice.statement.audio = UnityEditor.EditorGUILayout.ObjectField("Audio File", choice.statement.audio, typeof(AudioClip), false) as AudioClip;
            choice.statement.meta = UnityEditor.EditorGUILayout.TextField("Meta Data", choice.statement.meta);

            NodeCanvas.Editor.TaskEditor.TaskFieldMulti<ConditionTask>(choice.condition, graph, (c) => { choice.condition = c; });

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

#endif
    }
}
