using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimationStart : MonoBehaviour
{
	[SerializeField] private string stateName = "";
	[SerializeField] private Vector2 speedRange = new Vector2(0.2f, 1.2f);

	[SerializeField] private float moveSpeed;
	[SerializeField] private float distance;

	private Vector3 startPosition;
	private Vector3 endPosition;
	private float animSpeed;
	private float t;
	private bool forward;
	private bool rotating = false;

	private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
		startPosition = transform.position;
		endPosition = startPosition + transform.forward * distance;
		animSpeed = Random.Range(speedRange.x, speedRange.y);
		t = Random.value;
		transform.Translate(Vector3.forward * t * distance);
		forward = Random.value >= 0.5f;
		if (!forward)
			rotating = true;

		animator = GetComponent<Animator>();
		animator.Play(stateName, 0, Random.value);
		animator.speed = animSpeed;
    }

	private void Update()
	{
		if (distance <= 0) return;

		if (rotating)
		{
			transform.Rotate(Vector3.up * 180);
			rotating = false;
		}
		else
		{
			transform.Translate(Vector3.forward * moveSpeed * animSpeed * Time.deltaTime);

			//transform.position = Vector3.Lerp(startPosition, endPosition, t);
			t = Mathf.Clamp01(t + ((moveSpeed * animSpeed * Time.deltaTime) / distance) * ((forward) ? 1 : -1));

			if (Mathf.Approximately(t, 0) || Mathf.Approximately(t, 1))
			{
				forward = !forward;
				rotating = true;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
			Gizmos.DrawLine(startPosition, endPosition);
		else
			Gizmos.DrawLine(transform.position, transform.position + transform.forward * distance);
	}
}
