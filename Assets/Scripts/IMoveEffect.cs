using System;
using System.Collections.Generic;

public abstract class IMoveEffect {
    public abstract void Use(Combatant user, Combatant target, MoveData move);
}

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

public class Defend : IMoveEffect {
    public override void Use(Combatant user, Combatant target, MoveData move){
        throw new NotImplementedException();
    }
}
