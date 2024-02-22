using System.Collections.Generic;
using System.Text.RegularExpressions;
using Moonflow;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class MFAssetRenameTool : EditorWindow
{
    internal static MFAssetRenameTool s_Instance;

    public Object folder;
    public bool forceNumberEnd;
    private Object _folder;
    private bool _singleMode;
    private int _replaceSpace;
    private ReplaceString[] _parts;
    private List<Object> _searchResult;
    private string[] _previewNames;

    private int _index;
    private bool _foldout = true;
    private GUIStyle _titleStyle;
    private string _message = "";

    private static string[] _replaceSpaceMode = { "不替换", "删除并使下一单词首字母大写", "删除", "使用_替换" };

    [MenuItem("Moonflow/Tools/Assets/资源重命名")]
    static void ShowWindow()
    {
        s_Instance = GetWindow<MFAssetRenameTool>("Moonflow Asset Rename Tool");
        s_Instance.Show();
        s_Instance.Init();
    }

    public void Init()
    {
        _parts = new ReplaceString[]
        {
            new ReplaceString("前缀(匹配)"),
            new ReplaceString("主体名(包含)"),
            new ReplaceString("后缀(匹配)")
        };
        _titleStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState()
            {
                textColor = Color.white
            },
            fontSize = 14
        };
    }

    private void OnGUI()
    {
        using (new EditorGUILayout.VerticalScope(new GUIStyle() { fixedWidth = 605 }))
        {
            using (new EditorGUILayout.HorizontalScope("box"))
            {
                folder = EditorGUILayout.ObjectField("目标文件夹", folder, typeof(Object), false, GUILayout.MaxWidth(600));
            }

            using (new EditorGUILayout.HorizontalScope("box"))
            {
                using (new EditorGUILayout.VerticalScope(new GUIStyle("box") { fixedWidth = 70 }))
                {
                    EditorGUILayout.LabelField("");
                    EditorGUILayout.LabelField("旧名");
                    // EditorGUILayout.LabelField("包含");
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("新名");
                    EditorGUILayout.LabelField("空置替换");
                }

                for (int i = 0; i < 3; i++)
                {
                    using (new EditorGUILayout.VerticalScope(new GUIStyle("box") { fixedWidth = 170 }))
                    {
                        EditorGUILayout.LabelField(_parts[i].partName, _titleStyle);
                        _parts[i].oldString = EditorGUILayout.TextField(_parts[i].oldString);
                        // if(i == 1)_parts[i].contains = EditorGUILayout.Toggle(_parts[i].contains);
                        EditorGUILayout.Space();
                        _parts[i].newString = EditorGUILayout.TextField(_parts[i].newString);
                        _parts[i].emptyReplace = EditorGUILayout.Toggle(_parts[i].emptyReplace);
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _replaceSpace = EditorGUILayout.Popup("空格替换方式", _replaceSpace, _replaceSpaceMode);
            }
            EditorGUILayout.Space(10);

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("开始搜索"))
                {
                    Search();
                }

                if (GUILayout.Button("预览替换名"))
                {
                    CreateReplacedName();
                }
                if (GUILayout.Button("执行替换"))
                {
                    Tips();
                }
            }

            if (_searchResult != null && _searchResult.Count > 0)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope(new GUIStyle("box") { fixedWidth = 350 }))
                    {
                        EditorGUILayout.LabelField("搜索结果", _titleStyle);
                        MFEditorUI.DrawFlipList(_searchResult, ref _index, ref _foldout, 20);
                    }
                    using (new EditorGUILayout.VerticalScope(new GUIStyle("box") { fixedWidth = 250 }))
                    {
                        EditorGUILayout.LabelField("替换结果", _titleStyle);
                        EditorGUILayout.LabelField("");
                        int start = _index * 20;
                        for (int i = start; i < ((start + 20 > _searchResult.Count) ? _searchResult.Count: _index + 20); i++)
                        {
                            EditorGUILayout.LabelField(_previewNames[i]);
                        }
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope("box"))
            {
                if(_message != "")EditorGUILayout.HelpBox(_message, MessageType.Error);
            }
        }
    }

    private void Search()
    {
        _folder = folder;
        if (_folder != null)
        {
            _searchResult = new List<Object>();
            var pathFolder = AssetDatabase.GetAssetPath(_folder);
            string filter = "";
            for (int i = 0; i < 3; i++)
            {
                if (_parts[i].oldString != "")
                {
                    filter += $"{_parts[i].oldString} ";
                }
            }
            // if (filter == "")
            // {
            //     _message = "没有任何筛选条件！";
            //     return;
            // }
            var assetPaths = AssetDatabase.FindAssets(filter, new[] {pathFolder});
            for (int i = 0; i < assetPaths.Length; i++)
            {
                _searchResult.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetPaths[i]),
                    typeof(Object)));
            }
            CreateReplacedName();
        }
        else
        {
            _message = "未选择查找路径";
        }
    }

    private void CreateReplacedName()
    {
        _previewNames = new string[_searchResult.Count];
        for (int i = 0; i < _searchResult.Count; i++)
        {
            _previewNames[i] = _searchResult[i].name;
            if (_parts[0].newString != "" || _parts[0].emptyReplace)
            {
                if (_parts[0].oldString != "")
                {
                    _previewNames[i] = _previewNames[i].Replace(_parts[0].oldString, _parts[0].newString);
                }
                else
                {
                    _previewNames[i] = _previewNames[i].Insert(0, _parts[0].newString);
                }
            }
            if (_parts[1].newString != ""|| _parts[1].emptyReplace)
            {
                if (_parts[1].oldString != "")
                {
                    _previewNames[i] = _previewNames[i].Replace(_parts[1].oldString, _parts[1].newString);
                }
                // else
                // {
                //     _previewNames[i] = _previewNames[i].Insert(0, _parts[1].newString);
                // }
            }
            if (_parts[2].newString != "" || _parts[2].emptyReplace)
            {
                if (_parts[2].oldString != "")
                {
                    _previewNames[i] = _previewNames[i].Replace(_parts[2].oldString, _parts[2].newString);
                }
                else
                {
                    _previewNames[i] += _parts[2].newString;
                }
            }

            switch (_replaceSpace)
            {
                case 0: break;
                case 1:
                {
                    Regex reg = new Regex(" [a-z]");//这是搜索匹配0-9的数字
                    var col = reg.Matches(_previewNames[i]);
                    for (int j = 0; j < col.Count; j++)
                    {
                        _previewNames[i] = Regex.Replace(_previewNames[i], col[j].Value, col[j].Value.ToUpper());
                    }
                    _previewNames[i] = _previewNames[i].Replace(" ", "");
                }
                    continue;
                case 2: _previewNames[i] = _previewNames[i].Replace(" ", "");
                    break;
                case 3: _previewNames[i] = _previewNames[i].Replace(" ", "_");
                    break;
            }
        }
    }

    private void Tips()
    {
        // int returnNum = MFMessageBox.MessageBox(IntPtr.Zero, "确定替换吗(此操作不可逆)", "批量替换资产名", 1);
        if (EditorUtility.DisplayDialog("批量替换资产名", "确定替换吗(此操作不可逆)", "确定", "取消"))
        {
            Replace();
        }
    }

    private void Replace()
    {
        for (int i = 0; i < _searchResult.Count; i++)
        {
            var pathName = AssetDatabase.GetAssetPath(_searchResult[i]);
            AssetDatabase.RenameAsset(pathName, _previewNames[i]);
        }
    }
    public struct ReplaceString
    {
        public string partName;
        public string oldString;
        public bool emptyReplace;
        // public bool contains;
        public string newString;

        public ReplaceString(string name)
        {
            partName = name;
            oldString = "";
            emptyReplace = false;
            // contains = false;
            newString = "";
        }
    }
}
