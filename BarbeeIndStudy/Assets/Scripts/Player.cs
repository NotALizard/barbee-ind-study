using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public bool keyboard = true;
    private PlayerInputFactory input;
    Vector2 movement = Vector2.zero;
    readonly float walkSpeed = 5;

    bool canJump = true;
    public bool sameJump = false;
    float jumpStrength = 8;
    public float remainingJumpPower = 0;

    Rigidbody rb;
    public Transform head;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        if (keyboard)
        {
            input = new KeyboardMousePlayerInputFactory();
        }
        else
        {
            input = new GamepadPlayerInputFactory();
        }
        input.Init();
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 walk = input.WalkVec();
        transform.Translate(walkSpeed * Time.deltaTime * new Vector3(walk.x, 0, walk.y));
        Vector2 look = input.LookVec();
        transform.Rotate(new Vector3(0, look.x, 0));
        head.Rotate(new Vector3(-look.y, 0, 0));

        canJump = Physics.CheckBox(transform.position, new Vector3(0.3f, 0.1f, 0.3f), Quaternion.LookRotation(transform.forward, Vector3.up), LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore);
        if(canJump && (input.GetJumpDown() || (input.GetJump() && !sameJump)))
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpStrength, rb.velocity.z);
            remainingJumpPower = 4;
            sameJump = true;
        }
        else if(input.GetJump() && remainingJumpPower > 0)
        {
            rb.AddForce(new Vector3(0, Mathf.Min(8*Time.deltaTime,remainingJumpPower), 0), ForceMode.VelocityChange);
            remainingJumpPower -= 8*Time.deltaTime;
        }
        else if (!input.GetJump())
        {
            remainingJumpPower = 0;
        }

        if (!canJump)
        {
            sameJump = false;
        }
	}


}
