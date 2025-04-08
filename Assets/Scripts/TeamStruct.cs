using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamStruct
{
    public class TeamMember {
        public string UnitTag;
        public int Level;

        public TeamMember(Combatant combatant){
            UnitTag = combatant.UnitTag;
            Level = combatant.Level;
        }
    }
    
    public List<TeamMember> Members;
}
