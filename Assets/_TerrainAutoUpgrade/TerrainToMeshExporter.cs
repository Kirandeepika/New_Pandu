using UnityEngine;
using System.IO;

public class TerrainToMeshExporter : MonoBehaviour
{
    public Terrain terrain;

    [ContextMenu("Export Terrain To OBJ")]
    void Export()
    {
        if (terrain == null)
        {
            Debug.LogError("Assign Terrain!");
            return;
        }

        TerrainData data = terrain.terrainData;
        int w = data.heightmapResolution;
        int h = data.heightmapResolution;

        Vector3 size = data.size;
        float[,] heights = data.GetHeights(0, 0, w, h);

        Vector3[] vertices = new Vector3[w * h];
        int[] triangles = new int[(w - 1) * (h - 1) * 6];

        int index = 0;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float height = heights[y, x];
                vertices[y * w + x] = new Vector3(
                    (float)x / (w - 1) * size.x,
                    height * size.y,
                    (float)y / (h - 1) * size.z
                );
            }
        }

        int ti = 0;
        for (int y = 0; y < h - 1; y++)
        {
            for (int x = 0; x < w - 1; x++)
            {
                int i = y * w + x;

                triangles[ti++] = i;
                triangles[ti++] = i + w;
                triangles[ti++] = i + w + 1;

                triangles[ti++] = i;
                triangles[ti++] = i + w + 1;
                triangles[ti++] = i + 1;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        SaveOBJ(mesh);
    }

    void SaveOBJ(Mesh mesh)
    {
        string path = Application.dataPath + "/terrain.obj";
        using (StreamWriter sw = new StreamWriter(path))
        {
            foreach (Vector3 v in mesh.vertices)
                sw.WriteLine($"v {v.x} {v.y} {v.z}");

            foreach (Vector3 n in mesh.normals)
                sw.WriteLine($"vn {n.x} {n.y} {n.z}");

            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                int a = mesh.triangles[i] + 1;
                int b = mesh.triangles[i + 1] + 1;
                int c = mesh.triangles[i + 2] + 1;
                sw.WriteLine($"f {a}//{a} {b}//{b} {c}//{c}");
            }
        }

        Debug.Log("Terrain exported to: " + path);
    }
}