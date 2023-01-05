namespace Libria.Data
{
    public static class ModelValidationMessages
    {
        public const string Required = "Це поле не може бути пустим";
        public const string Email = "Введіть правильну email адресу";
        public const string PhoneNumber = "Введіть правильний номер телефону";
        public const string Password = "Пароль повинен бути довжиною від 6 символів і мати цифру";
        public const string PasswordIncorrect = "Введено невірний пароль";
        public const string PasswordMismatch = "Паролі не співпадають";
        public const string Isbn = "Довжина ISBN повинна бути від 10 до 13 символів";

    }
}
