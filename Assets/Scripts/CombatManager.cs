using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private GameObject _combatPanel;
    [SerializeField] private bool _debugStartCombat;
    [SerializeField] private List<CombatantSlot> _playerSlots;
    [SerializeField] private List<CombatantSlot> _enemySlots;
    [SerializeField] private List<Combatant> _playerTeam;
    [SerializeField] private List<Combatant> _enemyTeam;

    public void StartCombat(){
        _combatPanel.SetActive(true);
    }
    private void EndCombat(){

    }
    private void Setup(){
        _combatPanel.SetActive(false);
    }

    private void DebugUpdate(){
        if(_debugStartCombat){
            _debugStartCombat = false;
            StartCombat();
        }
    }

    void Update()
    {
        
    }

    public static CombatManager Instance { get; private set;}
    void Start(){
        if(Instance == null){
            Instance = this;
        } else {
            Destroy(this);
        }

        Setup();
    }
    void OnDestroy()
    {
        if(Instance == this){
            Instance = null;
        }
    }
}
