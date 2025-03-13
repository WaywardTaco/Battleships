using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class MoveData {
    public String MoveTag;
    public String MoveName;
    public String MoveDescription;
    public String MoveKind;
    public String MoveType;
    public List<String> MoveTargets;
    public List<String> MoveEffect = new();
    public int MovePower;
    public int MoveCostBase;
    public float MoveCostGrowthRate;
}
