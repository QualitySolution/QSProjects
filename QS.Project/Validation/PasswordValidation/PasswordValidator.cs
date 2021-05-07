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
            if(password == null) {
                password = "";
            }
            errorMessages = new List<string>();

            if(Settings == null) {
                return true;
            }
            if(Settings.AllowEmpty && String.IsNullOrEmpty(password)) {
                return true;
            }

            if(password.Length > Settings.MaxLength) {
                errorMessages.Add(
                    $"Пароль слишком длинный (максимум: {Settings.MaxLength} {NumberToTextRus.Case(Settings.MaxLength, "символ", "символа", "символов")})");
            }
            if(password.Length < Settings.MinLength) {
                errorMessages.Add($"Пароль слишком короткий (минимум: {Settings.MinLength} {NumberToTextRus.Case(Settings.MinLength, "символ", "символа", "символов")})");
            }
            if(password.Contains(" ") && !Settings.AllowWhitespaces) {
                errorMessages.Add("Пароль не должен содержать пробелов");
            }
            if(password.Contains("\n") || password.Contains("\r\n")) {
                errorMessages.Add("Пароль не должен содержать символ переноса строки");
            }
            if(Settings.NotAllowedStrings != null && Settings.NotAllowedStrings.Any(password.Contains)) {
                errorMessages.Add($"Пароль не должен содержать запрещённые символы и слова ( {String.Join(" ", Settings.NotAllowedStrings)} )");
            }
            if(Settings.ASCIIOnly && !StringValidationHelper.ContainsOnlyASCIICharacters(password)) {
                errorMessages.Add("Пароль должен содержать только цифры, спец. символы и буквы англ. алфавита");
            }
            if(Settings.MinNumberCount != 0 && password.Count(Char.IsNumber) < Settings.MinNumberCount) {
                errorMessages.Add($"Пароль должен содержать минимум {Settings.MinNumberCount} {NumberToTextRus.Case(Settings.MinNumberCount, "цифру", "цифры", "цифр")}");
            }
            if(Settings.MinOtherCharactersCount != 0 && password.Count(x => !Char.IsLetterOrDigit(x)) < Settings.MinOtherCharactersCount) {
                errorMessages.Add($"Пароль должен содержать минимум {Settings.MinOtherCharactersCount} спец. {NumberToTextRus.Case(Settings.MinOtherCharactersCount, "символ", "символа", "символов")} ( . + - * : ? и др.)");
            }
            if(Settings.MinLetterSameCaseCount != 0
                && password.Count(Char.IsUpper) < Settings.MinLetterSameCaseCount
                && password.Count(Char.IsLower) < Settings.MinLetterSameCaseCount
            ) {
                errorMessages.Add(
                    $"Пароль должен содержать минимум {Settings.MinLetterSameCaseCount} {NumberToTextRus.Case(Settings.MinLetterSameCaseCount, "прописную", "прописных", "прописных")} " +
                    $"и {Settings.MinLetterSameCaseCount} {NumberToTextRus.Case(Settings.MinLetterSameCaseCount, "строчную", "строчных", "строчных")} буквы");
            }
            return !errorMessages.Any();
        }
    }
}
