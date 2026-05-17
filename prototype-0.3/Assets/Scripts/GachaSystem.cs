using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pure logic — no MonoBehaviour, no UI.
/// Called by the Summon screen UI, returns HeroInstance list.
/// </summary>
public static class GachaSystem
{
    // ── Pull costs ────────────────────────────────────────
    public const int COST_GEMS_1X   = 300;
    public const int COST_GEMS_10X  = 2700;
    public const int PITY_THRESHOLD = 90;

    // ── Base rates (must sum to 100) ──────────────────────
    static readonly (int stars, float rate)[] BaseRates = new[]
    {
        (5, 1.0f),
        (4, 4.0f),
        (3, 10.0f),
        (2, 25.0f),
        (1, 60.0f)
    };

    // ─────────────────────────────────────────────────────
    // PUBLIC ENTRY POINTS
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// Attempt a 1x pull. Returns null if not enough gems.
    /// </summary>
    public static HeroInstance Pull1x(GameManager gm)
    {
        if (!gm.SpendGems(COST_GEMS_1X)) return null;

        var result = DoSinglePull(gm);
        gm.State.totalSummons++;
        gm.SaveGame();
        return result;
    }

    /// <summary>
    /// Attempt a 10x pull. Returns empty list if not enough gems.
    /// </summary>
    public static List<HeroInstance> Pull10x(GameManager gm)
    {
        if (!gm.SpendGems(COST_GEMS_10X)) return new List<HeroInstance>();

        var results = new List<HeroInstance>();
        for (int i = 0; i < 10; i++)
            results.Add(DoSinglePull(gm));

        gm.State.totalSummons += 10;
        gm.SaveGame();
        return results;
    }

    // ─────────────────────────────────────────────────────
    // CORE ROLL
    // ─────────────────────────────────────────────────────
    static HeroInstance DoSinglePull(GameManager gm)
    {
        gm.State.pityCounter++;

        int stars = RollStars(gm.State.pityCounter);

        // Pity triggered: force 4★ (10% chance for 5★)
        if (gm.State.pityCounter >= PITY_THRESHOLD)
        {
            stars = Random.value < 0.10f ? 5 : 4;
            gm.State.pityCounter = 0;
        }

        HeroData data = PickHeroFromPool(gm, stars);
        if (data == null)
        {
            Debug.LogError($"[GachaSystem] No HeroData found for {stars}★ — check allHeroData in GameManager.");
            return null;
        }

        var instance = new HeroInstance(data);
        gm.AddHeroToRoster(instance);
        return instance;
    }

    // ─────────────────────────────────────────────────────
    // STAR ROLL
    // ─────────────────────────────────────────────────────
    static int RollStars(int currentPity)
    {
        // Soft pity: after 75 pulls, 5★ rate increases linearly
        float fiveStar = 1.0f;
        if (currentPity > 75)
            fiveStar += (currentPity - 75) * 2.5f;  // +2.5% per pull after 75

        float roll = Random.Range(0f, 100f);

        if (roll < fiveStar)  return 5;
        if (roll < fiveStar + 4.0f) return 4;
        if (roll < fiveStar + 14.0f) return 3;
        if (roll < fiveStar + 39.0f) return 2;
        return 1;
    }

    // ─────────────────────────────────────────────────────
    // HERO PICKER (weighted random within star tier)
    // ─────────────────────────────────────────────────────
    static HeroData PickHeroFromPool(GameManager gm, int stars)
    {
        HeroData[] pool = gm.GetPoolByStars(stars);
        if (pool == null || pool.Length == 0) return null;

        float totalWeight = 0f;
        foreach (var h in pool) totalWeight += h.dropWeight;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var h in pool)
        {
            cumulative += h.dropWeight;
            if (roll <= cumulative) return h;
        }

        // Fallback: return last in pool
        return pool[pool.Length - 1];
    }
}
