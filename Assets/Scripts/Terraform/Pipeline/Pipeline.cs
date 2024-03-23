using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

[CreateAssetMenu(menuName = "Terraform/Pipeline")]
public class Pipeline : NodeGraph {

    public T GetNode<T>() where T : Node {
        foreach (Node node in nodes) {
            if (node is T) return (T)node;
        }
        return null;
    }
    public Texture2D GetFinalTexture() {
        EndNode node = GetNode<EndNode>();
        if (node == null) return null;
        return node.GetTexture();
    }

    public void Place(CustomTerrain ct, SurfacePointToWorldPos f) {
        PlacementNode placer = GetNode<PlacementNode>();
        if (placer == null) return;
        placer.Place(name, ct, f);
    }

    public void SetSeed(int seed) {
        // Sets seed for random interfaces and updates outputs and previews.
        foreach (Node node in nodes) {
            if (node is IRandomNode) {
                IRandomNode randomNode = (IRandomNode)node;
                randomNode.SetSeed(seed);
            }
            NotifyEditor(node);
        }
    }

    public void SetSize(int size) {
        foreach (Node node in nodes) {
            if (node is ISizeNode) {
                ISizeNode sizeNode = (ISizeNode)node;
                sizeNode.SetSize(size);
            }
            NotifyEditor(node);
        }
    }

    public List<GameObject> GetInstantiatedObjects() {
        GameObject root = GameObject.Find(name);
        return root == null ? new List<GameObject>() : root.GetChildren().ToList<GameObject>();
    }

    private void NotifyEditor(Node node) {
        if (node is BiomeNode) {
            BiomeNode biomeNode = (BiomeNode)node;
            biomeNode.OnInputChanged();
        }
        if (node is SourceNode) {
            // Trick until https://github.com/Siccity/xNode/pull/243 is merged
            SourceNode sourceNode = (SourceNode)node;
            sourceNode.dirty = true;
        }
    }
}
