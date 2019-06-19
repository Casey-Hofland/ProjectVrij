using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animatie_oproep_test : MonoBehaviour
{

    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey("b"))
        {

            anim.Play("biter");
        }

       
    }
}
