using HelloWorld.Services;

namespace HelloWorld.Test;

/// <summary>
/// Тестирование <see cref="IPasswordGeneratorService"/>
/// </summary>
public class PasswordGeneratorServiceTest
{
    private readonly PasswordGeneratorService _service;

    public PasswordGeneratorServiceTest()
    {
        _service = new PasswordGeneratorService();
    }

    /// <summary>
    /// Проверяет, что пароль с параметрами по умолчанию содержит символы всех четырёх наборов
    /// </summary>
    [Fact]
    public void Generate_WithDefaultOptions_ReturnsPasswordWithAllCharSets()
    {
        // Arrange
        var options = new PasswordOptions(16);

        // Act
        var result = _service.Generate(options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(16, result.Length);
        Assert.True(result.Any(c => char.IsUpper(c)), "Пароль должен содержать хотя бы одну заглавную букву");
        Assert.True(result.Any(c => char.IsLower(c)), "Пароль должен содержать хотя бы одну строчную букву");
        Assert.True(result.Any(c => char.IsDigit(c)), "Пароль должен содержать хотя бы одну цифру");
        Assert.True(result.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c)), "Пароль должен содержать хотя бы один специальный символ");
    }

    /// <summary>
    /// Проверяет, что при длине меньше 4 выбрасывается ArgumentException
    /// </summary>
    [Fact]
    public void Generate_WithLengthLessThan4_ThrowsArgumentException()
    {
        // Arrange
        var options = new PasswordOptions(3);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.Generate(options));
    }

    /// <summary>
    /// Проверяет, что при включённых только цифрах пароль содержит исключительно цифры
    /// </summary>
    [Fact]
    public void Generate_WithOnlyDigits_ReturnsDigitsOnly()
    {
        // Arrange
        var options = new PasswordOptions(20, UseUpper: false, UseLower: false, UseDigits: true, UseSpecial: false);

        // Act
        var result = _service.Generate(options);

        // Assert
        Assert.Equal(20, result.Length);
        Assert.True(result.All(char.IsDigit), "Все символы должны быть цифрами");
    }

    /// <summary>
    /// Проверяет, что при включённых только заглавных буквах пароль содержит исключительно заглавные
    /// </summary>
    [Fact]
    public void Generate_WithOnlyUpper_ReturnsUpperOnly()
    {
        // Arrange
        var options = new PasswordOptions(10, UseUpper: true, UseLower: false, UseDigits: false, UseSpecial: false);

        // Act
        var result = _service.Generate(options);

        // Assert
        Assert.Equal(10, result.Length);
        Assert.True(result.All(char.IsUpper), "Все символы должны быть заглавными");
    }

    /// <summary>
    /// Проверяет, что при включённых только строчных буквах пароль содержит исключительно строчные
    /// </summary>
    [Fact]
    public void Generate_WithOnlyLower_ReturnsLowerOnly()
    {
        // Arrange
        var options = new PasswordOptions(10, UseUpper: false, UseLower: true, UseDigits: false, UseSpecial: false);

        // Act
        var result = _service.Generate(options);

        // Assert
        Assert.Equal(10, result.Length);
        Assert.True(result.All(char.IsLower), "Все символы должны быть строчными");
    }

    /// <summary>
    /// Проверяет, что при включённых только специальных символах пароль содержит исключительно специальные символы
    /// </summary>
    [Fact]
    public void Generate_WithOnlySpecial_ReturnsSpecialOnly()
    {
        // Arrange
        var options = new PasswordOptions(15, UseUpper: false, UseLower: false, UseDigits: false, UseSpecial: true);

        // Act
        var result = _service.Generate(options);

        // Assert
        Assert.Equal(15, result.Length);
        Assert.True(result.All(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c)), "Все символы должны быть специальными");
    }

    /// <summary>
    /// Проверяет, что при задании пользовательского набора символов пароль содержит исключительно символы из него
    /// </summary>
    [Fact]
    public void Generate_WithCustomChars_UsesOnlyCustomChars()
    {
        // Arrange
        var options = new PasswordOptions(30, UseUpper: false, UseLower: false, UseDigits: false, UseSpecial: false, CustomChars: "abc123");

        // Act
        var result = _service.Generate(options);

        // Assert
        Assert.Equal(30, result.Length);
        Assert.True(result.All(c => "abc123".Contains(c)), "Все символы должны быть из пользовательского набора");
    }

    /// <summary>
    /// Проверяет, что пароль минимально допустимой длины (4 символа) содержит символы всех четырёх наборов
    /// </summary>
    [Fact]
    public void Generate_WithMinimumLength_ReturnsValidPassword()
    {
        // Arrange
        var options = new PasswordOptions(4);

        // Act
        var result = _service.Generate(options);

        // Assert
        Assert.Equal(4, result.Length);
        Assert.Contains(result, c => char.IsUpper(c));
        Assert.Contains(result, c => char.IsLower(c));
        Assert.Contains(result, c => char.IsDigit(c));
        Assert.Contains(result, c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c));
    }

    /// <summary>
    /// Проверяет, что сгенерированный пароль имеет точную длину, указанную в параметрах
    /// </summary>
    [Fact]
    public void Generate_ReturnsPasswordOfCorrectLength()
    {
        // Arrange
        var options = new PasswordOptions(32);

        // Act
        var result = _service.Generate(options);

        // Assert
        Assert.Equal(32, result.Length);
    }
}