﻿using System;
using System.Reflection;
using UnityEngine;

#if false
using UnityEngine.Experimental.Input;

namespace UnityTemplateProjects
{
	public class CameraController_InputSystem : MonoBehaviour
	{
		class CameraState
		{
			public float yaw;
			public float pitch;
			public float roll;
			public float x;
			public float y;
			public float z;

			public void SetFromTransform(Transform t)
			{
				pitch = t.eulerAngles.x;
				yaw = t.eulerAngles.y;
				roll = t.eulerAngles.z;
				x = t.position.x;
				y = t.position.y;
				z = t.position.z;
			}

			public void Translate(Vector3 translation)
			{
				Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

				x += rotatedTranslation.x;
				y += rotatedTranslation.y;
				z += rotatedTranslation.z;
			}

			public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
			{
				yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
				pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
				roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);

				x = Mathf.Lerp(x, target.x, positionLerpPct);
				y = Mathf.Lerp(y, target.y, positionLerpPct);
				z = Mathf.Lerp(z, target.z, positionLerpPct);
			}

			public void UpdateTransform(Transform t)
			{
				t.eulerAngles = new Vector3(pitch, yaw, roll);
				t.position = new Vector3(x, y, z);
			}
		}

		Keyboard keyboard = Keyboard.current;
		Mouse mouse = Mouse.current;

        CameraState m_TargetCameraState = new CameraState();
        CameraState m_InterpolatingCameraState = new CameraState();

        [Header("Movement Settings")]
        [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
        public float boost = 3.5f;

        [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
        public float positionLerpTime = 0.2f;

        [Header("Rotation Settings")]
        [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
        public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

        [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
        public float rotationLerpTime = 0.01f;

        [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
        public bool invertY = false;

		void OnEnable()
        {
            m_TargetCameraState.SetFromTransform(transform);
            m_InterpolatingCameraState.SetFromTransform(transform);
        }

		Vector3 GetInputTranslationDirection()
        {
            Vector3 direction = new Vector3();
            if (keyboard.wKey.isPressed)
            {
                direction += Vector3.forward;
            }
            if (keyboard.sKey.isPressed)
            {
                direction += Vector3.back;
            }
            if (keyboard.aKey.isPressed)
            {
                direction += Vector3.left;
            }
            if (keyboard.dKey.isPressed)
            {
                direction += Vector3.right;
            }
            if (keyboard.qKey.isPressed)
            {
                direction += Vector3.down;
            }
            if (keyboard.eKey.isPressed)
            {
                direction += Vector3.up;
            }
            return direction;
        }
        
        void Update()
        {
            // Exit Sample  
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Application.Quit();
				#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false; 
				#endif
            }

            // Hide and lock cursor when right mouse button pressed
            if (mouse.rightButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            // Unlock and show cursor when right mouse button released
            if (mouse.rightButton.wasReleasedThisFrame)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            // Rotation
            if (mouse.rightButton.isPressed)
            {
                var mouseMovement = new Vector2(mouse.delta.x.ReadValue(), mouse.delta.y.ReadValue() * (invertY ? 1 : -1)) * 0.05f;

                var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

                m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
                m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
            }
            
            // Translation
            var translation = GetInputTranslationDirection() * Time.deltaTime;

            // Speed up movement when shift key held
            if (keyboard.leftShiftKey.isPressed)
            {
                translation *= 10.0f;
            }

            // Modify movement by a boost factor (defined in Inspector and modified in play mode through the mouse scroll wheel)
            // boost += mouse.scroll.y.ReadValue() * 0.2f; // A bug in the Unity Editor currently messes up the scroll value when moving the mouse after having scrolled.
            translation *= Mathf.Pow(2.0f, boost);

            m_TargetCameraState.Translate(translation);

            // Framerate-independent interpolation
            // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
            var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
            var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
            m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

            m_InterpolatingCameraState.UpdateTransform(transform);
        }
    }
}
#else
namespace UnityTemplateProjects
{
	public class CameraController_InputSystem : MonoBehaviour
	{
		#pragma warning disable CS0414 // Assigned but never used
		[Tooltip("The Camera Controller_Input System won't work until the directive at the top of the script is changed from 'false' to 'true'. " +
				"This to ensure that the script won't throw any compile errors if the Input System Package has not been installed in the project. " +
				"To edit, go to the component menu 'Edit Script', or find the .cs file in your project window and doubleclick it.")]
		[SerializeField] private string HoverOverMe = "< User Instructions";
		#pragma warning restore CS0414 // Assigned but never used

		private void Awake()
		{
			Debug.LogWarning("The Camera Controller_Input System won't work until the directive at the top of the script is changed from 'false' to 'true'. " +
				"This to ensure that the script won't throw any compile errors if the Input System Package has not been installed in the project. " +
				"To edit, go to the component menu 'Edit Script', or find the .cs file in your project window and doubleclick it.");
		}
	}
}
#endif