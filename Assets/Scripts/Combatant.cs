using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class Combatant {
    private const float ATK_STAGE_MULT = 0.5f;
    private const float SPA_STAGE_MULT = 0.5f;
    private const float DEF_STAGE_MULT = 0.5f;
    private const float SPD_STAGE_MULT = 0.5f;
    private const float SPE_STAGE_MULT = 0.25f;

    public string UnitTag;
    public string SubmittedMoveTag;
    public string SubmittedSlotTargetTag;
    public int Level;
    public int CurrentHealth;
    public int CurrentStamina;
    public int ATKStage;
    public int SPAStage;
    public int DEFStage;
    public int SPDStage;
    public int SPEStage;
    public bool isAlly;
    public bool HasDied;
    public UnitData Data {
        get {return DataLoader.Instance.GetUnitData(UnitTag);}
    }

    public void Initialize(){
        CurrentHealth = MaxHP();
        CurrentStamina = MaxSP();
        HasDied = false;
    }

    public int CurrentAttack(){
        return (int)(((float)Data.ATK_Base + (Data.ATK_Growth * (float)Level)) * (((float)ATKStage * ATK_STAGE_MULT) + 1.0f));
    }
    public int CurrentSpecialAttack(){
        return (int)(((float)Data.SPA_Base + (Data.SPA_Growth * (float)Level)) * (((float)SPAStage * SPA_STAGE_MULT) + 1.0f));
    }
    public int CurrentDefense(){
        return (int)(((float)Data.DEF_Base + (Data.DEF_Growth * (float)Level)) * (((float)DEFStage * DEF_STAGE_MULT) + 1.0f));
    }
    public int CurrentSpecialDefense(){
        return (int)(((float)Data.SPD_Base + (Data.SPD_Growth * (float)Level)) * (((float)SPDStage * SPD_STAGE_MULT) + 1.0f));
    }
    public int CurrentSpeed(){
        return (int)(((float)Data.SPE_Base + (Data.SPE_Growth * (float)Level)) * (((float)SPEStage * SPE_STAGE_MULT) + 1.0f));
    }

    public int MaxHP(){
        return (int)((float)Data.HP_Base + (Data.HP_Growth * (float)Level));
    }
    public int MaxSP(){
        return (int)((float)Data.SP_Base + (Data.SP_Growth * (float)Level));
    }

    public void DealDamage(int amount){
        if(amount < 0) amount = 0;

        if(CurrentHealth - amount > 0){
            CurrentHealth -= amount;
        } else {
            CurrentHealth = 0;
            HasDied = true;
        }
    }

    public void Heal(int amount, bool doesRevive = false){
        if(amount < 0) amount = 0;

        if(HasDied && !doesRevive) return;

        if(CurrentHealth + amount < MaxHP()){
            CurrentHealth+= amount;
        } else {
            CurrentHealth = MaxHP();
        }

        if(CurrentHealth > 0 && doesRevive) 
            HasDied = false;
    }
}
