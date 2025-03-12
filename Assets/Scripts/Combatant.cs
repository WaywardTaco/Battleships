using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class Combatant {
    public string UnitTag;
    public int CurrentHealth;
    public int CurrentStamina;
    public bool isInRear;
    public bool HasDied;
}
