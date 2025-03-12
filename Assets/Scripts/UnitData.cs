using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class UnitData {
    public String UnitTag;
    public String UnitName;
    public List<String> SpriteList = new();
    public List<String> MoveList = new();
}
