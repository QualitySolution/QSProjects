namespace QS.Validation
{
    public class DefaultPasswordValidationSettings : IPasswordValidationSettings
    {
        public int MinLength { get; set; } = 3;
        public int MaxLength { get; set; } = 30;
        public int MinNumberCount { get; set; } = 0;
        public int MinOtherCharactersCount { get; set; } = 0;
        public int MinLetterSameCaseCount { get; set; } = 0;
        public bool ASCIIOnly { get; set; } = true;
        public bool AllowWhitespaces { get; set; } = false;
        public string[] NotAllowedStrings { get; set; } = { "'" };
        public bool AllowEmpty { get; set; } = false;
    }
}
