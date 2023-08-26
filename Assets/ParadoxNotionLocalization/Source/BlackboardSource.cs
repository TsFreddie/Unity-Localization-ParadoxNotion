using System;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.Core.Settings;

namespace TSF.ParadoxNotion.Localization.Source
{
    /// <summary>
    /// Provides the ability to extract an object with a matching Key from Blackboard
    /// </summary>
    [Serializable]
    public class BlackboardSource : ISource
    {
        [Tooltip("If true, the variable name will be used as a fallback if the variable can not be found.")]
        public bool FallbackToVariableName = true;
            
        /// <summary>
        /// Creates a new instance of the formatter.
        /// </summary>
        /// <param name="formatter">The formatter that the source will be added to.</param>
        public BlackboardSource(SmartFormatter formatter)
        {
            // Add some special info to the parser:
            formatter.Parser.AddAlphanumericSelectors(); // (A-Z + a-z)
            formatter.Parser.AddAdditionalSelectorChars("_");
            formatter.Parser.AddOperators(".");
        }
        
        internal StringComparison GetCaseSensitivityComparison(ISelectorInfo selectorInfo)
        {
           switch (selectorInfo.FormatDetails.Settings.CaseSensitivity)
                {
                case CaseSensitivityType.CaseSensitive:
                    return StringComparison.Ordinal;
                case CaseSensitivityType.CaseInsensitive:
                    return StringComparison.OrdinalIgnoreCase;
                default:
                    throw new InvalidOperationException(
                        $"The case sensitivity type [{selectorInfo.FormatDetails.Settings.CaseSensitivity}] is unknown.");
            }
        }

        public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            var current = selectorInfo.CurrentValue;
            var selector = selectorInfo.SelectorText;

            // See if current is a IDictionary and contains the selector:
            var blackboard = current as IBlackboard;
            if (blackboard != null)
            {
                var variable = blackboard.GetVariable(selector);
                if (variable != null && variable.value != null)
                {
                    // A special case to support LocalizedString as Blackboard variable
                    if (variable.value is LocalizedString localizedString)
                    {
                        selectorInfo.Result = localizedString.GetLocalizedString();
                        return true;
                    }
                    selectorInfo.Result = variable.value;
                    return true;
                }
                
                if (FallbackToVariableName)
                {
                    selectorInfo.Result = selectorInfo.SelectorText;
                    return true;
                }
            }
            return false;
        }
    }
}
