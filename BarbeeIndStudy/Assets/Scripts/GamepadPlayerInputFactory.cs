using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamepadPlayerInputFactory : PlayerInputFactory {

    public override void Init()
    {
        Cursor.visible = false;
    }

    public override Vector2 LookVec()
    {
        return new Vector2(Input.GetAxis("LookHorizontalGP"), Input.GetAxis("LookVerticalGP"));
    }

    public override Vector2 WalkVec()
    {
        return new Vector2(Input.GetAxisRaw("HorizontalGP"), Input.GetAxisRaw("VerticalGP")).normalized;
    }

    public override bool GetJump()
    {
        return Input.GetButton("JumpGP");
    }

    public override bool GetJumpDown()
    {
        return Input.GetButtonDown("JumpGP");
    }

    public override bool GetFire()
    {
        return Input.GetButton("Fire1GP");
    }

    public override bool GetFireDown()
    {
        return Input.GetButtonDown("Fire1GP");
    }

    public override bool GetFireUp()
    {
        return Input.GetButtonUp("Fire1GP");
    }
}
