namespace Ateliers.Voice.Engines.VoicePeakTools;

public class VoicePeakNarraterFactory
{
    public static IVoicePeakNarrator CreateNarratorByName(string name, bool throwIfNotFound = true)
    {
        return name.ToLower() switch
        {
            "フリモメン" or "frimomen" or "furimomen"  => new Narrators.Frimomen(),
            "夏色花梨" or "natukikarin" => new Narrators.NatukiKarin(),
            "ポロンちゃん" or "poronchan" => new Narrators.Poronchan(),
            _ => throwIfNotFound ? throw new ArgumentException($"Unknown narrator name: {name}") : new Narrators.UnknownNarrator(name)
        };
    }

    public static T CreateNarratorByName<T>(string name, bool throwIfNotFound = true) where T : IVoicePeakNarrator
    {
        var narrator = CreateNarratorByName(name, throwIfNotFound);
        if (narrator is T typedNarrator)
        {
            return typedNarrator;
        }
        else
        {
            throw new InvalidCastException($"Cannot cast narrator of type {narrator.GetType().Name} to {typeof(T).Name}");
        }
    }

    public static IEnumerable<IVoicePeakNarrator> CreateAllNarrators()
    {
        yield return new Narrators.Frimomen();
        yield return new Narrators.NatukiKarin();
        yield return new Narrators.Poronchan();
    }
}
