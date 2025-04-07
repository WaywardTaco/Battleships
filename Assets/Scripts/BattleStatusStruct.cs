using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStatusStruct 
{
    public class CombatantStatus{
        public string UnitTag = "";
        public int CurrentHealth = 0;
        public int CurrentStamina = 0;
        public int ATKStage = 0;
        public int SPAStage = 0;
        public int DEFStage = 0;
        public int SPDStage = 0;
        public int SPEStage = 0;
        public bool HasDied = false;
        public CombatantStatus(Combatant combatant){
            UnitTag = combatant.UnitTag;
            CurrentHealth = combatant.CurrentHealth;
            CurrentStamina = combatant.CurrentStamina;
            ATKStage = combatant.ATKStage;
            SPAStage = combatant.SPAStage;
            DEFStage = combatant.DEFStage;
            SPDStage = combatant.SPDStage;
            SPEStage = combatant.SPEStage;
            HasDied = combatant.HasDied;
        }
    }

    public List<CombatantStatus> ServerCombatantStatuses;
    public List<CombatantStatus> ClientCombatantStatuses;
}
