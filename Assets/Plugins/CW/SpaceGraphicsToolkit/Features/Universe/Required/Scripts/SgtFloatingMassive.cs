using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component works like <b>SgtFloatingObject</b>, but it will scale the object toward the camera if it's too far away to render.
	/// NOTE: This component scales based on the <b>SgtFloatingCamera</b> component's <b>MassiveDistance</b> setting.
	/// NOTE: This component overrides the <b>Transform</b> component settings, and will not react to any manual changes made to it.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingMassive")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Massive")]
	public class SgtFloatingMassive : SgtFloatingObject
	{
		/// <summary>This setting allows you to adjust the per-axis size of this object.</summary>
		public Vector3 Scale { set { scale = value; } get { return scale; } } [SerializeField] protected Vector3 scale = Vector3.one;

		/// <summary>The base scale of this object.</summary>
		public SgtLength Size { set { size = value; } get { return size; } } [SerializeField] protected SgtLength size = new SgtLength(1.0, SgtLength.ScaleType.Meter);

		protected override void ApplyPosition(SgtFloatingCamera floatingCamera)
		{
			// Ignore standard snaps
		}

		protected override void CheckForPositionChanges()
		{
			// Ignore position changes
		}

		private double ConvertDistance(double distance, double maxDistance)
		{
			var halfDistance = maxDistance * 0.5;

			if (distance < halfDistance)
			{
				return distance / halfDistance;
			}
            else
            {
                var log = System.Math.Log(distance / halfDistance, 10) + 1;

				return (1.0f - System.Math.Pow(0.5f, log)) * maxDistance;
            }
        }

		protected virtual void LateUpdate()
		{
			if (SgtFloatingCamera.Instances.Count > 0)
			{
				var floatingCamera = SgtFloatingCamera.Instances.First.Value;
				var camPos         = floatingCamera.Position;
				var distance       = SgtPosition.Distance(ref camPos, ref position);
				var sca            = CalculateScale(distance, floatingCamera.MassiveDistance);

				transform.position   = floatingCamera.transform.position + SgtPosition.Vector(ref camPos, ref position, sca);
				transform.localScale = scale * (float)(size * sca);
			}
		}

		private double CalculateScale(double distance, double maximumDistance)
		{
			distance += size;

			var scaledDistance = ConvertDistance(distance, maximumDistance);

			if (distance > 0.0f)
			{
				return scaledDistance / distance;
			}

			return 1.0;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingMassive;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET), true)]
	public class SgtFloatingMassive_Editor : SgtFloatingObject_Editor
	{
		protected override void OnInspector()
		{
			base.OnInspector();

			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("scale", "This setting allows you to adjust the per-axis size of this object.");
			Draw("size", "The base scale of this object.");

			if (SgtFloatingCamera.Instances.Count > 0)
			{
				var floatingCamera = SgtFloatingCamera.Instances.First.Value;

				Separator();

				BeginDisabled();
					EditorGUILayout.FloatField(new GUIContent("Massive Distance", "This component scales based on the SgtFloatingCamera component's MassiveDistance setting."), (float)floatingCamera.MassiveDistance);
				EndDisabled();
			}
		}
	}
}
#endif