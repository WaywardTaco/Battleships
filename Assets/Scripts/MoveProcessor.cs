using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveProcessor : MonoBehaviour
{
    public bool IsProcessingMove { get; private set; } = false;
    [SerializeField] private bool _debugEndProcessMove = false;

    public bool IsTargetSlotValid(Combatant user, CombatantSlot slot){
        if(user == null) return false;
        Debug.Log("User is not null");
        if(user.HasDied) return false;
        Debug.Log("User is not dead");

        if(slot == null) return false;
        Debug.Log("Slot is not null");

        MoveData moveData = DataLoader.Instance.GetMoveData(user.SubmittedMoveTag);
        if(moveData == null) return false;
        Debug.Log("Move Data is not null");

        List<String> validMoveTargets = moveData.MoveTargets;
        if(validMoveTargets.Count == 0) return false;
        Debug.Log("move has valid targets");

        if(slot.AssignedCombatant == user){
            Debug.Log("Target is self");

            if(validMoveTargets.Contains("Self")) return true;
            if(validMoveTargets.Contains("Party")) return true;
        } else {
            Debug.Log("Target is not self");
            if(slot.AssignedCombatant.isAlly){
                Debug.Log("Target is ally");
                if(slot.AssignedCombatant.HasDied){
                    Debug.Log("Target is dead");
                    if(validMoveTargets.Contains("DeadAlly")) return true;
                } else {
                    Debug.Log("Target is not dead");
                    if(validMoveTargets.Contains("Party")) return true;
                    if(validMoveTargets.Contains("Ally")) return true;
                }
            } else {
                Debug.Log("Target is not ally");
                if(validMoveTargets.Contains("Single")) return true;
                if(validMoveTargets.Contains("Spread")) return true;
            }
        }

        return false;
    }

    public void Process(Combatant user){
        if(user.HasDied) return;

        IsProcessingMove = true;
        StartCoroutine(ProcessMove(user));
    }
    
    private IEnumerator ProcessMove(Combatant user){
        Debug.Log($"[COMBAT]: Processing {user.UnitTag}'s {user.SubmittedMoveTag} (Speed: {user.CurrentSpeed()})");

        while(!_debugEndProcessMove){
            yield return new WaitForEndOfFrame();
        }

        _debugEndProcessMove = false;

        IsProcessingMove = false;
    }

    public static MoveProcessor Instance { get; private set;}
    void Start() {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    void OnDestroy(){
        if(Instance == this)
            Instance = null;
    }
}
