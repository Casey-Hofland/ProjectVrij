using System;
using System.Collections;
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

			activateTrigger = repeat;
		}
	}

	private void OnDisable() { }

	private void OnTriggerEnter(Collider other)
	{
		if (!this.enabled) return;
		foreach (TriggerEvent triggerEvent in triggerEvents)
		{
			StartCoroutine(triggerEvent.ActivateTrigger(transform));
		}
	}
}
