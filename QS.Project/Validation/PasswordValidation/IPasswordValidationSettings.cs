namespace QS.Validation
{
    public interface IPasswordValidationSettings
    {
        /// <summary>
        ///  A password must contain at least this many characters
        /// </summary>
        int MinLength { get; }

        /// <summary>
        ///  A password must contain equal or less then this many characters
        /// </summary>
        int MaxLength { get; }

        /// <summary>
        ///  A password must contain at least this many digits
        /// </summary>
        int MinNumberCount { get; }

        /// <summary>
        ///  A password must contain at least this many characters that are neither digits nor letters
        /// </summary>
        int MinOtherCharactersCount { get; }

        /// <summary>
        ///  A password must contain at least this many upper-case and this many lower-case letters
        /// </summary>
        int MinLetterSameCaseCount { get; }

        /// <summary>
        ///  A password must contain characters of ASCII encoding only
        /// </summary>
        bool ASCIIOnly { get; set; }

        /// <summary>
        ///  A password can contain whitespace characters
        /// </summary>
        bool AllowWhitespaces { get; set; }

        /// <summary>
        ///  A password must not contain any substring from this array
        /// </summary>
        string[] NotAllowedStrings { get; set; }

        /// <summary>
        ///  A password can be empty, if true other settings ignored
        /// </summary>
        bool AllowEmpty { get; set; }
    }
}
