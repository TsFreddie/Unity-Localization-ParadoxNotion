#if NODECANVAS

using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace TSF.ParadoxNotion.Localization
{
    ///<summary>Holds data of what's being said usually by an actor</summary>
    [System.Serializable]
    public class LocalizedStatement : IStatement
    {
        [SerializeField] private LocalizedString _text;
        [SerializeField] private AudioClip _audio;
        [SerializeField] private string _meta = string.Empty;

        public string text
        {
            get
            {
                if (_text == null) return null;
                if (_text.IsEmpty) return null;
                return _text.GetLocalizedString();
            }
        }

        public LocalizedString localizedText
        {
            get => _text;
            set => _text = value;
        }

        public AudioClip audio
        {
            get { return _audio; }
            set { _audio = value; }
        }

        public string meta
        {
            get { return _meta; }
            set { _meta = value; }
        }

        //required
        public LocalizedStatement() { }
        public LocalizedStatement(TableReference table, TableEntryReference entry)
        {
            this.localizedText = new LocalizedString(table, entry);
        }

        public LocalizedStatement(TableReference table, TableEntryReference entry, AudioClip audio)
        {
            this.localizedText = new LocalizedString(table, entry);
            this.audio = audio;
        }

        public LocalizedStatement(TableReference table, TableEntryReference entry, AudioClip audio, string meta)
        {
            this.localizedText = new LocalizedString(table, entry);
            this.audio = audio;
            this.meta = meta;
        }

        public LocalizedStatement(LocalizedString text)
        {
            this.localizedText = text;
            this.audio = audio;
            this.meta = meta;
        }

        public LocalizedStatement(LocalizedString text, AudioClip audio)
        {
            this.localizedText = text;
            this.audio = audio;
            this.meta = meta;
        }

        public LocalizedStatement(LocalizedString text, AudioClip audio, string meta)
        {
            this.localizedText = text;
            this.audio = audio;
            this.meta = meta;
        }

        ///<summary>Process smart string arguments and returns a Statement copy</summary>
        public IStatement BlackboardReplace(IBlackboard bb)
        {
            var copy = new Statement(localizedText.GetLocalizedStringBlackboard(bb), audio, meta);
            return copy;
        }

        public override string ToString()
        {
            if (localizedText == null)
            {
                return "None";
            }
            return $"{localizedText.ToHumanString()}";
        }
    }
}

#endif
