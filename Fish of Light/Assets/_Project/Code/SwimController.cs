using Cinemachine;
using FMODUnity;
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
	private StudioEventEmitter eventEmitter;

	private void Start()
	{
		animator = GetComponent<Animator>();
		rigidbody = GetComponent<Rigidbody>();
		camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		eventEmitter = GetComponent<StudioEventEmitter>();

		Debug.LogWarning("Translation lerping can cause the player to go from fullspeed forwards to fullspeed backwards (lerping not taking direction into account)");
	}

	private void FixedUpdate()
	{
		UpdateAnimatorSpeed();
	}

	public void Move(Vector3 translation)
	{
		if (canMove && translation != Vector3.zero)
		{
			rigidbody.velocity = translation;
			lastMoveVelocity = rigidbody.velocity;
		}
		else if (decelerationTime <= 0 ||
			Mathf.Pow(0.000001f, 2) >= rigidbody.velocity.sqrMagnitude ||
			lastMoveVelocity.normalized != rigidbody.velocity.normalized)
		{
			rigidbody.velocity = Vector3.zero;
			lastMoveVelocity = rigidbody.velocity;
			canMove = true;
		}
		else
		{
			// Slow down the rigidbody.
			Vector3 slowdownVelocity = -lastMoveVelocity * Time.deltaTime / decelerationTime;
			rigidbody.AddForce(slowdownVelocity, ForceMode.VelocityChange);
		}
	}

	public void Move(Vector3 translation, bool[] useSpeed)
	{
		if (!canMove)
		{
			Move(translation);
			return;
		}

		float speed = 0;
		int speedDivider = 0;

		if (useSpeed[0])
		{
			speed += verticalSpeed;
			speedDivider++;
		}
		if (useSpeed[1])
		{
			speed += forwardSpeed;
			speedDivider++;
		}
		else if (useSpeed[2])
		{
			speed += backwardsSpeed;
			speedDivider++;
		}
		if (useSpeed[3])
		{
			speed += sidewaysSpeed;
			speedDivider++;
		}

		if (speedDivider == 0)
		{
			Move(translation);
			return;
		}

		speed /= (float)speedDivider;

		// Lerp the target speed according to the current speed.
		float acceleratedTime = rigidbody.velocity.magnitude / speed * Time.deltaTime + Time.deltaTime;
		float t = (acceleratedTime <= accelerationTime ? acceleratedTime / accelerationTime : accelerationTime / acceleratedTime);
		translation *= Mathf.Lerp(rigidbody.velocity.magnitude, speed, t);

		Move(translation);
	}

	// Rotates the controller to face in line with the camera.
	public void Rotate(float axis)
	{
		Quaternion targetRotation = transform.rotation * Quaternion.Euler(Vector3.up * axis);
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

	public void PlaySound()
	{
		if (eventEmitter.isActiveAndEnabled)
		{
			eventEmitter.Play();
		}
	}

	public void Hit(Transform hitTransform)
	{
		Vector3 awayDir = (transform.position - hitTransform.position).normalized;
		rigidbody.velocity = awayDir * pushbackForce;
		lastMoveVelocity = rigidbody.velocity / 2;
		canMove = false;
	}
}
