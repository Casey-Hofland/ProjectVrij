using UnityEngine;

[DisallowMultipleComponent]
[UnityEditor.CanEditMultipleObjects]
[RequireComponent(typeof(SwimController))]
public class SwimUserControls : MonoBehaviour
{
	private SwimController swimController;

	private void Start()
	{
		swimController = GetComponent<SwimController>();
	}

	private void OnEnable()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void OnDisable()
	{
		Cursor.lockState = CursorLockMode.None;
	}

	private void FixedUpdate()
	{
		Vector3 translation = new Vector3();

		bool[] useSpeed = new bool[4];

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
			useSpeed[0] = true;
		}
		*/
		if (wKey != sKey)
		{
			translation += transform.forward * (wKey ? 1 : -1);
			useSpeed[(wKey ? 1 : 2)] = true;
		}
		if (dKey != aKey)
		{
			translation += transform.right * (dKey ? 1 : -1);
			useSpeed[3] = true;
		}

		translation.Normalize();

		float mouseX = Input.GetAxis("Mouse X");

		swimController.Move(translation, useSpeed);
		swimController.Rotate(mouseX);
	}
}
