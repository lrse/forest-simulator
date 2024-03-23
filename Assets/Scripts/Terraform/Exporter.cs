using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Exporter : System.IDisposable {
    private StreamWriter writer;
    private string path;
    public Exporter() {
        this.path = GetUniquePath();
        Directory.CreateDirectory(Path.GetDirectoryName(this.path));
        this.writer = new StreamWriter(this.path);
    }

    public void Dispose() {
        Debug.Log($"Exported to file {this.path}");
        writer?.Close();
    }

    private string GetUniquePath() {
        return Path.Combine(new string[] {
            Application.persistentDataPath, "Exporter",
            $"{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}.csv"
        });
    }

    public void WriteVertices(List<GameObject> gos) {
        foreach (GameObject go in gos) {
            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf != null) {
                // Mesh is in the same object
                _WriteVertices(mf, go.tag);
            } else {
                // Submeshes
                foreach (GameObject child in go.GetChildren()) {
                    mf = child.GetComponent<MeshFilter>();
                    if (mf != null) {
                        _WriteVertices(mf, child.tag);
                    }
                }
            }
        }
    }

    public void WriteVertices(Vector3[] positions, string tag) {
        int uid = GetIndexedLabelByTag(tag);
        foreach (Vector3 p in positions) {
            writer.WriteLine($"{p.x};{p.y};{p.z};{uid}");
        }
    }

    private void _WriteVertices(MeshFilter mf, string tag) {
        int uid = GetIndexedLabelByTag(tag);
        Vector3[] verticesWS = mf.sharedMesh.vertices;
        mf.transform.TransformPoints(verticesWS);
        foreach (Vector3 v in verticesWS) {
            writer.WriteLine($"{v.x};{v.y};{v.z};{uid}");
        }
    }

    private int GetIndexedLabelByTag(string tag) =>
        tag switch {
            "Terrain" => 0,
            "Trunk" => 1,
            "Canopy" => 2,
            "Branches" => 3,
            "Bushes" => 4,
            "Understorey" => 5,
            "Grass" => 6,
            "Cactae" => 7,
            "Deadwood" => 8,
            _ => 9
        };
}
