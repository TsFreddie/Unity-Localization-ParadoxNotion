#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;

namespace TSF.ParadoxNotion.Localization
{
    /// <summary>
    /// To make drawing custom editor easier.
    /// </summary>
    public class LocalizedStringProxy : ScriptableObject
    {
        private SerializedProperty _property;

        [SerializeField]
        private LocalizedString _value;

        public LocalizedString Value
        {
            get => _value;
            set
            {
                if (_value == value)
                    return;

                _value = value;
                if (this) _property = new SerializedObject(this).FindProperty("_value");
                else _property = null;
            }
        }

        public static LocalizedStringProxy Create()
        {
            var proxy = CreateInstance<LocalizedStringProxy>();
            proxy._property = new SerializedObject(proxy).FindProperty("_value");
            return proxy;
        }

        public static implicit operator SerializedProperty(LocalizedStringProxy proxy)
        {
            return proxy._property;
        }

        private void OnDestroy()
        {
            _property = null;
            _value = null;
        }

        public void Apply()
        {
            _property.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif
