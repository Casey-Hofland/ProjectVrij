using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimationStart : MonoBehaviour
{
	[SerializeField] private string stateName = "";
	[SerializeField] private Vector2 speedRange = new Vector2(0.2f, 1.2f);

	private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
		animator = GetComponent<Animator>();
		animator.Play(stateName, 0, Random.value);
		animator.speed = Random.Range(speedRange.x, speedRange.y);
    }
}
