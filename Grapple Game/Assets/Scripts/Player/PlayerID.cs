using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ID", menuName = "ScriptableObjects/PlayerID", order = 2)]
public class PlayerID : ScriptableObject
{
    public PlayerData data;
    public PlayerEvents events;
}
