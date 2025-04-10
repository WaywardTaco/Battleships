using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class TeamStruct
{
    [Serializable] public class TeamMember {
        public string UnitTag;
        public int Level;
    }
    
    public List<TeamMember> Members;
}
