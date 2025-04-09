using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Scriptables/MoveEffects/Defend", order = 0)]
public class Defend : IMoveEffect {
    public override void Use(Combatant user, Combatant target, MoveProcessor processor){
        throw new NotImplementedException();
    }
}
