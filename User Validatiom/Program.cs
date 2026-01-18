using Validation;

class Program
{
    static void Main(string[] args)
    {
        var user = new User
        {
            Username = "JohnDoe",
            Password = "Password123",
            Age = 25,
            Gender = "Male"
        };

        bool isValid = Validator.Validate(user);
        Console.WriteLine($"Is user valid? {isValid}");
    }
}
