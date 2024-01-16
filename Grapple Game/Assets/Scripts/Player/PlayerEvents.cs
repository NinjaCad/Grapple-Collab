using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct PlayerEvents
{
    #region Movement
    public Action<Vector2> OnXYInput;
    public Action OnJumpDown;
    public Action OnJumpUp;
    #endregion

    #region Grapple
    public Action OnGrappleDown;
    public Action OnGrappleUp;
    public Action OnPullDown;
    #endregion

    #region Death and Respawn
    public Action OnDeath;
    public Action OnRespawn;
    #endregion
}
