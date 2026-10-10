namespace HelloWorld.Services;

public interface IPasswordGeneratorService
{
    string Generate(PasswordOptions options);
}
