using UnityEngine;

/// <summary>
/// Utils for transforming 4D geomtry and static mesh data for a Tesseract.
/// 
/// Based on: http://steve.hollasch.net/thesis/#s2.2
/// 
/// Author: Luke Holland (@luke161)
/// http://lukeholland.me/
/// </summary>
public static class UtilsGeom4D 
{
	
	public readonly static Vector4[] kTesseractPoints = new Vector4[]{
		new Vector4(1,1,1,1),
		new Vector4(1,1,1,-1),
		new Vector4(1,1,-1,1),
		new Vector4(1,1,-1,-1),
		new Vector4(1,-1,1,1),
		new Vector4(1,-1,1,-1),
		new Vector4(1,-1,-1,1),
		new Vector4(1,-1,-1,-1),
		new Vector4(-1,1,1,1),
		new Vector4(-1,1,1,-1),
		new Vector4(-1,1,-1,1),
		new Vector4(-1,1,-1,-1),
		new Vector4(-1,-1,1,1),
		new Vector4(-1,-1,1,-1),
		new Vector4(-1,-1,-1,1),
		new Vector4(-1,-1,-1,-1)
	};

	public readonly static int[,] kTesseractPlanes = new int[23,4]{
		{0,1,5,4},
		{0,2,6,4},
		{0,8,12,4},
		{0,2,3,1},
		{0,1,9,8},
		{0,2,10,8},
		{1,3,7,5},
		{1,9,13,5},
		{1,9,11,3},
		{2,3,7,6},
		{11,10,2,3},
		{2,10,14,6},
		{3,11,15,7},
		{4,12,13,5},
		{4,6,14,12},
		{4,6,7,5},
		{5,7,15,13},
		{7,6,14,15},
		{8,10,14,12},
		{8,9,13,12},
		{9,8,10,11},
		{9,11,15,13},
		{10,11,15,14}
	};

	public static void ProjectTo3DParallel(Vector4[] points, Matrix4x4 pointTransform, ref Vector3[] results, float radius, Vector4 from, Vector4 to, Vector4 up, Vector4 right)
	{
		Vector4 wa,wb,wc,wd;
		CalculateMatrix(out wa, out wb, out wc, out wd,from,to,up,right);

		int i = 0, l = points.Length;
		float s = 1 / radius;

		for(; i<l; ++i){

			Vector4 vp = pointTransform*points[i];
			Vector4 v = vp-from;

			results[i] = new Vector3(
				s*Vector4.Dot(v,wa),
				s*Vector4.Dot(v,wb),
				s*Vector4.Dot(v,wc)
			);
		}
	}

	public static void ProjectTo3DPerspective(Vector4[] points, Matrix4x4 pointTransform, ref Vector3[] results, float viewingAngle, Vector4 from, Vector4 to, Vector4 up, Vector4 right)
	{
		Vector4 wa,wb,wc,wd;
		CalculateMatrix(out wa, out wb, out wc, out wd,from,to,up,right);

		int i = 0, l = points.Length;
		float s = 0, t = 0;

		t = 1 / Mathf.Tan((viewingAngle*Mathf.Deg2Rad)/2);

		for(; i<l; ++i){

			Vector4 vp = pointTransform*points[i];
			Vector4 v = vp-from;
			s = t / Vector4.Dot(v,wd);

			results[i] = new Vector3(
				s*Vector4.Dot(v,wa),
				s*Vector4.Dot(v,wb),
				s*Vector4.Dot(v,wc)
			);
		}
	}

	public static void CalculateMatrix  (out Vector4 Wa,out Vector4 Wb,out Vector4 Wc,out Vector4 Wd, Vector4 from, Vector4 to, Vector4 up, Vector4 right)
	{
		// Get the normalized Wd column-vector.
		Wd = to-from;

		float norm = Norm4(Wd);
		if (norm == 0)
			Debug.LogError ("To point and From point are the same.");

		Wd *= 1/norm;

		// Calculate the normalized Wa column-vector.
		Wa = Cross4 (up,right,Wd);

		norm = Norm4(Wa);
		if (norm == 0)
			Debug.LogError  ("Invalid Up vector.");

		Wa *= 1/norm;

		// Calculate the normalized Wb column-vector.
		Wb = Cross4 (right, Wd, Wa);
		norm = Norm4(Wb);
		if (norm == 0)
			Debug.LogError  ("Invalid Over vector.");
		Wb *= 1/norm;

		// Calculate the Wc column-vector.
		Wc = Cross4 (Wd, Wa, Wb);
	}

	public static Vector4 Cross4 (Vector4 U, Vector4 V, Vector4 W)
	{
		float A, B, C, D, E, F;       

		// Calculate intermediate values.
		A = (V.x * W.y) - (V.y * W.x);
		B = (V.x * W.z) - (V.z * W.x);
		C = (V.x * W.w) - (V.w * W.x);
		D = (V.y * W.z) - (V.z * W.y);
		E = (V.y * W.w) - (V.w * W.y);
		F = (V.z * W.w) - (V.w * W.z);

		// Calculate the result-vector components.
		Vector4 result = Vector4.zero;
		result.x =   (U.y * F) - (U.z * E) + (U.w * D);
		result.y = - (U.x * F) + (U.z * C) - (U.w * B);
		result.z =   (U.x * E) - (U.y * C) + (U.z * A);
		result.w = - (U.x * D) + (U.y * B) - (U.z * A);

		return result;
	}

	public static float Norm4(Vector4 V)
	{
		return Mathf.Sqrt(Vector4.Dot(V,V));
	}

	public static Matrix4x4 CreateRotationMatrixXY(float radians)
	{
		float cos = Mathf.Cos(radians);
		float sin = Mathf.Sin(radians);

		Matrix4x4 mat = new Matrix4x4();
		mat[0,0] = cos;
		mat[0,1] = sin;
		mat[0,2] = 0;
		mat[0,3] = 0;

		mat[1,0] = -sin;
		mat[1,1] = cos;
		mat[1,2] = 0;
		mat[1,3] = 0;

		mat[2,0] = 0;
		mat[2,1] = 0;
		mat[2,2] = 1;
		mat[2,3] = 0;

		mat[3,0] = 0;
		mat[3,1] = 0;
		mat[3,2] = 0;
		mat[3,3] = 1;

		return mat;
	}

	public static Matrix4x4 CreateRotationMatrixYZ(float radians)
	{
		float cos = Mathf.Cos(radians);
		float sin = Mathf.Sin(radians);

		Matrix4x4 mat = new Matrix4x4();
		mat[0,0] = 1;
		mat[0,1] = 0;
		mat[0,2] = 0;
		mat[0,3] = 0;

		mat[1,0] = 0;
		mat[1,1] = cos;
		mat[1,2] = sin;
		mat[1,3] = 0;

		mat[2,0] = 0;
		mat[2,1] = -sin;
		mat[2,2] = cos;
		mat[2,3] = 0;

		mat[3,0] = 0;
		mat[3,1] = 0;
		mat[3,2] = 0;
		mat[3,3] = 1;

		return mat;
	}

	public static Matrix4x4 CreateRotationMatrixZX(float radians)
	{
		float cos = Mathf.Cos(radians);
		float sin = Mathf.Sin(radians);

		Matrix4x4 mat = new Matrix4x4();
		mat[0,0] = cos;
		mat[0,1] = 0;
		mat[0,2] = -sin;
		mat[0,3] = 0;

		mat[1,0] = 0;
		mat[1,1] = 1;
		mat[1,2] = 0;
		mat[1,3] = 0;

		mat[2,0] = sin;
		mat[2,1] = 0;
		mat[2,2] = cos;
		mat[2,3] = 0;

		mat[3,0] = 0;
		mat[3,1] = 0;
		mat[3,2] = 0;
		mat[3,3] = 1;

		return mat;
	}

	public static Matrix4x4 CreateRotationMatrixXW(float radians)
	{
		float cos = Mathf.Cos(radians);
		float sin = Mathf.Sin(radians);

		Matrix4x4 mat = new Matrix4x4();
		mat[0,0] = cos;
		mat[0,1] = 0;
		mat[0,2] = 0;
		mat[0,3] = sin;

		mat[1,0] = 0;
		mat[1,1] = 1;
		mat[1,2] = 0;
		mat[1,3] = 0;

		mat[2,0] = 0;
		mat[2,1] = 0;
		mat[2,2] = 1;
		mat[2,3] = 0;

		mat[3,0] = -sin;
		mat[3,1] = 0;
		mat[3,2] = 0;
		mat[3,3] = cos;

		return mat;
	}

	public static Matrix4x4 CreateRotationMatrixYW(float radians)
	{
		float cos = Mathf.Cos(radians);
		float sin = Mathf.Sin(radians);

		Matrix4x4 mat = new Matrix4x4();
		mat[0,0] = 1;
		mat[0,1] = 0;
		mat[0,2] = 0;
		mat[0,3] = 0;

		mat[1,0] = 0;
		mat[1,1] = cos;
		mat[1,2] = 0;
		mat[1,3] = -sin;

		mat[2,0] = 0;
		mat[2,1] = 0;
		mat[2,2] = 1;
		mat[2,3] = 0;

		mat[3,0] = 0;
		mat[3,1] = sin;
		mat[3,2] = 0;
		mat[3,3] = cos;

		return mat;
	}

	public static Matrix4x4 CreateRotationMatrixZW(float radians)
	{
		float cos = Mathf.Cos(radians);
		float sin = Mathf.Sin(radians);

		Matrix4x4 mat = new Matrix4x4();
		mat[0,0] = 1;
		mat[0,1] = 0;
		mat[0,2] = 0;
		mat[0,3] = 0;

		mat[1,0] = 0;
		mat[1,1] = 1;
		mat[1,2] = 0;
		mat[1,3] = 0;

		mat[2,0] = 0;
		mat[2,1] = 0;
		mat[2,2] = cos;
		mat[2,3] = -sin;

		mat[3,0] = 0;
		mat[3,1] = 0;
		mat[3,2] = sin;
		mat[3,3] = cos;

		return mat;
	}


}

