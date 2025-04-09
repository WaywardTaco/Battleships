using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Scriptables/MoveEffects/Attack", order = 0)]
public class Attack : IMoveEffect {
    private const float MOVETIME = 0.5f;
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
        processor.StartExternalCoroutine(MoveToTarget(user.GetSlot().gameObject.transform, target, target.GetSlot().gameObject.transform.position, damage));
        
        Debug.Log($"{user.UnitTag} used an attack on {target.UnitTag} and dealt {damage} damage!");
    }

    private IEnumerator MoveToTarget(Transform user, Combatant target, Vector3 targetLocWorldPos, int dmgAmt){
        Vector3 initialLoc = user.position;
        float elapsedTime = 0;
        while(elapsedTime < MOVETIME){
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            user.position = Vector3.Lerp(initialLoc, targetLocWorldPos, elapsedTime/MOVETIME);
        }

        user.position = targetLocWorldPos;
        target.DealDamage(dmgAmt);

        elapsedTime = 0;
        while(elapsedTime < MOVETIME){
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            user.position = Vector3.Lerp(targetLocWorldPos, initialLoc, elapsedTime/MOVETIME);
        }
        user.position = initialLoc;
    }
}
