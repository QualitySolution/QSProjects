using System;
using System.Collections.Generic;
using System.Linq;
using QS.Utilities;
using QS.Utilities.Text;

namespace QS.Validation
{
    public class PasswordValidator : IPasswordValidator
    {
        public IPasswordValidationSettings Settings { get; }

        public PasswordValidator(IPasswordValidationSettings settings)
        {
            Settings = settings;
        }

        public bool Validate(string password, out IList<string> errorMessages)
        {
            errorMessages = new List<string>();

            if(Settings == null) {
                return true;
            }
            if(password.Length > Settings.MaxLength) {
                errorMessages.Add(
                    $"Пароль не должен содержать более {Settings.MaxLength} {GetFormattedCharacterString(Settings.MaxLength)}");
            }
            if(Settings.AllowEmpty) {
                return !errorMessages.Any();
            }
            if(password.Length < Settings.MinLength) {
                errorMessages.Add($"Пароль должен быть длиннее {Settings.MinLength - 1} {GetFormattedCharacterString(Settings.MinLength)}");
            }
            if(password.Contains(" ") && !Settings.AllowWhitespaces) {
                errorMessages.Add("Пароль не должен содержать пробелы");
            }
            if(Settings.NotAllowedStrings.Any(password.Contains)) {
                errorMessages.Add(
                    $"Пароль не должен содержать запрещённые символы и слова ( {String.Join(" ", Settings.NotAllowedStrings)} )");
            }
            if(Settings.ASCIIOnly && !StringValidationHelper.ContainsOnlyASCIICharacters(password)) {
                errorMessages.Add("Пароль должен содержать только цифры,\nразрешённые спец. символы и буквы английского алфавита");
            }
            if(Settings.MinNumberCount != 0 && password.Count(Char.IsNumber) < Settings.MinNumberCount) {
                errorMessages.Add(
                    $"Пароль должен содержать минимум {Settings.MinNumberCount} {GetFormattedNumberString(Settings.MinNumberCount)}");
            }
            if(Settings.MinOtherCharactersCount != 0 && password.Count(x => !Char.IsLetterOrDigit(x)) < Settings.MinOtherCharactersCount) {
                errorMessages.Add(
                    $"Пароль должен содержать минимум {Settings.MinOtherCharactersCount} спец. {GetFormattedCharacterString(Settings.MinOtherCharactersCount)}\n" +
                    "( . ; : & ? и др.)");
            }
            if(Settings.MinLetterSameCaseCount != 0
                && password.Count(Char.IsUpper) < Settings.MinLetterSameCaseCount
                && password.Count(Char.IsLower) < Settings.MinLetterSameCaseCount
            ) {
                errorMessages.Add(
                    $"Пароль должен содержать минимум\n{Settings.MinLetterSameCaseCount} {GetFormattedSameCaseString(Settings.MinLetterSameCaseCount)}");
            }
            return !errorMessages.Any();
        }

        private string GetFormattedCharacterString(int number)
        {
            return NumberToTextRus.Case(number, "символа", "символов", "символов");
        }

        private string GetFormattedNumberString(int number)
        {
            return NumberToTextRus.Case(number, "цифру", "цифры", "цифр");
        }

        private string GetFormattedSameCaseString(int number)
        {
            return NumberToTextRus.Case(number,
                $"прописную и {number} строчную буквы",
                $"прописные и {number} строчные буквы",
                $"прописных и {number} строчных букв");
        }
    }
}
