using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using nadena.dev.ndmf.preview;

namespace MantisLODEditor.ndmf
{
    internal class NDMFMantisLODPreview : IRenderFilter
    {
        public static TogglablePreviewNode ToggleNode = TogglablePreviewNode.Create(
            () => "NDMFMantisLODPreview",
            qualifiedName: "MantisLODEditor.ndmf/NDMFMantisLODPreview",
            true
        );
        
        public IEnumerable<TogglablePreviewNode> GetPreviewControlNodes()
        {
            yield return ToggleNode;
        }

        public bool IsEnabled(ComputeContext context)
        {
            return context.Observe(ToggleNode.IsEnabled);
        }

        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            return context.GetComponentsByType<NDMFMantisLODEditor>()
                .Select(mantis => RenderGroup.For(mantis.GetMeshes().Keys).WithData(new[] { mantis }))
                .ToImmutableList();
        }

        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            var mantis = group.GetData<NDMFMantisLODEditor[]>().First();

            context.Observe(mantis);

            var meshes = new Dictionary<Renderer, Mesh>();
            foreach(var pair in proxyPairs)
            {
                var original = pair.Item1;
                var proxy = pair.Item2;

                Mesh mesh = NDMFMantisLODEditor.GetMesh(proxy);
                meshes[original] = mesh;
            }

            mantis.Triangles = mantis.SimplifyMeshes(meshes, out var updatedMeshes);
            
            return Task.FromResult<IRenderFilterNode>(new NDMFMantisLODPreviewNode(updatedMeshes));
        }
    }

    internal class NDMFMantisLODPreviewNode : IRenderFilterNode
    {
        public RenderAspects WhatChanged => RenderAspects.Mesh;
        private Dictionary<Renderer, Mesh> _meshes; 

        public NDMFMantisLODPreviewNode(Dictionary<Renderer, Mesh> meshes)
        {
            _meshes = meshes;
        }
        
        public void OnFrame(Renderer original, Renderer proxy)
        {
            if (_meshes.TryGetValue(original, out var mesh))
            {
                NDMFMantisLODEditor.AssignMesh(proxy, mesh);
            }
        }

    }
}
