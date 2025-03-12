using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Scriptables/Unit")]
public class UnitScriptable : ScriptableObject
{
    public String UnitName;
    public String UnitTag;
    [SerializeReference] public List<MoveScriptable> MoveList = new();
}
