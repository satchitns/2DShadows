using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {
    public float playerSpeed = 10f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate() {
        float xVal = Input.GetAxisRaw("Horizontal");
        float yVal = Input.GetAxisRaw("Vertical");
        transform.Translate((Vector3.up * yVal + Vector3.right * xVal) * playerSpeed * Time.deltaTime);
    }
}
