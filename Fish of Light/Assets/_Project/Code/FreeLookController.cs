using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeLookController : MonoBehaviour
{
	[Header("Camera Radius")]
	[Tooltip("The minimum to maximum distance the camera can be away from the player.")]
	[SerializeField] private Vector2 camRadiusRange = new Vector2(10f, 20f);
	[Tooltip("The radius for the top and bottom of the camera.")]
	[SerializeField] [Range(0f, 1f)] private float camSplineRadiusMultiplier = 0.25f;
	[Tooltip("The higher this value, the faster the camera zooms to the desired value.")]
	[SerializeField] private float zoomSmoothing = 2f;

	private float camMin { get { return camRadiusRange.x; } }
	private float camMax { get { return camRadiusRange.y; } }
	private float camSplineMin { get { return camMin * camSplineRadiusMultiplier; } }
	private float camSplineMax { get { return camMax * camSplineRadiusMultiplier; } }

	private CinemachineFreeLook cinemachineCamera;

	private void Awake()
	{
		cinemachineCamera = GameObject.FindObjectOfType<CinemachineFreeLook>();
	}

	private void Start()
	{
		UpdateCameraZoom(false);
	}

	private void FixedUpdate()
	{
		UpdateCameraZoom(true);
	}

	private void UpdateCameraZoom(bool lerp = true)
	{
		if (cinemachineCamera == null) return;

		float radius = camMax;

		// Casts a ray to the ground to check if the camera should be zoomed in or out (But flawed, raycast is only checking for down)
		if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, camMax, LayerMask.GetMask("Terrain"), QueryTriggerInteraction.Ignore))
		{
			radius = Mathf.Max(camMin, hitInfo.distance);
		}

		float splineRadius = radius * camSplineRadiusMultiplier;

		// Lerps the target values according to the current values.
		if (lerp)
		{
			radius = Mathf.Lerp(cinemachineCamera.m_Orbits[1].m_Radius, radius, zoomSmoothing * Time.deltaTime);
			splineRadius = Mathf.Lerp(cinemachineCamera.m_Orbits[0].m_Radius, splineRadius, zoomSmoothing * Time.deltaTime);
		}

		// Sets the corresponding Cinemachine values.
		cinemachineCamera.m_Orbits[0].m_Height = radius;
		cinemachineCamera.m_Orbits[0].m_Radius = splineRadius;

		cinemachineCamera.m_Orbits[1].m_Radius = radius;

		cinemachineCamera.m_Orbits[2].m_Height = -radius;
		cinemachineCamera.m_Orbits[2].m_Radius = splineRadius;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		/*
		// Draw the camera circle of influence.
		UnityEditor.Handles.color = Color.green;

		UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, camMin);
		UnityEditor.Handles.DrawWireDisc(transform.position + (Vector3.up * camMin), Vector3.up, camSplineMin);
		UnityEditor.Handles.DrawWireDisc(transform.position + (Vector3.down * camMin), Vector3.up, camSplineMin);
		UnityEditor.Handles.DrawWireArc(transform.position, Vector3.right, Vector3.down, 180f, camMin);

		UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, camMax);
		UnityEditor.Handles.DrawWireDisc(transform.position + (Vector3.up * camMax), Vector3.up, camSplineMax);
		UnityEditor.Handles.DrawWireDisc(transform.position + (Vector3.down * camMax), Vector3.up, camSplineMax);
		UnityEditor.Handles.DrawWireArc(transform.position, Vector3.right, Vector3.down, 180f, camMax);
        */
	}
#endif
}
