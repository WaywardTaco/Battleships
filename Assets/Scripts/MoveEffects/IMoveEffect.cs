using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class IMoveEffect : ScriptableObject {
    public abstract void Use(Combatant user, Combatant target, MoveProcessor processor);
}