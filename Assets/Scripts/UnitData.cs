using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class UnitData {
    public String UnitTag;
    public String UnitName;
    public int HP_Base;
    public float HP_Growth;
    public int SP_Base;
    public float SP_Growth;
    public int ATK_Base;
    public float ATK_Growth;
    public int DEF_Base;
    public float DEF_Growth;
    public int SPA_Base;
    public float SPA_Growth;
    public int SPD_Base;
    public float SPD_Growth;
    public int SPE_Base;
    public float SPE_Growth;

    public List<String> Resistances = new();
    public List<String> Weaknesses = new();
    public List<String> SpriteList = new();
    public List<String> MoveList = new();
}
