using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "FancyDigits", menuName = "Scriptable Objects/Fancy Digits")]
public class FancyDigits : ScriptableObject
{
    [Tooltip("Index = digit value (0 - 9)")]
    [SerializeField] private Sprite[] digitsRaw = new Sprite[10];

    public Sprite GetDigit(int digit)
    {
        Assert.IsTrue(digit >= 0 && digit <= 9);
        Assert.IsNotNull(digitsRaw[digit]);
        return digitsRaw[digit];
    }

    public void GetDigits(
        int number,
        out Sprite tens,
        out Sprite ones
    )
    {
        number = Mathf.Clamp(number, 0, 99);

        int t = number / 10;
        int o = number % 10;

        tens = t > 0 ? GetDigit(t) : null;
        ones = GetDigit(o);
    }
}
