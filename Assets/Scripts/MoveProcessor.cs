using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveProcessor : MonoBehaviour
{
    public bool IsProcessingMove { get; private set; } = false;
    [SerializeField] private bool _debugEndProcessMove = false;

    public bool HasValidTarget(Combatant user){
        return true;
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
