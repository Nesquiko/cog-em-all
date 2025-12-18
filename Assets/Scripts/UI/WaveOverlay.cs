using TMPro;
using UnityEngine;

public class WaveOverlay : MonoBehaviour
{
    [SerializeField] private TMP_Text waveText;

    public void Initialize(int wave)
    {
        waveText.text = $"{GetWaveLabel(wave)} wave";
    }

    private static string GetWaveLabel(int wave)
    {
        if (wave <= 20) return OrdinalWord(wave);
        return OrdinalNumber(wave);
    }

    private static string OrdinalWord(int number)
    {
        return number switch
        {
            1 => "First",
            2 => "Second",
            3 => "Third",
            4 => "Fourth",
            5 => "Fifth",
            6 => "Sixth",
            7 => "Seventh",
            8 => "Eighth",
            9 => "Ninth",
            10 => "Tenth",
            11 => "Eleventh",
            12 => "Twelfth",
            13 => "Thirteenth",
            14 => "Fourteenth",
            15 => "Fifteenth",
            16 => "Sixteenth",
            17 => "Seventeenth",
            18 => "Eighteenth",
            19 => "Nineteenth",
            20 => "Twentieth",
            _ => number.ToString(),
        };
    }

    private static string OrdinalNumber(int number)
    {
        if (number % 100 is 11 or 12 or 13)
            return $"{number}th";

        return (number % 10) switch
        {
            1 => $"{number}st",
            2 => $"{number}nd",
            3 => $"{number}rd",
            _ => $"{number}th",
        };
    }
}
