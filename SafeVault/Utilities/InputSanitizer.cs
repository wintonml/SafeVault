namespace SafeVault.Utilities;

public static class InputSanitizer
{
    public static string? NormaliseIdentifier(string? value)
    {
        if (value is null)
            return null;

        return new string(value.Where(character => !char.IsControl(character)).ToArray()).Trim();
    }
}