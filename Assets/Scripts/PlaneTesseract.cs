using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlaneTesseract : MonoBehaviour
{
    MeshFilter[] meshFilters;
    [SerializeField, Range(0, 360)]
    public float rotationXY, rotationYZ, rotationZX, rotationXW, rotationYW, rotationZW;
    public Transform viewPoint;



    void Awake()
    {
        meshFilters = new MeshFilter[UtilsGeom4D.kTesseractPlanes.GetLength(0)];
        int[] tris = { 0, 1, 3, 3, 1, 2 };

        Vector3[] normals = new Vector3[4] {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0)
        };
        Shader shader = Shader.Find("Standard");
        for (int i = 0; i < meshFilters.Length; i++)
        {
            GameObject child = new GameObject($"Plane_{i}");
            child.transform.parent = transform;
            meshFilters[i] = child.AddComponent<MeshFilter>();
            MeshRenderer renderer = child.AddComponent<MeshRenderer>();
            Material mat = new Material(shader);
            mat.color = Color.HSVToRGB((i * 1f) / meshFilters.Length, 1, 1);
            renderer.material = mat;
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = tris;
            mesh.uv = uv;
            meshFilters[i].mesh = mesh;
        }
        Project();
    }

    void Update()
    {
        if (viewPoint.hasChanged) Project();
    }

    void OnValidate()
    {
        if (Application.isPlaying) Project();

    }


    void Project()
    {
        Camera cam = Camera.main;
        float viewingAngle = cam.fieldOfView;

        Matrix4x4 matrixXY = UtilsGeom4D.CreateRotationMatrixXY(rotationXY * Mathf.Deg2Rad);
        Matrix4x4 matrixYZ = UtilsGeom4D.CreateRotationMatrixYZ(rotationYZ * Mathf.Deg2Rad);
        Matrix4x4 matrixZX = UtilsGeom4D.CreateRotationMatrixZX(rotationZX * Mathf.Deg2Rad);

        Matrix4x4 matrixXW = UtilsGeom4D.CreateRotationMatrixXW(rotationXW * Mathf.Deg2Rad);
        Matrix4x4 matrixYW = UtilsGeom4D.CreateRotationMatrixYW(rotationYW * Mathf.Deg2Rad);
        Matrix4x4 matrixZW = UtilsGeom4D.CreateRotationMatrixZW(rotationZW * Mathf.Deg2Rad);

        Matrix4x4 matrix = matrixXY * matrixYZ * matrixZX * matrixXW * matrixYW * matrixZW;

        // calculate view point vectors
        Vector3 tp = transform.position;
        Vector3 cp = viewPoint.position;
        Vector3 cu = viewPoint.up;
        Vector3 co = viewPoint.right;

        Vector4 toDir = new Vector4(tp.x, tp.y, tp.z, 0);
        Vector4 fromDir = new Vector4(cp.x, cp.y, cp.z, 0);
        Vector4 upDir = new Vector4(cu.x, cu.y, cu.z, 0);
        Vector4 overDir = new Vector4(co.x, co.y, co.z, 0);

        for (int i = 0; i < meshFilters.Length; i++)
        {
            Vector4[] points = new Vector4[4];
            for (int p = 0; p < 4; p++)
            {
                points[p] = UtilsGeom4D.kTesseractPoints[UtilsGeom4D.kTesseractPlanes[i, p]];
            }
            Vector3[] vertices = new Vector3[4];
            UtilsGeom4D.ProjectTo3DPerspective(points, matrix, ref vertices, viewingAngle, fromDir, toDir, upDir, overDir);
            meshFilters[i].mesh.vertices = vertices;
        }
    }

    async void OnApplicationQuit()
    {
        foreach (Transform child in transform)
        {
            Destroy(child);
        }
    }
}
