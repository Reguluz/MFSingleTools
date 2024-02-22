using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Moonflow.MFAssetTools
{
    public class MFSubAssetCleaner : EditorWindow
    {
        public Object asset;
        private int index;
        [MenuItem("Moonflow/Tools/Remove SubAssets")]
        public static void openwindow()
        {
            var window = GetWindow<MFSubAssetCleaner>();
            window.Show();
        }

        private void OnGUI()
        {
            asset = EditorGUILayout.ObjectField(asset, typeof(Object), true);
            MFEditorUI.DivideLine(Color.grey);
            //show sub asset list
            if (asset != null)
            {
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));
                if (subAssets != null)
                {
                    MFEditorUI.DrawFlipList<Object>(DrawSubItem, subAssets.ToList(), ref index, 10);
                }
            }
        }
        private void DrawSubItem(Object item, int index)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.ObjectField(item, typeof(Object), true);
                if (GUILayout.Button("Remove"))
                {
                    AssetDatabase.RemoveObjectFromAsset(item);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}