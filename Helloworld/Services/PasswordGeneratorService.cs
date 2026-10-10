namespace HelloWorld.Services;

public record PasswordOptions(
    int Length,
    bool UseUpper = true,
    bool UseLower = true,
    bool UseDigits = true,
    bool UseSpecial = true,
    string? CustomChars = null
);

public record PasswordGenerationResult(
    string Password
);

public class PasswordGeneratorService : IPasswordGeneratorService
{
    private const string UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowerChars = "abcdefghijklmnopqrstuvwxyz";
    private const string DigitChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

    public string Generate(PasswordOptions options)
    {
        if (options.Length < 4)
            throw new ArgumentException("Password length must be at least 4 characters.", nameof(options.Length));

        var charset = BuildCharset(options);
        var passwordChars = new char[options.Length];

        var requiredChars = GetRequiredChars(options);
        for (int i = 0; i < requiredChars.Length; i++)
        {
            passwordChars[i] = requiredChars[i];
        }

        var rng = Random.Shared;
        for (int i = requiredChars.Length; i < options.Length; i++)
        {
            passwordChars[i] = charset[rng.Next(charset.Length)];
        }

        Shuffle(passwordChars, rng);

        return new string(passwordChars);
    }

    private string BuildCharset(PasswordOptions options)
    {
        var chars = new List<char>();

        if (!string.IsNullOrEmpty(options.CustomChars))
        {
            chars.AddRange(options.CustomChars);
            return new string(chars.Distinct().ToArray());
        }

        if (options.UseUpper) chars.AddRange(UpperChars);
        if (options.UseLower) chars.AddRange(LowerChars);
        if (options.UseDigits) chars.AddRange(DigitChars);
        if (options.UseSpecial) chars.AddRange(SpecialChars);

        return new string(chars.Distinct().ToArray());
    }

    private char[] GetRequiredChars(PasswordOptions options)
    {
        var charset = BuildCharset(options);
        if (string.IsNullOrEmpty(charset))
            throw new ArgumentException("At least one character set must be enabled.", nameof(options));

        var required = new List<char>();

        if (options.UseUpper)
            required.Add(UpperChars[Random.Shared.Next(UpperChars.Length)]);
        if (options.UseLower)
            required.Add(LowerChars[Random.Shared.Next(LowerChars.Length)]);
        if (options.UseDigits)
            required.Add(DigitChars[Random.Shared.Next(DigitChars.Length)]);
        if (options.UseSpecial)
            required.Add(SpecialChars[Random.Shared.Next(SpecialChars.Length)]);

        return required.ToArray();
    }

    private void Shuffle(char[] array, Random rng)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
