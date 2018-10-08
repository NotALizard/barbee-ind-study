using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstracts player input to enable support of 
/// </summary>
public abstract class PlayerInputFactory {

    public abstract void Init();

    /// <summary>
    /// Reads input for look direction. This vector is NOT normalized
    /// </summary>
    /// <returns>Vector2 rotation about y axis, rotation about x axis look</returns>
    public abstract Vector2 LookVec();

    /// <summary>
    /// Reads input for walk direction. This vector is normalized
    /// </summary>
    /// <returns>Vector2 x,z movement</returns>
    public abstract Vector2 WalkVec();

    public abstract bool GetJump();

    public abstract bool GetJumpDown();

    public abstract bool GetFire();

    public abstract bool GetFireDown();

    public abstract bool GetFireUp();
  
}
