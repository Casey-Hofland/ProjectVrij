using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTrigger : MonoBehaviour
{
	[SerializeField] private bool repeatTrigger;



	private void OnTriggerEnter(Collider other)
	{
		ActivateTrigger();
	}

	private void ActivateTrigger()
	{

	}
}
