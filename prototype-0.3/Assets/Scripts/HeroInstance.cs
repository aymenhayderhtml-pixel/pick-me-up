using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime data for ONE specific summoned hero.
/// Serialized to JSON for save/load.
/// HeroData = template (ScriptableObject, read-only)
/// HeroInstance = this individual hero's life story
/// </summary>
[Serializable]
public class HeroInstance
{
    // ── Identity ──────────────────────────────────────────
    public string instanceId;           // GUID, unique per hero
    public string heroDataId;           // matches HeroData.name (SO asset name)
    public string heroName;             // copy from HeroData at summon time
    public int starRating;
    public int synthesisPromotions = 0; // tracks number of synthesis promotions applied

    // ── Progression ───────────────────────────────────────
    public int level;                   // 1 to 50
    public int currentXP;
    public int xpToNextLevel;

    // ── Live Stats (calculated from base + level) ─────────
    public int maxHP;
    public int currentHP;
    public int atk;
    public int def;
    public int spd;
    public float critChance;
    public float critMult;

    // ── Personality & Morale ──────────────────────────────
    public PersonalityTrait trait;
    public int morale;                  // 0-100
    public HeroStatus status;           // Active, Fatigued, Wounded, Dead

    // ── History (attachment system) ───────────────────────
    public int missionsCompleted;
    public int floorsCleared;
    public int kills;
    public int nearDeathMoments;        // survived with < 10% HP
    public string earnedTitle;          // "Kael the Enduring" after 10 missions
    public List<string> battleLog;      // last 5 notable events

    // ── State ─────────────────────────────────────────────
    public bool isDeployed;             // currently in a squad
    public bool isNew;                  // just summoned, show NEW badge
    public string dateObtained;         // ISO string
    public string dateDied;             // set on permadeath

    // ─────────────────────────────────────────────────────
    // Constructor — called by GachaSystem when a hero is summoned
    // ─────────────────────────────────────────────────────
    public HeroInstance(HeroData data)
    {
        instanceId    = Guid.NewGuid().ToString();
        heroDataId    = data.name;
        heroName      = data.heroName;
        starRating    = data.starRating;
        level         = 1;
        currentXP     = 0;
        xpToNextLevel = CalculateXPThreshold(1);
        morale        = 80;
        status        = HeroStatus.Active;
        isNew         = true;
        dateObtained  = DateTime.UtcNow.ToString("o");
        battleLog     = new List<string>();
        kills         = 0;
        missionsCompleted = 0;
        floorsCleared = 0;
        nearDeathMoments  = 0;
        earnedTitle   = "";

        // Pick a random trait from the data's possible traits
        if (data.possibleTraits != null && data.possibleTraits.Count > 0)
            trait = data.possibleTraits[UnityEngine.Random.Range(0, data.possibleTraits.Count)];
        else
            trait = PersonalityTrait.Stoic;

        RecalculateStats(data);
    }

    // ─────────────────────────────────────────────────────
    // Stat calculation: base + (level-1) * perLevel scalar
    // ─────────────────────────────────────────────────────
    public void RecalculateStats(HeroData data)
    {
        int lvl = level - 1;
        int bHP = data.baseStats != null ? data.baseStats.hp : 50;
        int bATK = data.baseStats != null ? data.baseStats.atk : 10;
        int bDEF = data.baseStats != null ? data.baseStats.def : 5;
        int bSPD = data.baseStats != null ? data.baseStats.spd : 10;

        maxHP     = Mathf.RoundToInt(bHP  + lvl * data.hpPerLevel);
        atk       = Mathf.RoundToInt(bATK + lvl * data.atkPerLevel);
        def       = Mathf.RoundToInt(bDEF + lvl * data.defPerLevel);
        spd       = Mathf.RoundToInt(bSPD + lvl * data.spdPerLevel);

        // Apply 15% synthesis promotion stat boost per promotion
        if (synthesisPromotions > 0)
        {
            float mult = Mathf.Pow(1.15f, synthesisPromotions);
            maxHP = Mathf.RoundToInt(maxHP * mult);
            atk   = Mathf.RoundToInt(atk * mult);
            def   = Mathf.RoundToInt(def * mult);
            spd   = Mathf.RoundToInt(spd * mult);
        }

        currentHP  = maxHP;
        critChance = data.baseCritChance;
        critMult   = data.baseCritMult;

        // Trait modifiers
        ApplyTraitModifiers();
    }

    void ApplyTraitModifiers()
    {
        switch (trait)
        {
            case PersonalityTrait.Cautious:
                def = Mathf.RoundToInt(def * 1.10f);
                atk = Mathf.RoundToInt(atk * 0.95f);
                break;
            // Brave is handled in CombatManager at runtime (conditional on HP)
            // Loyal/Rebellious/Compassionate/Stoic handled in MoraleSystem
        }
    }

    // ─────────────────────────────────────────────────────
    // XP & levelling
    // ─────────────────────────────────────────────────────
    /// <summary>Returns true if levelled up.</summary>
    public bool AddXP(int amount, HeroData data)
    {
        currentXP += amount;
        if (currentXP >= xpToNextLevel && level < 50)
        {
            currentXP -= xpToNextLevel;
            level++;
            xpToNextLevel = CalculateXPThreshold(level);
            RecalculateStats(data);
            return true;
        }
        return false;
    }

    static int CalculateXPThreshold(int level)
    {
        // Simple quadratic curve: 10, 22, 38, 58 ...
        return Mathf.RoundToInt(10 * level + 2 * level * level);
    }

    // ─────────────────────────────────────────────────────
    // Morale helpers
    // ─────────────────────────────────────────────────────
    public void ModifyMorale(int delta)
    {
        // Loyal trait: dampen negative morale changes
        if (delta < 0 && trait == PersonalityTrait.Loyal)
            delta = Mathf.RoundToInt(delta * 0.6f);

        morale = Mathf.Clamp(morale + delta, 0, 100);
    }

    public bool IsRebelling()
    {
        return trait == PersonalityTrait.Rebellious && morale < 30;
    }

    // ─────────────────────────────────────────────────────
    // Death
    // ─────────────────────────────────────────────────────
    public void Die()
    {
        status    = HeroStatus.Dead;
        currentHP = 0;
        dateDied  = DateTime.UtcNow.ToString("o");
        isDeployed = false;
    }

    // ─────────────────────────────────────────────────────
    // Title system
    // ─────────────────────────────────────────────────────
    public void CheckAndAssignTitle()
    {
        if (missionsCompleted >= 10 && string.IsNullOrEmpty(earnedTitle))
        {
            earnedTitle = heroName + " the Enduring";
        }
        else if (floorsCleared >= 30 && !earnedTitle.Contains("Veteran"))
        {
            earnedTitle = heroName + " the Veteran";
        }
    }

    public bool HasTrait(PersonalityTrait trait) => this.trait == trait;

    public void AddHistory(string entry)
    {
        if (battleLog == null) battleLog = new List<string>();
        battleLog.Add(entry);
        if (battleLog.Count > 5) battleLog.RemoveAt(0);
    }

    public List<string> GetHistory()
    {
        if (battleLog == null) battleLog = new List<string>();
        return battleLog;
    }

    // Serialization field
    public bool essenceExtracted = false;

    // UI properties
    public bool EssenceExtracted
    {
        get => essenceExtracted;
        set => essenceExtracted = value;
    }

    public HeroStatus Status
    {
        get => status;
        set => status = value;
    }

    public HeroData data => GameManager.Instance.GetHeroData(heroDataId);
    public string HeroName => heroName;
    public string Title => earnedTitle;
    public string Id => instanceId;

    // ─────────────────────────────────────────────────────
    // Display helpers
    // ─────────────────────────────────────────────────────
    public string DisplayName => string.IsNullOrEmpty(earnedTitle) ? heroName : earnedTitle;

    public float HPPercent => maxHP > 0 ? (float)currentHP / maxHP : 0f;

    public int Power => atk + def + (maxHP / 10) + spd;
}

public enum HeroStatus
{
    Active,     // ready to deploy
    Fatigued,   // needs rest (morale < 20)
    Wounded,    // HP not full, needs healing
    Deployed,   // currently in a mission
    Dead        // in Memorial Hall
}
