using System.Reflection;
using NodeCanvas.Framework;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace TSF.ParadoxNotion.Localization
{
    public static class LocalizedStringExtension
    {
        internal static readonly object[] ArgumentCache = new object[1];
        private static PropertyInfo _sharedTableDataPropertyInfo;

        public static string GetLocalizedStringBlackboard(this LocalizedString localizedString, IBlackboard bb)
        {
            if (localizedString.IsEmpty) return null;

            ArgumentCache[0] = bb;
            localizedString.Arguments = ArgumentCache;
            var result = localizedString.GetLocalizedString();
            localizedString.Arguments = null;
            ArgumentCache[0] = null;
            return result;
        }

        public static string ToHumanString(this LocalizedReference value)
        {
            // use reflection to get LocalizedReference.TableReference.SharedTableData
            _sharedTableDataPropertyInfo ??= typeof(TableReference).GetProperty("SharedTableData", BindingFlags.NonPublic | BindingFlags.Instance);

            // fallback to the default verbose ToString
            if (_sharedTableDataPropertyInfo == null) return value.ToString();

            var collection = value.TableReference;
            var entry = value.TableEntryReference;

            if (collection.ReferenceType == TableReference.Type.Empty || entry.ReferenceType == TableEntryReference.Type.Empty)
                return "None";

            var key = (entry.ReferenceType) switch
            {
                TableEntryReference.Type.Name => entry.Key,
                TableEntryReference.Type.Id => (_sharedTableDataPropertyInfo.GetValue(value.TableReference) as SharedTableData)?.GetKey(entry.KeyId),
                _ => null,
            };

            return key == null ? value.ToString() : $"{collection.TableCollectionName}/{key}";
        }
    }
}
