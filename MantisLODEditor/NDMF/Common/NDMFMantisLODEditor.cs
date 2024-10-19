using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace MantisLODEditor.ndmf
{
    [DisallowMultipleComponent]
    public class NDMFMantisLODEditor : MonoBehaviour, IEditorOnly
    {
        [HideInInspector]
        public (int, int) Triangles = (0, 0);

        [SerializeField]
        private bool protect_boundary = true;
        
        [SerializeField]
        private bool protect_detail = false;
        
        [SerializeField]
        private bool protect_symmetry = false;
        
        [SerializeField]
        private bool protect_normal = false;
        
        [SerializeField]
        private bool protect_shape = true;
        
        [SerializeField]
        private bool use_detail_map = false;
        
        [SerializeField]
        private int detail_boost = 10;
        
        [SerializeField][Range(0, 100)]
        private float quality = 100.0f;
        
        [SerializeField]
        private bool remove_vertex_color = false;

        /// <summary>
        /// メッシュをまとめて投げてMantisLODEditorにデシメートしてもらう
        /// Through meshes and make MantisLODEditor decimate them
        /// </summary>
        /// <param name="_meshes">Renderers and meshes</param>
        /// <returns>triangles</returns>
        public (int, int) Apply(Dictionary<Renderer, Mesh> _meshes = null)
        {
            var meshes = _meshes ?? GetMeshes();
            if (meshes == null)
            {
                Debug.LogError("No mesh found!");
                return default;
            }

            var (originalTriangles, modifiedTriangles) = SimplifyMeshes(meshes, out var updatedMeshes);
            foreach (var updatedMesh in updatedMeshes)
            {
                AssignMesh(updatedMesh.Key, updatedMesh.Value);
            }

            return (originalTriangles, modifiedTriangles);
        }

        public (int, int) SimplifyMeshes(Dictionary<Renderer, Mesh> meshes, out Dictionary<Renderer, Mesh> updatedMeshes)
        {
            var originalTriangles = 0;
            var modifiedTriangles = 0; 

            updatedMeshes = new Dictionary<Renderer, Mesh>();
            foreach (var meshPair in meshes)
            {
                var mantisMeshArray = new[] { new Mantis_Mesh { mesh = Instantiate(meshPair.Value) } };
                originalTriangles += MantisLODEditorUtility.PrepareSimplify(mantisMeshArray);
                MantisLODEditorUtility.Simplify(mantisMeshArray, protect_boundary, protect_detail, protect_symmetry, protect_normal, protect_shape, use_detail_map, detail_boost);
                modifiedTriangles += MantisLODEditorUtility.SetQuality(mantisMeshArray, quality);

                var mesh = mantisMeshArray[0].mesh;
                mesh.name = $"NDMFMantisMesh{mesh.name}";

                if (remove_vertex_color)
                {
                    mesh.colors32 = null;
                }

                updatedMeshes[meshPair.Key] = mesh;
            }
            return (originalTriangles, modifiedTriangles);
        }

        public static void AssignMesh(Renderer renderer, Mesh mesh)
        {
            switch (renderer)
            {
                case MeshRenderer meshrenderer:
                    var meshfilter = meshrenderer.GetComponent<MeshFilter>();
                    if (meshfilter == null) return;
                    meshfilter.sharedMesh = mesh;
                    break;
                case SkinnedMeshRenderer skinnedMeshRenderer:
                    skinnedMeshRenderer.sharedMesh = mesh;
                    break;
            }
        }

        public static Mesh GetMesh(Renderer renderer)
        {
            switch (renderer)
            {
                case MeshRenderer meshrenderer:
                    var meshfilter = meshrenderer.GetComponent<MeshFilter>();
                    return meshfilter?.sharedMesh;
                case SkinnedMeshRenderer skinnedMeshRenderer:
                    return skinnedMeshRenderer.sharedMesh;
                default:
                    return null;
            }
        }

        public Dictionary<Renderer, Mesh> GetMeshes()
        {
            var meshes = new Dictionary<Renderer, Mesh>();
            var renderers = gameObject.GetComponentsInChildren<Renderer>(true);

            foreach (var renderer in renderers)
            {
                var mesh = GetMesh(renderer);
                if (mesh != null)
                {
                    meshes.Add(renderer, mesh);
                }
            }

            return meshes.Count > 0 ? meshes : null;
        }
    }
}