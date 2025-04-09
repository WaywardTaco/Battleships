using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveProcessor : MonoBehaviour
{
    [Serializable] public class TagEffectTracker{
        public String MoveTag;
        [SerializeReference] public IMoveEffect MoveEffect;
    }

    public bool IsProcessingMove { get; private set; } = false;
    private bool _isProcessingEffect = false;
    [SerializeField] private List<TagEffectTracker> _moveTagEffects = new();
    private Dictionary<String, IMoveEffect> _moveEffectDict = new(); 

    private IEnumerator ProcessMove(Combatant user, CombatantHandler combatantHandler, CombatViewHandler viewHandler){
        /* START PROCESSING MOVE CODE */
        Combatant target = combatantHandler.GetCombatantSlot(user.TargetSlotTag).AssignedCombatant;

        if(user.IsAlly){
            if(target.IsAlly)
                viewHandler.SetMoveFeedbackText($"Your {user.Info.UnitName} used {user.Move.MoveName} on your {target.Info.UnitName}!");
            else
                viewHandler.SetMoveFeedbackText($"Your {user.Info.UnitName} used {user.Move.MoveName} on your opponent's {target.Info.UnitName}!");

        }
        else{
            if(target.IsAlly)
                viewHandler.SetMoveFeedbackText($"Your opponent's {user.Info.UnitName} used {user.Move.MoveName} on your {target.Info.UnitName}!");
            else
                viewHandler.SetMoveFeedbackText($"Your opponent's {user.Info.UnitName} used {user.Move.MoveName} on their {target.Info.UnitName}!");
        }
        
        _isProcessingEffect = false;

        /* PROCESSING ALL MOVE EFFECTS CODE */
        for(int workingEffectIndex = 0; workingEffectIndex < user.Move.MoveEffect.Count; workingEffectIndex++){
            do{
                if(!_isProcessingEffect){
                    _isProcessingEffect = true;
                    Debug.Log($"[DEBUG]: Processing effect {workingEffectIndex} for effect {user.Move.MoveEffect[workingEffectIndex]}");
                    _moveEffectDict[user.Move.MoveEffect[workingEffectIndex]].Use(user, target, this);
                }

                yield return new WaitForEndOfFrame();
            } while (_isProcessingEffect);
        }

        /* END PROCESSING CODE */
        IsProcessingMove = false;
        user.HasMoved = true;
        combatantHandler.UpdateMovingCombatants();
    }

    public void FinishedMoveProcessCallback(){
        _isProcessingEffect = false;
    }

    public void StartExternalCoroutine(IEnumerator routine){
        StartCoroutine(routine);
    }

    public bool IsMoveTargetValid(Combatant user, CombatantSlot slot){
        if(user == null)        return false;
        if(slot == null)        return false;
        if(user.HasDied)        return false;
        if(user.Move == null)   return false;

        List<String> validMoveTargets = user.Move.MoveTargets;
        if(validMoveTargets.Count == 0) return false;

        if(slot.AssignedCombatant == user){
                    if(validMoveTargets.Contains("Self")) return true;
                    if(validMoveTargets.Contains("Party")) return true;
        } else {
            if(slot.AssignedCombatant.IsAlly){
                if(slot.AssignedCombatant.HasDied){
                    if(validMoveTargets.Contains("DeadAlly")) return true;
                } else {
                    if(validMoveTargets.Contains("Party")) return true;
                    if(validMoveTargets.Contains("Ally")) return true;
                }
            } else {
                    if(validMoveTargets.Contains("Single")) return true;
                    if(validMoveTargets.Contains("Spread")) return true;
            }
        }

        return false;
    }

    public void Process(Combatant user, CombatantHandler combatantHandler, CombatViewHandler viewHandler){
        if(IsProcessingMove) return;

        IsProcessingMove = true;
        StartCoroutine(ProcessMove(user, combatantHandler, viewHandler));
    }

    public void Initialize(){
        _moveEffectDict.Clear();

        foreach(TagEffectTracker tracker in _moveTagEffects)
            _moveEffectDict.Add(tracker.MoveTag, tracker.MoveEffect);
    }
}
