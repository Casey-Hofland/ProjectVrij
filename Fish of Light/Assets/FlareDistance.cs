using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FlareDistance : MonoBehaviour
{
	[SerializeField] private StudioEventEmitter studioEvent;
	[SerializeField] private Transform target;

	void Update()
    {
		float distance = Vector3.Distance(transform.position, target.position);
		float distPercentage = Mathf.Clamp01((studioEvent.OverrideMaxDistance - distance) / studioEvent.OverrideMaxDistance);
		studioEvent.SetParameter("distanceToPlayer", distPercentage);
    }
}
