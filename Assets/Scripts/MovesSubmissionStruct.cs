
using System.Collections.Generic;

public class MovesSubmissionStruct
{
    public class MoveSubmission {
        public string UnitTag = "";
        public string MoveTag = "";
        public string TargetSlotTag = "";

        public MoveSubmission(Combatant combatant){
            UnitTag = combatant.UnitTag;
            MoveTag = combatant.MoveTag;
            TargetSlotTag = combatant.TargetSlotTag;
        }
    }

    public List<MoveSubmission> MoveSubmissions;
}
