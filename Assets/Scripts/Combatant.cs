using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class Combatant {
    public string UnitTag;
    public string SubmittedMoveTag;
    public string SubmittedTarget;
    public int Level;
    public int CurrentHealth;
    public int CurrentStamina;
    public int SpeedStage;
    public bool isInRear;
    public bool HasDied;
    public UnitData Data {
        get {
            return DataLoader.Instance.GetUnitData(UnitTag);
        }
    }

    public int CurrentSpeed(){
        return SpeedStage;
    }
}
