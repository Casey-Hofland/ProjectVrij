using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTrigger : MonoBehaviour
{
	[SerializeField] private TriggerEvent[] triggerEvents = new TriggerEvent[1];

	[Serializable]
	private class TriggerEvent
	{
		[Header("Animation")]
		[SerializeField] private Animator animator = null;
		[SerializeField] private string triggerName = "OnTriggerEnter";

		[Header("Sound")]
		[SerializeField] private AudioClip clip = null;
		[Tooltip("Specifies if the sound should be 2D (plays at set volume no matter how far away you are).")]
		[SerializeField] private bool sound2D = true;
		[Tooltip("Defaults to this gameobjects' transform if left null.")]
		[SerializeField] private Transform soundTransform = null;

		[Header("Event")]
		[SerializeField] private float delay = 0f;
		[SerializeField] private bool repeat = false;

		private bool activateTrigger = true;

		public IEnumerator ActivateTrigger(Transform transform)
		{
			if (!activateTrigger)
				yield break;

			activateTrigger = false;
			yield return new WaitForSeconds(delay);

			animator.SetTrigger(triggerName);

			GameObject go = new GameObject();
			go.name = clip.name;
			Transform tf = (soundTransform != null) ? soundTransform : transform;
			Debug.Log(tf);
			go.transform.position = tf.position;
			go.transform.rotation = tf.rotation;
			AudioSource audioS = go.AddComponent<AudioSource>();
			audioS.spatialBlend = (sound2D) ? 0 : 1;
			audioS.PlayOneShot(clip);

			yield return new WaitForSeconds(clip.length);

			Destroy(go);
			activateTrigger = repeat;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		foreach (TriggerEvent triggerEvent in triggerEvents)
		{
			StartCoroutine(triggerEvent.ActivateTrigger(transform));
		}
	}
}
