using Cinemachine;
using UnityEngine;
using UnityEditor;

[DisallowMultipleComponent]
[CanEditMultipleObjects]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class SwimController : MonoBehaviour
{
	[Header("Speed")]
	[SerializeField] private float forwardSpeed = 30f;
	[SerializeField] private float sidewaysSpeed = 20f;
	[SerializeField] private float backwardsSpeed = 10f;
	[SerializeField] private float verticalSpeed = 20f;
	[SerializeField] private float pushbackForce = 40f;
	[SerializeField] private Vector2 animatorSpeedRange = new Vector2(0.5f, 2f);

	[Header("Responsiveness")]
	[Tooltip("Time in seconds it takes to reach the set speed.")]
	[SerializeField] private float accelerationTime = 1f;
	[Tooltip("Time in seconds it takes to stop to a halt.")]
	[SerializeField] private float decelerationTime = 2f;
	[Tooltip("The higher this value, the faster the gameObject turns on reorienting itself.")]
	[SerializeField] private float turnSmoothing = 2f;

	private Vector3 lastMoveVelocity = Vector3.zero;
	private bool canMove = true;
	private float animatorMaxSpeed { get { return Mathf.Max(forwardSpeed, sidewaysSpeed, backwardsSpeed, verticalSpeed); } }

	private Animator animator;
	private new Rigidbody rigidbody;
	private new Camera camera;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		rigidbody = GetComponent<Rigidbody>();
		camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

		Cursor.lockState = CursorLockMode.Locked;

		Debug.LogWarning("Translation lerping can cause the player to go from fullspeed forwards to fullspeed backwards (lerping not taking direction into account)");
		Debug.LogWarning("The AnimtorSpeedRange Lerping assumes forwardSpeed is the maximum speed");
	}

	private void FixedUpdate()
	{
		Vector3 translation = GetTranslation();

		bool isMoving = translation != Vector3.zero;

		if (isMoving)
		{
			// Move the controller.
			//rigidbody.useGravity = false;
			rigidbody.velocity = translation; // Flawed way of setting velocity : physics calculations might be off.
			lastMoveVelocity = rigidbody.velocity;
		}
		// Check if the rigidboy has stopped moving and if so reset the rigidbody.
		else if (decelerationTime <= 0 ||
			Mathf.Pow(0.000001f, 2) >= rigidbody.velocity.sqrMagnitude ||
			lastMoveVelocity.normalized != rigidbody.velocity.normalized)
		{
			//rigidbody.useGravity = true;
			rigidbody.Sleep();
			lastMoveVelocity = Vector3.zero;
			canMove = true;
		}
		else 
		{
			// Slow down the rigidbody.
			Vector3 slowdownVelocity = -lastMoveVelocity * Time.deltaTime / decelerationTime;
			rigidbody.AddForce(slowdownVelocity, ForceMode.VelocityChange);
		}

		Rotate();
		UpdateAnimatorSpeed();
	}

	// Returns a movement vector for determining the next translation.
	private Vector3 GetTranslation()
	{
		if (!canMove) return Vector3.zero;

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
		/*
		if (qKey != eKey)
		{
			translation += Vector3.up * (qKey ? 1 : -1);
			speed += verticalSpeed;
			speedDivider++;
		}
		*/
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
		float mouseX = Input.GetAxis("Mouse X");
		Quaternion targetRotation = transform.rotation * Quaternion.Euler(Vector3.up * mouseX);
		rigidbody.MoveRotation(targetRotation);

		/*
		Vector3 targetDirection = camera.transform.forward;

		Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
		targetRotation = Quaternion.Lerp(rigidbody.rotation, targetRotation, turnSmoothing * Time.deltaTime);

		rigidbody.MoveRotation(targetRotation);
		*/
	}

	private void UpdateAnimatorSpeed()
	{
		float t = rigidbody.velocity.magnitude / animatorMaxSpeed;
		float animatorSpeed = Mathf.Lerp(animatorSpeedRange.x, animatorSpeedRange.y, t);

		animator.speed = animatorSpeed;
	}

	public void Hit(Transform hitTransform)
	{
		Vector3 awayDir = (transform.position - hitTransform.position).normalized;
		rigidbody.velocity = awayDir * pushbackForce;
		lastMoveVelocity = rigidbody.velocity / 2;
		canMove = false;
	}
}
