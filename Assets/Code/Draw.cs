using UnityEngine;
using System.Collections;

/// <summary>
/// Utility class for drawing various handy debug stuff.
/// </summary>
public static class Draw
{
	public static void ArrowLine(Vector3 from, Vector3 to)
	{
		Gizmos.DrawLine(from, to);

		var indicatorPoint = to;
		var direction = (from - to) * 0.1f;
		var perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;
		var offsetDirection = direction + perpendicular * 0.1f;
		Gizmos.DrawLine(indicatorPoint, indicatorPoint + offsetDirection);
		Gizmos.DrawLine(indicatorPoint, indicatorPoint + Quaternion.AngleAxis(90f, direction) * offsetDirection);
		Gizmos.DrawLine(indicatorPoint, indicatorPoint + Quaternion.AngleAxis(180f, direction) * offsetDirection);
		Gizmos.DrawLine(indicatorPoint, indicatorPoint + Quaternion.AngleAxis(270f, direction) * offsetDirection);
	}

	public static void GizmoX(Vector3 position, float size)
	{
		float halfSize = size / 2;
		Gizmos.DrawLine(position + new Vector3(halfSize, 0f, halfSize), position + new Vector3(-halfSize, 0f, -halfSize));
		Gizmos.DrawLine(position + new Vector3(-halfSize, 0f, halfSize), position + new Vector3(halfSize, 0f, -halfSize));
	}
}
