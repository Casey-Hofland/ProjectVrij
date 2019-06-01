using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwimController : MonoBehaviour
{
	[Header("Speed")]
	[SerializeField] private float forwardSpeed = 3f;
	[SerializeField] private float sidewaysSpeed = 2f;
	[SerializeField] private float backwardsSpeed = 1f;
	[SerializeField] private float verticalSpeed = 2f;

	[Header("Responsiveness")]
	[Tooltip("Time in seconds it takes to reach the set speed.")]
	[SerializeField] private float accelerationTime = 1f;
	[Tooltip("Time in seconds it takes to stop to a halt.")]
	[SerializeField] private float decelerationTime = 2f;
	[Tooltip("The higher this value, the faster the gameObject turns on reorienting itself.")]
	[SerializeField] private float turnSmoothing = 2f;

	[Header("Camera Radius")]
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
			// Move the controller.
			rigidbody.useGravity = false;
			rigidbody.velocity = translation; // Flawed way of setting velocity : physics calculations might be off.

			Rotate();
			lastMoveVelocity = rigidbody.velocity;
		}
		// Check if the rigidboy has stopped moving and if so reset the rigidbody.
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
			// Slow down the rigidbody.
			Vector3 slowdownVelocity = -lastMoveVelocity * Time.deltaTime / decelerationTime;
			rigidbody.AddForce(slowdownVelocity, ForceMode.VelocityChange);

			Rotate();
		}

		SetCamera();
	}

	// Returns a movement vector for determining the next translation.
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

		// If one of direction keys is pressed, add direction and speed to the next frame of movement.
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

		if (speedDivider == 0)
			return translation;

		// Normalize the direction and speed.
		translation.Normalize();
		speed /= speedDivider;

		// Lerp the target speed according to the current speed.
		float acceleratedTime = rigidbody.velocity.magnitude / speed * Time.deltaTime + Time.deltaTime;
		float t = (acceleratedTime <= accelerationTime ? acceleratedTime / accelerationTime : accelerationTime / acceleratedTime);
		translation *= Mathf.Lerp(rigidbody.velocity.magnitude, speed, t);

		return translation;
	}

	// Rotates the controller to face in line with the camera.
	private void Rotate()
	{
		Vector3 targetDirection = camera.transform.forward;

		Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
		targetRotation = Quaternion.Lerp(rigidbody.rotation, targetRotation, turnSmoothing * Time.deltaTime);

		rigidbody.MoveRotation(targetRotation);
	}

	// Updates the Camera Zoom.
	private void SetCamera(bool lerp = true)
	{
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
	}
#endif
}
