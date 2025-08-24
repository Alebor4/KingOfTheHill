// ScoreManager.cs
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    float score = 0f;

    public void ResetScore() { score = 0f; }
    public void AddScore(float v) { score += v; }
    public float GetScore() { return score; }

    public string GetDisplayScore()
    {
        return score.ToString("F2");
    }

    // старая формула для матчей (рядом/вертикаль)
    public float CalculateScoreForMatch(int matchedCount, float remainingTime, float totalLevelTime)
    {
        float basePoints = 10f * matchedCount;
        float lengthBonus = 1f + Mathf.Max(0, matchedCount - 3) * 0.25f;
        float timeBonus = (remainingTime / Mathf.Max(1f, totalLevelTime)) * 0.5f;
        float raw = basePoints * lengthBonus * (1f + timeBonus);
        return raw;
    }

    // новая формула для цепочек: даём больше веса длинным цепочкам, возвращаем дробные значения
    public float CalculateScoreForChain(int chainLength, float remainingTime, float totalLevelTime)
    {
        // базовый таргет — 8 очков за тайл
        float basePerTile = 8f;
        float basePoints = basePerTile * chainLength;

        // прогрессивный бонус за длину: квадратичный эффект небольшого масштаба
        float lengthMultiplier = 1f + (chainLength - 1) * 0.12f + Mathf.Pow(chainLength - 1, 1.15f) * 0.01f;

        // небольшой бонус за скорость (если нужен) — если не знаете текущее время, передавайте 0 и не будет бонуса
        float timeBonus = 0f;
        if (totalLevelTime > 0f) timeBonus = (remainingTime / totalLevelTime) * 0.5f;

        float raw = basePoints * lengthMultiplier * (1f + timeBonus);

        // дробные очки — возвращаем с двумя знаками после запятой при отображении
        return raw;
    }
}
