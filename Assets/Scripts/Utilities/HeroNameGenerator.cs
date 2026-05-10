using UnityEngine;

public static class HeroNameGenerator
{
    private static readonly string[] FirstNames = new[]
    {
        "Beru", "Igris", "Tank", "Shadow", "Iron", "Bellion", "Greed", 
        "Lime", "Esil", "Tusk", "Kaisel", "Ashborn", "Cha", "Rakan", 
        "Baran", "Sillad", "Phantom", "Crimson", "Frost", "Storm"
    };
    
    private static readonly string[] Titles = new[]
    {
        "the Fallen", "of Shadows", "the Eternal", "of Flames", "the Undying",
        "of Doom", "the Brave", "of Light", "the Reaper", "of Chaos",
        "the Silent", "of Thunder", "the Monarch", "the Walker", "of Void"
    };

    public static string Generate(int starRating)
    {
        string name = FirstNames[Random.Range(0, FirstNames.Length)];
        
        // Higher stars = more epic names
        if (starRating >= 4)
            name += " " + Titles[Random.Range(0, Titles.Length)];
        else if (starRating == 3 && Random.value > 0.5f)
            name += " " + Titles[Random.Range(0, Titles.Length)];
            
        return name;
    }
}
