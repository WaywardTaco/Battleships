using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Scriptables/MoveEffects/Recoil", order = 0)]
public class Recoil : IMoveEffect {
    private const float MOVETIME = 0.5f;
    public override void Use(Combatant user, Combatant target, MoveProcessor processor){
        
        float levelRecoil = user.Move.MoveCostGrowthRate * user.Level;
        float power = user.Move.MovePower + levelRecoil;

        if(user.Info.Resistances.Contains(user.Move.MoveType)) power *= 0.5f;

        int damage = (int)(power / 20.0f);

        user.DealDamage(damage);
    }
}
