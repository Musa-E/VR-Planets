using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component will orbit the attached SgtFloatingPoint around the parent SgtFloatingPoint.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtFloatingPoint))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingOrbit")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Orbit")]
	public class SgtFloatingOrbit : MonoBehaviour
	{
		/// <summary>The radius of the orbit in meters.</summary>
		public SgtLength Radius { set { radius = value; } get { return radius; } } [SerializeField] private SgtLength radius = 1.0f;

		/// <summary>How squashed the orbit is.</summary>
		public float Oblateness { set { oblateness = value; } get { return oblateness; } } [SerializeField] [Range(0.0f, 1.0f)] private float oblateness;

		/// <summary>The local rotation of the orbit in degrees.</summary>
		public Vector3 Tilt { set { tilt = value; } get { return tilt; } } [SerializeField] private Vector3 tilt;

		/// <summary>The local offset of the orbit in meters.</summary>
		public Vector3 Offset { set { offset = value; } get { return offset; } } [SerializeField] private Vector3 offset;

		/// <summary>The current position along the orbit in degrees.</summary>
		public double Angle { set { angle = value; } get { return angle; } } [SerializeField] private double angle;

		/// <summary>The orbit speed.</summary>
		public double DegreesPerSecond { set { degreesPerSecond = value; } get { return degreesPerSecond; } } [SerializeField] private double degreesPerSecond = 10.0f;

		[SerializeField]
		private SgtFloatingPoint parentPoint;

		[System.NonSerialized]
		private SgtFloatingPoint cachedPoint;

		[System.NonSerialized]
		private bool cachedPointSet;

		/// <summary>The center orbit point. NOTE: This should be null/None if it will be spawned by SgtFloatingSpawnerOrbit.</summary>
		public SgtFloatingPoint ParentPoint
		{
			set
			{
				if (parentPoint != value)
				{
					UnregisterParentPoint();

					parentPoint = value;

					RegisterParentPoint();
				}
			}

			get
			{
				return parentPoint;
			}
		}

		public void RegisterParentPoint()
		{
			if (parentPoint != null)
			{
				parentPoint.OnPositionChanged += ParentPositionChanged;
			}
		}

		public void UnregisterParentPoint()
		{
			if (parentPoint != null)
			{
				parentPoint.OnPositionChanged -= ParentPositionChanged;
			}
		}

		public static SgtPosition CalculatePosition(SgtFloatingPoint parentPoint, double radius, double angle, Vector3 tilt, Vector3 offset, float oblateness)
		{
			if (parentPoint != null)
			{
				var rotation = parentPoint.transform.rotation * Quaternion.Euler(tilt);
				var r1       = radius;
				var r2       = radius * (1.0f - oblateness);
				var localX   = offset.x + System.Math.Sin(angle * Mathf.Deg2Rad) * r1;
				var localY   = offset.y + 0.0;
				var localZ   = offset.z + System.Math.Cos(angle * Mathf.Deg2Rad) * r2;

				//Rotate(rotation, ref localX, ref localY, ref localZ);
				var mat = Rotate2(rotation);
				var loc = Unity.Mathematics.math.mul(mat, new Unity.Mathematics.double3(localX, localY, localZ));
				localX = loc.x;
				localY = loc.y;
				localZ = loc.z;

				var position = parentPoint.Position;

				position.LocalX += localX;
				position.LocalY += localY;
				position.LocalZ += localZ;
				position.SnapLocal();

				return position;
			}

			return default(SgtPosition);
		}

		[ContextMenu("Update Orbit")]
		public void UpdateOrbit()
		{
			cachedPoint.SetPosition(CalculatePosition(ParentPoint, radius, angle, tilt, offset, oblateness));
		}

		public static void Rotate(Quaternion q, ref double x, ref double y, ref double z)
		{
			var tx = 2.0f * (q.y * z - q.z * y);
			var ty = 2.0f * (q.z * x - q.x * z);
			var tz = 2.0f * (q.x * y - q.y * x);

			x = x + q.w * tx + (q.y * tz - q.z * tx);
			y = y + q.w * ty + (q.z * tx - q.z * tz);
			z = z + q.w * tz + (q.x * ty - q.z * ty);
		}

		public static Unity.Mathematics.double3x3 Rotate2(Quaternion q)
		{
			var num = q.x * 2.0;
			var num2 = q.y * 2.0;
			var num3 = q.z * 2.0;
			var num4 = q.x * num;
			var num5 = q.y * num2;
			var num6 = q.z * num3;
			var num7 = q.x * num2;
			var num8 = q.x * num3;
			var num9 = q.y * num3;
			var num10 = q.w * num;
			var num11 = q.w * num2;
			var num12 = q.w * num3;
			Unity.Mathematics.double3x3 result = default(Unity.Mathematics.double3x3);
			result.c0.x = 1f - (num5 + num6);
			result.c0.y = num7 + num12;
			result.c0.z = num8 - num11;

			result.c1.x = num7 - num12;
			result.c1.y = 1f - (num4 + num6);
			result.c1.z = num9 + num10;

			result.c2.x = num8 + num11;
			result.c2.y = num9 - num10;
			result.c2.z = 1f - (num4 + num5);
			return result;
		}

		protected virtual void OnEnable()
		{
			if (cachedPointSet == false)
			{
				cachedPoint    = GetComponent<SgtFloatingPoint>();
				cachedPointSet = true;
			}

#if UNITY_EDITOR
			if (parentPoint == null)
			{
				var parent = transform.parent;

				if (parent != null)
				{
					parentPoint = GetComponent<SgtFloatingPoint>();
				}
			}
#endif

			RegisterParentPoint();
		}

		protected virtual void OnDisable()
		{
			UnregisterParentPoint();
		}

		protected virtual void LateUpdate()
		{
			if (Application.isPlaying == true)
			{
				angle += degreesPerSecond * Time.deltaTime;

				angle = Repeat(angle, 360.0f);
			}

			UpdateOrbit();
		}

		private void ParentPositionChanged()
		{
			UpdateOrbit();
		}

		private static double Repeat(double t, double length)
		{
			return System.Math.Clamp(t - System.Math.Floor(t / length) * length, 0.0, length);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingOrbit;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingOrbit_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var updateOrbit = false;

			BeginError(Any(tgts, t => t.ParentPoint == null));
				Each(tgts, t => t.UnregisterParentPoint());
					Draw("parentPoint", ref updateOrbit, "The point this orbit will go around. NOTE: This should be null/None if it will be spawned by SgtFloatingSpawnerOrbit.");
				Each(tgts, t => t.RegisterParentPoint());
			EndError();
			if (Any(tgts, t => t.ParentPoint == null))
			{
				Info("ParentPoint should only be None/null if this prefab will be spawned from the SgtFloatingSpawnerOrbit component. If not, you should add one to the parent GameObject.");
			}
			Draw("radius", ref updateOrbit, "The radius of the orbit in meters.");
			Draw("oblateness", ref updateOrbit, "How squashed the orbit is.");
			Draw("tilt", ref updateOrbit, "The local rotation of the orbit in degrees.");
			Draw("offset", ref updateOrbit, "The local offset of the orbit in meters.");
			Draw("angle", ref updateOrbit, "The current position along the orbit in degrees.");
			Draw("degreesPerSecond", "The orbit speed.");

			if (updateOrbit == true)
			{
				Each(tgts, t => t.UpdateOrbit(), true, true);
			}
		}
	}
}
#endif