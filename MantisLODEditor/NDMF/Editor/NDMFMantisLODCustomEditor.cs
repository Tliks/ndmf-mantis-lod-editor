using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using nadena.dev.ndmf.preview;

namespace MantisLODEditor.ndmf
{
    [CustomEditor(typeof(NDMFMantisLODEditor))]
    public class NDMFMantisLODCustomEditor : Editor
    {
        private NDMFMantisLODEditor m_target;
        private TogglablePreviewNode m_toggleNode;

        //Mantis Editor Parameters
        private SerializedProperty m_protectBoundary;
        private SerializedProperty m_protectDetail;
        private SerializedProperty m_protectSymmetry;
        private SerializedProperty m_protectNormal;
        private SerializedProperty m_protectShape;
        private SerializedProperty m_useDetailMap;
        private SerializedProperty m_detailBoost;
        private SerializedProperty m_quality;
        
        //NDMF Mantis Parameters
        private SerializedProperty m_removeVertexColor;

        private void OnEnable()
        {
            m_protectBoundary = serializedObject.FindProperty("protect_boundary");
            m_protectDetail = serializedObject.FindProperty("protect_detail");
            m_protectSymmetry = serializedObject.FindProperty("protect_symmetry");
            m_protectNormal = serializedObject.FindProperty("protect_normal");
            m_protectShape = serializedObject.FindProperty("protect_shape");
            m_useDetailMap = serializedObject.FindProperty("use_detail_map");
            m_detailBoost = serializedObject.FindProperty("detail_boost");
            m_quality = serializedObject.FindProperty("quality");
            m_removeVertexColor = serializedObject.FindProperty("remove_vertex_color");
            
            m_target = target as NDMFMantisLODEditor;
            m_toggleNode = NDMFMantisLODPreview.ToggleNode;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(m_protectBoundary, new GUIContent("Protect Boundary"));
            EditorGUILayout.PropertyField(m_protectDetail, new GUIContent("More Details"));
            EditorGUILayout.PropertyField(m_protectSymmetry, new GUIContent("Protect Symmetry"));
            EditorGUILayout.PropertyField(m_protectNormal, new GUIContent("Protect Hard Edge"));
            EditorGUILayout.PropertyField(m_protectShape, new GUIContent("Beautiful Triangles"));
            EditorGUILayout.PropertyField(m_useDetailMap, new GUIContent("Use Detail Map"));
            EditorGUILayout.PropertyField(m_detailBoost, new GUIContent("Detail Boost"));
            EditorGUILayout.PropertyField(m_removeVertexColor, new GUIContent("Remove Vertex Color After Optimize"));
            EditorGUILayout.PropertyField(m_quality, new GUIContent("Quality"));
            serializedObject.ApplyModifiedProperties();
            
            if (GUILayout.Button(m_toggleNode.IsEnabled.Value ? "Stop Preview" : "Preview"))
            {
                m_toggleNode.IsEnabled.Value = !m_toggleNode.IsEnabled.Value;
            }

            if (m_toggleNode.IsEnabled.Value)
            {
                EditorGUILayout.LabelField($"Triangles", $"{m_target.Triangles.Item2}/{m_target.Triangles.Item1}");
            }
            else
            {
                EditorGUILayout.LabelField($"Triangles", $"- / - (works during only Preview)");
            }
        }

    }
}