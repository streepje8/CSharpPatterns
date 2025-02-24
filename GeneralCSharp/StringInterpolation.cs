namespace CodingPatterns.GeneralCSharp;

public class StringInterpolation
{
    public static string Text { get; set; } = $"Today, it is {DateTime.Now:d}";

    public void Update()
    {
        Text = $"{GetType().FullName} has a field {nameof(Text)} which contains {Text}";
    }
}