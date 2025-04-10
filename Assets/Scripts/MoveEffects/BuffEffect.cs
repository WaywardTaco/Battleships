using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Scriptables/MoveEffects/Buff", order = 0)]
public class Buff : IMoveEffect {
    private const float MOVETIME = 0.5f;
    [SerializeField] private string _affectedStat;
    public override void Use(Combatant user, Combatant target, MoveProcessor processor){
    
        int power = user.Move.MovePower;
        processor.StartExternalCoroutine(BuffMove(user.GetSlot().gameObject.transform, target, 0.5f, power));
        
    }

    private IEnumerator BuffMove(Transform user, Combatant target, float displacement, int power){
        Vector3 initialLoc = user.position;
        Vector3 targetLoc = new(initialLoc.x, initialLoc.y + displacement, initialLoc.z);
        float elapsedTime = 0;
        while(elapsedTime < MOVETIME){
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            user.position = Vector3.Lerp(initialLoc, targetLoc, elapsedTime/MOVETIME);
        }

        user.position = targetLoc;
        target.AffectStat(_affectedStat, power);

        elapsedTime = 0;
        while(elapsedTime < MOVETIME){
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            user.position = Vector3.Lerp(targetLoc, initialLoc, elapsedTime/MOVETIME);
        }
        user.position = initialLoc;
    }
}
