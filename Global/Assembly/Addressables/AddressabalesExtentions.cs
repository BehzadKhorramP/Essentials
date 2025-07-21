using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
#endif




namespace MadApper.Addressable
{

    public static class AddressabalesExtentions
    {

#if UNITY_EDITOR

        public static AssetReference SetAddressableGroup(this Object obj, string groupName)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings not found!");
                return null;
            }

            var group = settings.FindGroup(groupName);

            if (!group)
                group = settings.CreateGroup(groupName,
                    false, false, true, null, typeof(ContentUpdateGroupSchema)
                    , typeof(BundledAssetGroupSchema));

            var assetpath = AssetDatabase.GetAssetPath(obj);
            var guid = AssetDatabase.AssetPathToGUID(assetpath);

            var e = settings.CreateOrMoveEntry(guid, group, false, false);
            var entriesAdded = new List<AddressableAssetEntry> { e };

            group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);

            var assetRef = new AssetReference();
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            assetRef.SetEditorAsset(asset);

            return assetRef;
        }

        public static void DeleteAddressableGroup(string groupName)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings not found!");
                return;
            }

            AddressableAssetGroup groupToDelete = settings.FindGroup(groupName);

            if (groupToDelete == null)
            {
                Debug.LogError($"Group '{groupName}' not found!");
                return;
            }

            settings.RemoveGroup(groupToDelete);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
#endif


    }

}