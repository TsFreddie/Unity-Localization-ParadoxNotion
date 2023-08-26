#if NODECANVAS

using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace TSF.ParadoxNotion.Localization
{
    [Name("Say Localized")]
    [Description("Make the selected Dialogue Actor talk using Unity.Localization. Blackboard variables will be passed as Smart String arguments, make sure to follow the Smart String format.")]
    public class LocalizedStatementNode : DTNode
    {
        [SerializeField]
        public LocalizedStatement statement = new LocalizedStatement();

        public override bool requireActorSelection { get { return true; } }

        protected override Status OnExecute(Component agent, IBlackboard bb)
        {
            var tempStatement = statement.BlackboardReplace(agent.GetComponent<Blackboard>());
            DialogueTree.RequestSubtitles(new SubtitlesRequestInfo(finalActor, tempStatement, OnStatementFinish));
            return Status.Running;
        }

        void OnStatementFinish()
        {
            status = Status.Success;
            DLGTree.Continue();
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR
        private readonly GUIContent _content = new GUIContent();

        protected override void OnNodeGUI()
        {
            GUILayout.BeginVertical(Styles.roundedBox);
            GUILayout.Label(statement.ToString());
            GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
            if (LocalizationSettings.SelectedLocale != null)
            {
                var preview = statement.localizedText?.GetLocalizedStringBlackboard(graphBlackboard);
                _content.text = "\"<i> " + preview.CapLength(30) + "</i> \"";
                _content.tooltip = preview;
                GUILayout.Label(_content);
            }
            else
            {
                _content.text = "No active locale";
                _content.tooltip = "To enable preview, select an active locale via the Localization Scene Controls Window (Window/Asset Management).";
                GUILayout.Label(_content, EditorStyles.centeredGreyMiniLabel);
            }
            GUILayout.EndVertical();
        }
#endif

    }
}

#endif
