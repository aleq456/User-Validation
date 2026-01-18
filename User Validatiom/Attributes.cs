namespace Validation;

public class Validator
{
    public static bool Validate(User User)
    {
        var type = User.GetType();
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            var attributes = property.GetCustomAttributes(typeof(ValidationAttribute), true);
            foreach (ValidationAttribute attribute in attributes)
            {
                var value = property.GetValue(User);
                if (!ValidateAttribute(attribute, value, User))
                {
                    Console.WriteLine($"Validation failed for {property.Name}: {attribute.ErrorMessage()}");
                    return false;
                }
            }
        }
        return true;
    }

    private static bool ValidateAttribute(ValidationAttribute attribute, object value, User user)
    {
        if (attribute is MinLengthAttribute minLen)
            return value is string str && str.Length >= minLen.Length;
        if (attribute is ContainsAttribute contains)
            return value is string str && str.Contains(contains.Text);
        if (attribute is PositiveAttribute)
            return value is int num && num > 0;
        if (attribute is NegativeAttribute neg)
            return value is int num && num < neg.Value;
        if (attribute is RequiredIfAttribute reqIf)
            return IsRequiredIfValid(reqIf, value, user);
        if (attribute is UrlAttribute)
            return value is string str && Uri.TryCreate(str, UriKind.Absolute, out var uri) && (uri.Scheme == "http" || uri.Scheme == "https");
        if (attribute is FutureDateAttribute)
            return value is DateTime dt && dt > DateTime.UtcNow;
        if (attribute is AllowedValuesAttribute allowed)
            return value is string str && allowed.AllowedValues.Contains(str, StringComparer.OrdinalIgnoreCase);
        if (attribute is AllowedEnumAttribute enumAttr)
            return value != null && (value.GetType() == enumAttr.EnumType ? Enum.IsDefined(enumAttr.EnumType, value) : (value is string s && Enum.TryParse(enumAttr.EnumType, s, out _)));
        return true;
    }

    private static bool IsRequiredIfValid(RequiredIfAttribute attr, object value, User user)
    {
        var condProp = user.GetType().GetProperty(attr.ConditionalProperty);
        if (condProp == null) return true;
        var condValue = condProp.GetValue(user);
        if (Equals(condValue, attr.ConditionalValue))
        {
            return value != null && !string.IsNullOrEmpty(value.ToString());
        }
        return true;
    } 
}

public class ValidationAttribute : Attribute
{
    public virtual string ErrorMessage() => "Validation failed.";
}

public class MinLengthAttribute : ValidationAttribute
{
    public int Length {get;}

    public MinLengthAttribute(int Length)
    {
        this.Length = Length;
    }

    public override string ErrorMessage()
    {
        return "Length should be at least 4 characters.";
    }
    
}

public class ContainsAttribute : ValidationAttribute
{
    public string Text {get;}

    public ContainsAttribute(string Text)
    {
        this.Text = Text;
    }

    public override string ErrorMessage()
    {
        return "Value should contain upper and lower case characters.";
    }
}

public class PositiveAttribute : ValidationAttribute
{

    public override string ErrorMessage()
    {
        return "Value should be positive.";
    }
}

public class NegativeAttribute : ValidationAttribute
{
    public int Value {get;}

    public NegativeAttribute(int Value)
    {
        this.Value = Value;
    }

    public override string ErrorMessage()
    {
        return "Value should be negative.";
    }
}

public class AllowedValuesAttribute : ValidationAttribute
{
    public string[] AllowedValues { get; }

    public AllowedValuesAttribute(params string[] allowedValues)
    {
        AllowedValues = allowedValues;
    }

    public override string ErrorMessage() => $"Value must be one of: {string.Join(", ", AllowedValues)}.";
}

public class AllowedEnumAttribute : ValidationAttribute
{
    public Type EnumType { get; }

    public AllowedEnumAttribute(Type enumType)
    {
        if (!enumType.IsEnum) throw new ArgumentException("Type must be an enum.");
        EnumType = enumType;
    }

    public override string ErrorMessage() => $"Value must be a valid {EnumType.Name} enum member.";
}

public class RequiredIfAttribute : ValidationAttribute
{
    public string ConditionalProperty { get; }
    public object ConditionalValue { get; }

    public RequiredIfAttribute(string conditionalProperty, object conditionalValue)
    {
        ConditionalProperty = conditionalProperty;
        ConditionalValue = conditionalValue;
    }

    public override string ErrorMessage() => $"This field is required when {ConditionalProperty} is {ConditionalValue}.";
}

public class UrlAttribute : ValidationAttribute
{
    public override string ErrorMessage() => "The value must be a valid http or https URL.";
}

public class FutureDateAttribute : ValidationAttribute
{
    public override string ErrorMessage() => "The date must be in the future.";
}
