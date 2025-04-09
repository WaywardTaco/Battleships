using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Scriptables/MoveEffects/Attack", order = 0)]
public class Attack : IMoveEffect {
    public override void Use(Combatant user, Combatant target, MoveProcessor processor){
        
        bool isSpecial = false;
        if(user.Move.MoveKind.CompareTo("Special") == 0) isSpecial = true;

        float power;
        if(!isSpecial)
            power = user.Move.MovePower + user.CurrentAttack() - target.CurrentDefense();
        else
            power = user.Move.MovePower + user.CurrentSpecialAttack() - target.CurrentSpecialDefense();

        if(target.Info.Resistances.Contains(user.Move.MoveType)) power *= 0.5f;
        if(target.Info.Weaknesses.Contains(user.Move.MoveType)) power *= 2.0f;

        int damage = (int)(power / 10.0f);
        target.DealDamage(damage);
        Debug.Log($"{user.UnitTag} used an attack on {target.UnitTag} and dealt {damage} damage!");

        processor.FinishedMoveProcessCallback();
    }
}
