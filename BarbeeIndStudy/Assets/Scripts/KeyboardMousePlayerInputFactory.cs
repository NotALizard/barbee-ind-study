using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardMousePlayerInputFactory : PlayerInputFactory {

    public override void Init()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    public override Vector2 LookVec()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }
  
    public override Vector2 WalkVec()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    public override bool GetJump()
    {
        return Input.GetButton("Jump");
    }

    public override bool GetJumpDown()
    {
        return Input.GetButtonDown("Jump");
    }

    public override bool GetFire()
    {
        return Input.GetButton("Fire1");
    }

    public override bool GetFireDown()
    {
        return Input.GetButtonDown("Fire1");
    }

    public override bool GetFireUp()
    {
        return Input.GetButtonUp("Fire1");
    }
}
