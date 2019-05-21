using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class FishController : MonoBehaviour
{
	[Header("Unsafe")]
	[SerializeField] private float speed = 3f;
	[Tooltip("Time in seconds it takes to reach the set speed")]
	[SerializeField] private float accelerationTime = 1f;

	[Tooltip("Time in seconds it takes to stop to a halt")]
	[SerializeField] private float decelerationTime = 2f;
	[SerializeField] private float idleThreshold = 0.000001f;

	[Header("Safe")]
	[Tooltip("The higher this value, the faster the gameObject turns on reorienting itself")]
	[SerializeField] private float turnSmoothing = 2f;

	[Header("Camera Settings")]
	[SerializeField] private Vector2 camRadiusRange = new Vector2(4f, 20f);
	[SerializeField] [Range(0f, 1f)] private float camSplineRadiusMultiplier = 0.25f;
	[SerializeField] private float zoomSmoothing = 2f;

	private float camMin { get { return camRadiusRange.x; } }
	private float camMax { get { return camRadiusRange.y; } }
	private float camSplineMin { get { return camMin * camSplineRadiusMultiplier; } }
	private float camSplineMax { get { return camMax * camSplineRadiusMultiplier; } }
	
	private Vector3 lastMoveVelocity = Vector3.zero;

	private float cTime = 0f; // Used for testing

	private new Rigidbody rigidbody;
	private new Camera camera;
	private CinemachineFreeLook cinemachineCamera;

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

		cinemachineCamera = GameObject.FindObjectOfType<CinemachineFreeLook>();

		Cursor.lockState = CursorLockMode.Locked;

		Debug.LogWarning("Target Direction in 'GetTranslation()' and 'Rotate()' are magic numbers!");
		Debug.LogWarning("slowdownTime depends on idleThreshold to be 0.000001 to be correct! Also it's not working anymore - don't know why yet");
	}

	private void Start()
	{
		SetCamera(false);
	}

	private void FixedUpdate()
	{
		Vector3 translation = GetTranslation();

		bool isMoving = translation != Vector3.zero;

		if (isMoving)
		{
			rigidbody.useGravity = false;

			Vector3 force = rigidbody.velocity + translation;
			rigidbody.Sleep();

			if (Mathf.Pow(speed, 2) <= force.sqrMagnitude)
			{
				force = force.normalized * speed;
			}

			rigidbody.AddForce(force, ForceMode.VelocityChange);

			Rotate();

			lastMoveVelocity = rigidbody.velocity;
			cTime = Time.timeSinceLevelLoad;
		}
		else if (decelerationTime <= 0 ||
			Mathf.Pow(idleThreshold, 2) >= rigidbody.velocity.sqrMagnitude ||
			lastMoveVelocity.normalized != rigidbody.velocity.normalized)
		{
			rigidbody.useGravity = true;
			rigidbody.Sleep();
			lastMoveVelocity = Vector3.zero;

			//Debug.Log(Time.timeSinceLevelLoad - cTime + " : " + rigidbody.velocity);
		}
		else 
		{
			Vector3 slowdownVelocity = -lastMoveVelocity * Time.deltaTime / decelerationTime;
			rigidbody.AddForce(slowdownVelocity, ForceMode.VelocityChange);

			Rotate();
		}

		SetCamera();
	}

	private Vector3 GetTranslation()
	{
		Vector3 translation = new Vector3();

		Vector3 direction = /*camera.*/transform.forward;
		//direction.y -= (1f / 3f);
		if (Input.GetKey(KeyCode.W))
		{
			translation += direction;
		}
		if (Input.GetKey(KeyCode.S))
		{
			translation -= direction;
		}

		translation *= speed;
		if (accelerationTime > 0)
		{
			translation *= Time.deltaTime / accelerationTime;
		}

		return translation;
	}

	private void Rotate()
	{
		Vector3 targetDirection = camera.transform.forward;
		targetDirection.y += (2f / 3f);

		Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
		targetRotation = Quaternion.Lerp(rigidbody.rotation, targetRotation, turnSmoothing * Time.deltaTime);

		rigidbody.MoveRotation(targetRotation);
	}

	private void SetCamera(bool lerp = true)
	{
		float radius = camMax;

		// Casts a ray to the ground to check if the camera should be zoomed in (But flawed, raycast is only checking for down)
		if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, camMax, LayerMask.GetMask("Terrain"), QueryTriggerInteraction.Ignore))
		{
			radius = Mathf.Max(camMin, hitInfo.distance);
		}

		float splineRadius = radius * camSplineRadiusMultiplier;

		if (lerp)
		{
			radius = Mathf.Lerp(cinemachineCamera.m_Orbits[1].m_Radius, radius, zoomSmoothing * Time.deltaTime);
			splineRadius = Mathf.Lerp(cinemachineCamera.m_Orbits[0].m_Height, splineRadius, zoomSmoothing * Time.deltaTime);
		}

		cinemachineCamera.m_Orbits[0].m_Height = radius;
		cinemachineCamera.m_Orbits[0].m_Radius = splineRadius;

		cinemachineCamera.m_Orbits[1].m_Radius = radius;

		cinemachineCamera.m_Orbits[2].m_Height = -radius;
		cinemachineCamera.m_Orbits[2].m_Radius = splineRadius;
	}

	/*
	private void OnTriggerStay(Collider other)
	{
		if (LayerMask.LayerToName(other.gameObject.layer) == "Terrain")
		{
			other.ClosestPoint(transform.position);
			SetCamera();
		}
	}
	*/

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		UnityEditor.Handles.color = Color.green;

		UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, camMin);
		UnityEditor.Handles.DrawWireDisc(transform.position + (Vector3.up * camMin), Vector3.up, camSplineMin);
		UnityEditor.Handles.DrawWireDisc(transform.position + (Vector3.down * camMin), Vector3.up, camSplineMin);
		UnityEditor.Handles.DrawWireArc(transform.position, Vector3.right, Vector3.down, 180f, camMin);

		UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, camMax);
		UnityEditor.Handles.DrawWireDisc(transform.position + (Vector3.up * camMax), Vector3.up, camSplineMax);
		UnityEditor.Handles.DrawWireDisc(transform.position + (Vector3.down * camMax), Vector3.up, camSplineMax);
		UnityEditor.Handles.DrawWireArc(transform.position, Vector3.right, Vector3.down, 180f, camMax);
	}
#endif
}
