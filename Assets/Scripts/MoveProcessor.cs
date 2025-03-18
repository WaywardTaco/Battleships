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
    public bool IsProcessingEffect = false;
    [SerializeField] private bool _debugEndProcessMove = false;
    [SerializeField] private List<TagEffectTracker> _moveTagEffects = new();
    private Dictionary<String, IMoveEffect> _moveEffectDict = new(); 

    private IEnumerator ProcessMove(Combatant user){
        Debug.Log($"[COMBAT]: Processing {user.UnitTag}'s {user.SubmittedMoveTag} (Speed: {user.CurrentSpeed()})");

        Combatant target = CombatManager.Instance.GetCombatantSlot(user.SubmittedSlotTargetTag).AssignedCombatant;
        MoveData move = DataLoader.Instance.GetMoveData(user.SubmittedMoveTag);
        
        IsProcessingEffect = false;

        int workingEffectTagIndex = 0;
        while(workingEffectTagIndex <= move.MoveEffect.Count || IsProcessingEffect){
            // Await async effects before processing next move
            if(!IsProcessingEffect && workingEffectTagIndex <= move.MoveEffect.Count){
                IsProcessingEffect = true;
                _moveEffectDict[move.MoveEffect[workingEffectTagIndex]].Use(user, target, move);
                workingEffectTagIndex++;
            }

            yield return new WaitForEndOfFrame();
        }

        IsProcessingMove = false;
    }

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

    private void Initialize(){
        _moveEffectDict.Clear();

        foreach(TagEffectTracker tracker in _moveTagEffects)
            _moveEffectDict.Add(tracker.MoveTag, tracker.MoveEffect);
    }

    public static MoveProcessor Instance { get; private set;}
    void Start() {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);

        Initialize();
    }
    void OnDestroy(){
        if(Instance == this)
            Instance = null;
    }
}
