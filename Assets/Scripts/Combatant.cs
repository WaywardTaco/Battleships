using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class Combatant {
    private const int STAT_MAXSTAGE = 5;
    private const float ATK_STAGE_MULT = 0.5f;
    private const float SPA_STAGE_MULT = 0.5f;
    private const float DEF_STAGE_MULT = 0.5f;
    private const float SPD_STAGE_MULT = 0.5f;
    private const float SPE_STAGE_MULT = 0.25f;

    public string UnitTag = "";
    public int Level = 10;
    [HideInInspector] public string MySlotTag = "";
    [HideInInspector] public string MoveTag = "";
    [HideInInspector] public string TargetSlotTag = "";
    public int CurrentHealth = 0;
    public int CurrentStamina = 0;
    [HideInInspector] public int ATKStage = 0;
    [HideInInspector] public int SPAStage = 0;
    [HideInInspector] public int DEFStage = 0;
    [HideInInspector] public int SPDStage = 0;
    [HideInInspector] public int SPEStage = 0;
    [HideInInspector] public bool IsAlly = false;
    [HideInInspector] public bool HasDied = false;
    [HideInInspector] public bool HasMoved = false;
    public UnitData Info {
        get { return DataLoader.Instance.GetUnitData(UnitTag); }
    }
    public MoveData Move {
        get { return DataLoader.Instance.GetMoveData(MoveTag); }
    }
    public bool HasMove {
         get { return MoveTag.CompareTo("") != 0; }
    }
    public bool HasTarget {
        get { return TargetSlotTag.CompareTo("") != 0; }
    }
    public Sprite GetSprite(int index){
        return DataLoader.Instance.GetUnitSprite(Info.SpriteList[index]);
    }

    public CombatantSlot GetSlot(){
        return CombatManager.Instance.GetSlot(MySlotTag);
    }

    public bool HasEnoughSP(int amount){
        return CurrentStamina >= amount;
    }

    public bool HasStatStage{
        get {
            if(ATKStage != 0) return true;
            if(SPAStage != 0) return true;
            if(DEFStage != 0) return true;
            if(SPDStage != 0) return true;
            if(SPEStage != 0) return true;

            return false;
        }
    }

    public void AffectStat(string stat, int amount){
        
        int referenceStatStage = 0;
        switch(stat){
            case "ATK": referenceStatStage = ATKStage; break;
            case "SPA": referenceStatStage = SPAStage; break;
            case "DEF": referenceStatStage = DEFStage; break;
            case "SPD": referenceStatStage = SPDStage; break;
            case "SPE": referenceStatStage = SPEStage; break;
        }

        if(referenceStatStage + amount > STAT_MAXSTAGE)
            amount = STAT_MAXSTAGE - referenceStatStage;
        if(referenceStatStage + amount < -STAT_MAXSTAGE)
            amount = -STAT_MAXSTAGE - referenceStatStage;

        switch(stat){
            case "ATK": ATKStage += amount; break;
            case "SPA": SPAStage += amount; break;
            case "DEF": DEFStage += amount; break;
            case "SPD": SPDStage += amount; break;
            case "SPE": SPEStage += amount; break;
        }

        GetSlot().AffectStat(stat, amount);
    }

    public void Initialize(){
        CurrentHealth = MaxHP;
        CurrentStamina = MaxSP;
        HasDied = false;
    }

    public int CurrentAttack(){
        return (int)(((float)Info.ATK_Base + (Info.ATK_Growth * (float)Level)) * (((float)ATKStage * ATK_STAGE_MULT) + 1.0f));
    }
    public int CurrentSpecialAttack(){
        return (int)(((float)Info.SPA_Base + (Info.SPA_Growth * (float)Level)) * (((float)SPAStage * SPA_STAGE_MULT) + 1.0f));
    }
    public int CurrentDefense(){
        return (int)(((float)Info.DEF_Base + (Info.DEF_Growth * (float)Level)) * (((float)DEFStage * DEF_STAGE_MULT) + 1.0f));
    }
    public int CurrentSpecialDefense(){
        return (int)(((float)Info.SPD_Base + (Info.SPD_Growth * (float)Level)) * (((float)SPDStage * SPD_STAGE_MULT) + 1.0f));
    }
    public int CurrentSpeed(){
        return (int)(((float)Info.SPE_Base + (Info.SPE_Growth * (float)Level)) * (((float)SPEStage * SPE_STAGE_MULT) + 1.0f));
    }

    public int MaxHP{
        get {
            return (int)((float)Info.HP_Base + (Info.HP_Growth * (float)Level));
        }
    }
    public int MaxSP{
        get {
            return (int)((float)Info.SP_Base + (Info.SP_Growth * (float)Level));
        }
    }

    public void DealDamage(int amount){
        if(amount < 0) amount = 0;

        if(CurrentHealth < amount)
            amount = CurrentHealth;

        CurrentHealth -= amount;

        GetSlot().DealDamage(amount);

        if(CurrentHealth <= 0){
            CurrentHealth = 0;
            HasDied = true;
            GetSlot().DieAnim();
        }
    }

    public void Heal(int amount, bool doesRevive = false){
        if(amount < 0) amount = 0;

        if(HasDied && !doesRevive) return;

        if(CurrentHealth + amount > MaxHP)
            amount = MaxHP - CurrentHealth;
            
        CurrentHealth += amount;
        GetSlot().Heal(amount);

        if(CurrentHealth > 0 && doesRevive){
            GetSlot().ReviveAnim();
            HasDied = false;
        }
    }

    public void AffectStamina(int amount){

        if(CurrentStamina + amount > MaxSP)
            amount = MaxSP - CurrentHealth;
        if(CurrentStamina + amount < 0)
            amount = 0 - CurrentStamina;
        if(CurrentStamina > MaxSP)
            CurrentStamina = MaxSP;
        if(CurrentStamina < 0)
            CurrentStamina = 0;
            
        CurrentStamina += amount;
        if(amount != 0)
            GetSlot().AffectStamina(amount);
    }

    

    public void UpdateMoveSubmission(MovesSubmissionStruct.MoveSubmission submission){
        if(UnitTag.CompareTo(submission.UnitTag) != 0) 
            return;

        MoveTag = submission.MoveTag;
        TargetSlotTag = submission.TargetSlotTag;
    }

    public void UpdateStatus(BattleStatusStruct.CombatantStatus status){
        if(UnitTag.CompareTo(status.UnitTag) != 0) 
            return;

        CurrentHealth = status.CurrentHealth;
        CurrentStamina = status.CurrentStamina;
        ATKStage = status.ATKStage;
        SPAStage = status.SPAStage;
        DEFStage = status.DEFStage;
        SPDStage = status.SPDStage;
        SPEStage = status.SPEStage;
        HasDied = status.HasDied;
    }
}
