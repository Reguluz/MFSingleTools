using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MFNewVersionObjDataDeliver : EditorWindow
{
    public GameObject m_sourceObj;
    public GameObject m_targetObj;
    
    [MenuItem("Moonflow/Tools/Assets/MFNewVersionObjDataDeliver")]
    static void Init()
    {
        MFNewVersionObjDataDeliver window = (MFNewVersionObjDataDeliver)EditorWindow.GetWindow(typeof(MFNewVersionObjDataDeliver));
        window.Show();
    }
    
    void OnGUI()
    {
        m_sourceObj = (GameObject)EditorGUILayout.ObjectField("Source Object", m_sourceObj, typeof(GameObject), true);
        m_targetObj = (GameObject)EditorGUILayout.ObjectField("Target Object", m_targetObj, typeof(GameObject), true);
        if (GUILayout.Button("Deliver Material"))
        {
            DeliverMaterial();
        }

        if (GUILayout.Button("Deliver Components"))
        {
            DeliverComponent();
        }
    }

    private void DeliverMaterial()
    {
        if (m_sourceObj == null || m_targetObj == null)
        {
            return;
        }
        Renderer[] sourceMeshRenderers = m_sourceObj.GetComponentsInChildren<Renderer>();
        Renderer[] targetMeshRenderers = m_targetObj.GetComponentsInChildren<Renderer>();
        if (sourceMeshRenderers.Length != targetMeshRenderers.Length)
        {
            Debug.LogError("Source and Target MeshRenderer count not equal");
            return;
        }
        Dictionary<string, Renderer> targetMeshRendererDic = new Dictionary<string, Renderer>();
        foreach (var targetMeshRenderer in targetMeshRenderers)
        {
            targetMeshRendererDic.Add(targetMeshRenderer.name, targetMeshRenderer);
        }
        foreach (var sourceMeshRenderer in sourceMeshRenderers)
        {
            if (targetMeshRendererDic.TryGetValue(sourceMeshRenderer.name, out var value))
            {
                value.sharedMaterials = sourceMeshRenderer.sharedMaterials;
            }
        }
    }

    private void GetAllTransform(ref Dictionary<string, Transform> transTable, Transform obj, string parentName)
    {
        string name = $"{parentName}/{obj.name}";
        transTable.Add(name, obj);
        if (obj.childCount > 0)
        {
            for (int i = 0; i < obj.childCount; i++)
            {
                GetAllTransform(ref transTable, obj.GetChild(i), name);
            }
        }
    }

    private void DeliverComponent()
    {
        Dictionary<string, Transform> srcTransDict = new Dictionary<string, Transform>();
        Dictionary<string, Transform> dstTransDict = new Dictionary<string, Transform>();
        GetAllTransform(ref srcTransDict, m_sourceObj.transform, m_sourceObj.name);
        GetAllTransform(ref dstTransDict, m_targetObj.transform, m_targetObj.name);
        foreach (var pair in dstTransDict)
        {
            if (srcTransDict.TryGetValue(pair.Key, out Transform srcTrans))
            {
                DeliverComponent(srcTrans, pair.Value);
            }
            else
            {
                Debug.LogError($"原对象不存在路径{pair.Key}");
            }
        }
    }

    private void DeliverComponent(Transform src, Transform dst)
    {
        Component[] src_Comp = src.GetComponents(typeof(Component));
        Component[] dest_Comp = dst.GetComponents(typeof(Component));
        foreach (var t in dest_Comp)
        {
            if (t is not Transform)
            {
                DestroyImmediate(t);
            }
        }

        foreach (var comp in src_Comp)
        {
            Type type = comp.GetType();
            Component dstComp;
            if (type != typeof(Transform))
            {
                dstComp = dst.gameObject.AddComponent(type);
            }
            else
            {
                dstComp = dst.GetComponent(type);
            }
            // EditorUtility.CopySerializedManagedFieldsOnly(comp, dstComp);
            EditorUtility.CopySerialized(comp, dstComp);
        }
    }
}
