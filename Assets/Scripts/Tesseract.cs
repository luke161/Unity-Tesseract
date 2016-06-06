using UnityEngine;
using System.Collections;

/// <summary>
/// Renders a 3D projection of a Tesseract.
/// 
/// Based on: http://steve.hollasch.net/thesis/#s2.2
/// 
/// Author: Luke Holland (@luke161)
/// http://lukeholland.me/
/// </summary>

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class Tesseract : MonoBehaviour
{

	[Header("Render Setup")]
	public Transform viewPoint;
	public bool useOrthoProjection = false;
	public float viewingAngle = 25; // for orthographic projection
	public float radius = 2; // for parallel projection

	[Space]
	public Color32[] vertexColors = new Color32[16];

	[Header("Rotatons")]
	public float rotationXY;
	public float rotationYZ;
	public float rotationZX;
	public float rotationXW;
	public float rotationYW;
	public float rotationZW;

	private Vector3[] _vertices;
	private Mesh _mesh;
	private MeshFilter _filter;

	protected void Awake()
	{
		_vertices = new Vector3[UtilsGeom4D.kTesseractPoints.Length];

		_mesh = new Mesh();
		_mesh.subMeshCount = 2;
		_mesh.vertices = _vertices;
		_mesh.colors32 = vertexColors;
		_mesh.MarkDynamic();

		GeneratePlaneMesh(_mesh,_vertices,0);
		GenerateLineMesh(_mesh,_vertices,1);

		_filter = GetComponent<MeshFilter>();
		_filter.mesh = _mesh;
	}

	protected void OnDestroy()
	{
		if(_mesh!=null){
			if(Application.isPlaying) Destroy(_mesh);
			else DestroyImmediate(_mesh);
		}

		_mesh = null;
	}

	public void Update()
	{
		GenerateVertices(_vertices);

		_mesh.vertices = _vertices;
	}

	/// <summary>
	/// Generates the Tesseracts 3D vertices, by taking the 4D points, apply 4D rotations and projecting them into 3D space.
	/// </summary>
	public void GenerateVertices(Vector3[] vertices)
	{
		// setup rotations
		Matrix4x4 matrixXY = UtilsGeom4D.CreateRotationMatrixXY(rotationXY*Mathf.Deg2Rad);
		Matrix4x4 matrixYZ = UtilsGeom4D.CreateRotationMatrixYZ(rotationYZ*Mathf.Deg2Rad);
		Matrix4x4 matrixZX = UtilsGeom4D.CreateRotationMatrixZX(rotationZX*Mathf.Deg2Rad);

		Matrix4x4 matrixXW = UtilsGeom4D.CreateRotationMatrixXW(rotationXW*Mathf.Deg2Rad);
		Matrix4x4 matrixYW = UtilsGeom4D.CreateRotationMatrixYW(rotationYW*Mathf.Deg2Rad);
		Matrix4x4 matrixZW = UtilsGeom4D.CreateRotationMatrixZW(rotationZW*Mathf.Deg2Rad);

		Matrix4x4 matrix = matrixXY*matrixYZ*matrixZX*matrixXW*matrixYW*matrixZW;

		// calculate view point vectors
		Vector3 tp = transform.position;
		Vector3 cp = viewPoint.position;
		Vector3 cu = viewPoint.up;
		Vector3 co = viewPoint.right;

		Vector4 toDir = new Vector4(tp.x,tp.y,tp.z,0);
		Vector4 fromDir = new Vector4(cp.x,cp.y,cp.z,0);
		Vector4 upDir = new Vector4(cu.x,cu.y,cu.z,0);
		Vector4 overDir = new Vector4(co.x,co.y,co.z,0);

		// update mesh vertices based on rotations and view directions
		if(useOrthoProjection)
			UtilsGeom4D.ProjectTo3DParallel(UtilsGeom4D.kTesseractPoints, matrix, ref vertices,radius,fromDir,toDir,upDir,overDir);
		else 
			UtilsGeom4D.ProjectTo3DPerspective(UtilsGeom4D.kTesseractPoints, matrix, ref vertices,viewingAngle,fromDir,toDir,upDir,overDir);
	}

	/// <summary>
	/// Create a submesh to draw the tesseracts outline.
	/// </summary>
	public void GenerateLineMesh(Mesh targetMesh, Vector3[] vertices, int submesh = 0)
	{
		int ti = 0, i = 0, l = UtilsGeom4D.kTesseractPlanes.GetLength(0);
		int[] indices = new int[8*l];
		for(; i<l; ++i){

			indices[ti  ] = UtilsGeom4D.kTesseractPlanes[i,0];
			indices[ti+1] = UtilsGeom4D.kTesseractPlanes[i,1];

			indices[ti+2] = UtilsGeom4D.kTesseractPlanes[i,1];
			indices[ti+3] = UtilsGeom4D.kTesseractPlanes[i,2];

			indices[ti+4] = UtilsGeom4D.kTesseractPlanes[i,2];
			indices[ti+5] = UtilsGeom4D.kTesseractPlanes[i,3];

			indices[ti+6] = UtilsGeom4D.kTesseractPlanes[i,3];
			indices[ti+7] = UtilsGeom4D.kTesseractPlanes[i,0];

			ti += 8;
		}

		targetMesh.SetIndices(indices, MeshTopology.Lines,submesh);
	}

	/// <summary>
	/// Create a submesh to draw the tesseracts planes (faces).
	/// </summary>
	public void GeneratePlaneMesh(Mesh targetMesh, Vector3[] vertices, int submesh = 0)
	{
		int ti = 0, i = 0, l = UtilsGeom4D.kTesseractPlanes.GetLength(0);
		int[] indices = new int[6*l];
		for(; i<l; ++i){

			indices[ti  ] = UtilsGeom4D.kTesseractPlanes[i,0];
			indices[ti+1] = UtilsGeom4D.kTesseractPlanes[i,1];
			indices[ti+2] = UtilsGeom4D.kTesseractPlanes[i,3];

			indices[ti+3] = UtilsGeom4D.kTesseractPlanes[i,1];
			indices[ti+4] = UtilsGeom4D.kTesseractPlanes[i,2];
			indices[ti+5] = UtilsGeom4D.kTesseractPlanes[i,3];

			ti += 6;
		}

		targetMesh.SetIndices(indices,MeshTopology.Triangles,submesh);
	}

}

