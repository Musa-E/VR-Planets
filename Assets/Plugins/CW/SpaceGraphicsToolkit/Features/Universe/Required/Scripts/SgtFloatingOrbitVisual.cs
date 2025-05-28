using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component draws an orbit in 3D space.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingOrbitVisual")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Orbit Visual")]
	public class SgtFloatingOrbitVisual : MonoBehaviour
	{
		/// <summary>The orbit that will be rendered by this component.</summary>
		public SgtFloatingOrbit Orbit { set { orbit = value; } get { return orbit; } } [SerializeField] private SgtFloatingOrbit orbit;

		/// <summary>The material of the orbit.</summary>
		public Material Material { set { material = value; } get { return material; } } [SerializeField] private Material material;

		/// <summary>The thickness of the visual ring in local space.</summary>
		public SgtLength Thickness { set { thickness = value; } get { return thickness; } } [SerializeField] private SgtLength thickness = 100000.0f;

		/// <summary>The amount of points used to draw the orbit.</summary>
		public int Points { set { points = value; } get { return points; } } [SerializeField] private int points = 360;

		/// <summary>The color of the orbit ring as it goes around the orbit.</summary>
		public Gradient Colors { get { if (colors == null) colors = new Gradient(); return colors; } } [SerializeField] private Gradient colors;

		[System.NonSerialized]
		private Mesh visualMesh;

		[System.NonSerialized]
		private List<Vector3> meshPositions = new List<Vector3>(360 * 2);

		[System.NonSerialized]
		private List<Vector2> meshCoords = new List<Vector2>(360 * 2);

		[System.NonSerialized]
		private List<Color> meshColors = new List<Color>(360 * 2);

		[System.NonSerialized]
		private List<int> meshIndices = new List<int>(360 * 6);

		[System.NonSerialized]
		private float orbitThickness;

		[System.NonSerialized]
		private float orbitOblateness;

		[System.NonSerialized]
		private float orbitRadius;

		[System.NonSerialized]
		private float orbitAngle;

		protected virtual void OnEnable()
		{
			SgtCamera.OnCameraDraw += HandleCameraDraw;
		}

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraDraw -= HandleCameraDraw;
		}

		private void HandleCameraDraw(Camera camera)
		{
			if (SgtCommon.CanDraw(gameObject, camera) == false) return;

			if (orbit != null && points > 2)
			{
				var floatingCamera = default(SgtFloatingCamera);

				if (SgtFloatingCamera.TryGetInstance(ref floatingCamera) == true)
				{
					if (visualMesh == null)
					{
						visualMesh = SgtCommon.CreateTempMesh("Orbit Visual");
					}

					var dirtyPoints = meshPositions.Count != points * 2 || orbitOblateness != orbit.Oblateness || orbitRadius != (float)orbit.Radius || orbitThickness != (float)thickness;
					var dirtyCoords = dirtyPoints == true || orbitAngle != (float)orbit.Angle;

					if (dirtyPoints == true)
					{
						orbitOblateness = orbit.Oblateness;
						orbitThickness  = (float)thickness;
						orbitRadius     = (float)orbit.Radius;

						var r1   = orbitRadius;
						var r2   = orbitRadius * (1.0f - orbitOblateness);
						var step = 360.0f / points;

						meshPositions.Clear();
						meshIndices.Clear();

						for (var i = 0; i < points; i++)
						{
							var angle = -(i * step) * Mathf.Deg2Rad;
							var sin   = Mathf.Sin(angle);
							var cos   = Mathf.Cos(angle);
							var off   = new Vector3(sin, 0.0f, cos) * orbitThickness;
							var point = new Vector3(sin * r1, 0.0f, cos * r2);

							meshPositions.Add(point - off);
							meshPositions.Add(point + off);
						}

						for (var i = 0; i < points; i++)
						{
							var indexA = i * 2 + 0;
							var indexB = i * 2 + 1; 
							var indexC = i * 2 + 2; indexC %= points * 2;
							var indexD = i * 2 + 3; indexD %= points * 2;

							meshIndices.Add(indexA);
							meshIndices.Add(indexB);
							meshIndices.Add(indexC);
							meshIndices.Add(indexD);
							meshIndices.Add(indexC);
							meshIndices.Add(indexB);
						}

						visualMesh.Clear();
						visualMesh.SetVertices(meshPositions);
						visualMesh.SetTriangles(meshIndices, 0);
						visualMesh.RecalculateBounds();
					}

					if (dirtyCoords == true)
					{
						orbitAngle = (float)orbit.Angle;

						var step = 1.0f / (points - 1);
						var off  = (float)orbit.Angle / 360.0f;

						meshCoords.Clear();
						meshColors.Clear();

						for (var i = 0; i < points; i++)
						{
							var u     = off + step * i;
							var color = colors.Evaluate(Mathf.Repeat(u, 1.0f));

							meshCoords.Add(new Vector2(u, 0.0f));
							meshCoords.Add(new Vector2(u, 1.0f));

							meshColors.Add(color);
							meshColors.Add(color);
						}

						visualMesh.SetUVs(0, meshCoords);
						visualMesh.SetColors(meshColors);
					}

					if (visualMesh != null)
					{
						var translation = Matrix4x4.Translate(orbit.ParentPoint.transform.position);
						var rotation    = Matrix4x4.Rotate(orbit.ParentPoint.transform.rotation * Quaternion.Euler(orbit.Tilt));
						var offset      = Matrix4x4.Translate(orbit.Offset);

						Graphics.DrawMesh(visualMesh, translation * rotation * offset, material, gameObject.layer, camera);
					}
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingOrbitVisual;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingOrbitVisual_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Orbit == null));
				Draw("orbit", "The orbit that will be rendered by this component.");
			EndError();
			BeginError(Any(tgts, t => t.Material == null));
				Draw("material", "The material of the orbit.");
			EndError();
			Draw("thickness", "The thickness of the visual ring in local space.");
			Draw("points", "The amount of points used to draw the orbit.");
			Draw("colors", "The color of the orbit ring as it goes around the orbit.");
		}
	}
}
#endif