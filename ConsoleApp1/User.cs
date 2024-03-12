namespace ConsoleApp1;

internal class User
{
    public User()
    {
    }

    public User(string name, string password)
    {
        Name = name;
        Password = password;
    }

    public string Name { get; set; }
    public string Password { get; set; }
    public string lastConnection { get; set; }

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(Password)}: {Password}, {nameof(lastConnection)}: {lastConnection}";
    }
}