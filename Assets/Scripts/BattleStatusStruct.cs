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
    }

    public List<CombatantStatus> ServerCombatantStatuses;
    public List<CombatantStatus> ClientCombatantStatuses;
}
