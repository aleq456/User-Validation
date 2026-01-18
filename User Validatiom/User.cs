using Validation;

public class User
{
    [MinLength(4)]
    public string Username { get; set; }

    [Contains("Password")]
    public string Password { get; set; }

    [Positive]
    public int Age { get; set; }

    [AllowedValues("Male", "Female", "Other")]
    public string Gender { get; set; }
}
