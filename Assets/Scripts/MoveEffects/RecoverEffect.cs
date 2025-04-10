using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Scriptables/MoveEffects/RecoverEffect", order = 0)]
public class RecoverEffect : IMoveEffect {
    private const float MOVETIME = 0.5f;
    public override void Use(Combatant user, Combatant target, MoveProcessor processor){
        
        int recovery = user.Move.MovePower;

        processor.StartExternalCoroutine(RecoverMove(user.GetSlot().gameObject.transform, target, 0.5f, recovery));
    }

    private IEnumerator RecoverMove(Transform user, Combatant target, float displacement, int recoverAmt){
        Vector3 initialLoc = user.position;
        Vector3 targetLoc = new(initialLoc.x, initialLoc.y + displacement, initialLoc.z);
        float elapsedTime = 0;
        while(elapsedTime < MOVETIME){
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            user.position = Vector3.Lerp(initialLoc, targetLoc, elapsedTime/MOVETIME);
        }

        user.position = targetLoc;
        target.AffectStamina(recoverAmt);

        elapsedTime = 0;
        while(elapsedTime < MOVETIME){
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            user.position = Vector3.Lerp(targetLoc, initialLoc, elapsedTime/MOVETIME);
        }
        user.position = initialLoc;
    }
}
