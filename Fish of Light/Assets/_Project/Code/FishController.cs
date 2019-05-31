using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class FishController : MonoBehaviour
{
	[Header("Movement Settings")]
	[SerializeField] private float forwardSpeed = 3f;
	[SerializeField] private float sidewaysSpeed = 2f;
	[SerializeField] private float backwardsSpeed = 1f;
	[SerializeField] private float verticalSpeed = 2f;
	[Tooltip("Time in seconds it takes to reach the set speed.")]
	[SerializeField] private float accelerationTime = 1f;
	[Tooltip("Time in seconds it takes to stop to a halt.")]
	[SerializeField] private float decelerationTime = 2f;
	[Tooltip("The higher this value, the faster the gameObject turns on reorienting itself.")]
	[SerializeField] private float turnSmoothing = 2f;

	[Header("Camera Settings")]
	[Tooltip("The minimum to maximum distance the camera can be away from the player.")]
	[SerializeField] private Vector2 camRadiusRange = new Vector2(4f, 20f);
	[Tooltip("The radius for the top and bottom of the camera.")]
	[SerializeField] [Range(0f, 1f)] private float camSplineRadiusMultiplier = 0.25f;
	[Tooltip("The higher this value, the faster the camera zooms to the desired value.")]
	[SerializeField] private float zoomSmoothing = 2f;

	private float camMin { get { return camRadiusRange.x; } }
	private float camMax { get { return camRadiusRange.y; } }
	private float camSplineMin { get { return camMin * camSplineRadiusMultiplier; } }
	private float camSplineMax { get { return camMax * camSplineRadiusMultiplier; } }
	
	private Vector3 lastMoveVelocity = Vector3.zero;

	private new Rigidbody rigidbody;
	private new Camera camera;
	private CinemachineFreeLook cinemachineCamera;

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		cinemachineCamera = GameObject.FindObjectOfType<CinemachineFreeLook>();

		Cursor.lockState = CursorLockMode.Locked;

		Debug.LogWarning("Translation lerping can cause the player to go from fullspeed forwards to fullspeed backwards (lerping not taking direction into account)");
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

			rigidbody.velocity = translation;

			/*
			Vector3 force = translation;
			rigidbody.velocity = Vector3.zero;

			if (Mathf.Pow(speed, 2) <= force.sqrMagnitude)
			{
				force = force.normalized * speed;
			}

			Debug.Log(force);
			rigidbody.AddForce(force, ForceMode.VelocityChange);
			Debug.Log(rigidbody.velocity);
			*/

			Rotate();
			lastMoveVelocity = rigidbody.velocity;
		}
		else if (decelerationTime <= 0 ||
			Mathf.Pow(0.000001f, 2) >= rigidbody.velocity.sqrMagnitude ||
			lastMoveVelocity.normalized != rigidbody.velocity.normalized)
		{
			rigidbody.useGravity = true;
			rigidbody.Sleep();
			lastMoveVelocity = Vector3.zero;
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
		float speed = 0f;
		int speedDivider = 0;

		bool qKey = Input.GetKey(KeyCode.Q),
			eKey = Input.GetKey(KeyCode.E),
			wKey = Input.GetKey(KeyCode.W),
			sKey = Input.GetKey(KeyCode.S),
			dKey = Input.GetKey(KeyCode.D),
			aKey = Input.GetKey(KeyCode.A);

		if (qKey != eKey)
		{
			translation += Vector3.up * (qKey ? 1 : -1);
			speed += verticalSpeed;
			speedDivider++;
		}
		if (wKey != sKey)
		{
			translation += transform.forward * (wKey ? 1 : -1);
			speed += (wKey ? forwardSpeed : backwardsSpeed);
			speedDivider++;
		}
		if (dKey != aKey)
		{
			translation += transform.right * (dKey ? 1 : -1);
			speed += sidewaysSpeed;
			speedDivider++;
		}

		translation.Normalize();
		if (speedDivider > 0)
			speed /= speedDivider;

		float acceleratedTime = rigidbody.velocity.magnitude / speed * Time.deltaTime + Time.deltaTime;
		float t = (acceleratedTime <= accelerationTime ? acceleratedTime / accelerationTime : accelerationTime / acceleratedTime);
		translation *= Mathf.Lerp(rigidbody.velocity.magnitude, speed, t);

		return translation;
	}

	// Rotates the fish to where it needs to be facing.
	private void Rotate()
	{
		Vector3 targetDirection = camera.transform.forward;

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
			splineRadius = Mathf.Lerp(cinemachineCamera.m_Orbits[0].m_Radius, splineRadius, zoomSmoothing * Time.deltaTime);
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
