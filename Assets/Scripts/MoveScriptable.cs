using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Move", menuName = "Scriptables/Move")]
public class MoveScriptable : ScriptableObject
{
    public String MoveName;
    public String MoveDescription;
    public String MoveKind;
    public String MoveType;
    public String MoveSpread;
    public List<String> MoveEffect = new();
    public int MovePower;
    public int MoveCostBase;
    public float MoveCostGrowthRate;

    public void UseMove(){
        Debug.Log($"[DEBUG]: I'm using {MoveName}");
    }
}
