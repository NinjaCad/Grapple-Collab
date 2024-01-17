using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct PlayerEvents
{
    #region Movement
    public Action<Vector2> OnXYInput;
    public Action OnJumpButtonDown;
    public Action OnJumpButtonUp;
    #endregion

    #region Grapple
    public Action OnGrappleButtonDown;
    public Action<Vector2> OnGrapple;
    public Action OnGrappleButtonUp;
    public Action OnPullButtonDown;
    #endregion

    #region Death and Respawn
    public Action OnDeath;
    public Action OnRespawn;
    #endregion
}
