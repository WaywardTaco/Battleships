using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class IMoveEffect : ScriptableObject {
    public abstract void Use(Combatant user, Combatant target, MoveData move);
}

[CreateAssetMenu(fileName = "New Effect", menuName = "Scriptables/MoveEffects/Attack", order = 0)]
public class Attack : IMoveEffect {
    public override void Use(Combatant user, Combatant target, MoveData move){
        bool isSpecial = false;
        if(move.MoveKind.CompareTo("Special") == 0) isSpecial = true;

        float power;
        if(!isSpecial)
            power = move.MovePower + user.CurrentAttack() - target.CurrentDefense();
        else
            power = move.MovePower + user.CurrentSpecialAttack() - target.CurrentSpecialDefense();

        if(target.Data.Resistances.Contains(move.MoveType)) power *= 0.5f;
        if(target.Data.Weaknesses.Contains(move.MoveType)) power *= 2.0f;

        int damage = (int)(power / 10.0f);
        target.DealDamage(damage);

        MoveProcessor.Instance.IsProcessingEffect = false;
    }
}

[CreateAssetMenu(fileName = "New Effect", menuName = "Scriptables/MoveEffects/Defend", order = 0)]
public class Defend : IMoveEffect {
    public override void Use(Combatant user, Combatant target, MoveData move){
        throw new NotImplementedException();
    }
}
