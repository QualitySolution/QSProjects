using NUnit.Framework;
using QS.Utilities.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QS.Test.Utilities.Text
{
    [TestFixture(TestOf = typeof(PersonHelper))]
    public class PersonHelperTests
    {
        private static readonly object[] FullNamesSource = new object[] {
            new object[] { "Тестов Тест Тестович", "Тестов", "Тест", "Тестович" },
            new object[] { "Тестов Тест Тестович Оглы", "Тестов", "Тест", "Тестович Оглы" }
        };

        [TestCaseSource(nameof(FullNamesSource))]
        public void SplitFullNameTest(string fullName, string lastNameReference, string firstNameReference, string patronymicReference)
        {
            //Arrange, Act
            PersonHelper.SplitFullName(fullName, out string lastName, out string firstName, out string patronymic );

            //Assert
            Assert.Multiple(() =>
            {
				Assert.That(lastName, Is.EqualTo(lastNameReference));
				Assert.That(firstName, Is.EqualTo(firstNameReference));
				Assert.That(patronymic, Is.EqualTo(patronymicReference));
            });
        }
        
        [Test]
        [TestCase("Тонких П. Н.", "Тонких", "П", "Н")]
        [TestCase("Бут Д.А.", "Бут", "Д", "А")]
        [TestCase("Мичина ЕС", "Мичина", "Е", "С")]
        [TestCase("Мильгунов   А.И.", "Мильгунов", "А", "И")]
        [TestCase("Кузнецов А.  А.", "Кузнецов", "А", "А")]
        public void SplitNameWithInitialsTest(string nameWithInitials, string lastNameReference, string firstNameReference, string patronymicReference)
        {
	        //Arrange, Act
	        PersonHelper.SplitNameWithInitials(nameWithInitials, out string lastName, out string firstName, out string patronymic );

	        //Assert
	        Assert.Multiple(() =>
	        {
		        Assert.That(lastName, Is.EqualTo(lastNameReference));
		        Assert.That(firstName, Is.EqualTo(firstNameReference));
		        Assert.That(patronymic, Is.EqualTo(patronymicReference));
	        });
        }
    }
}
