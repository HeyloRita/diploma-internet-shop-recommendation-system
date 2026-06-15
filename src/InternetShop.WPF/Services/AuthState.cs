using InternetShop.WPF.Models;

namespace InternetShop.WPF.Services;

public class AuthState
{
    public string? Token    { get; private set; }
    public string? Name     { get; private set; }
    public string? Email    { get; private set; }
    public string? Role     { get; private set; }
    public bool    IsLoggedIn => Token is not null;
    public bool    IsAdmin    => Role == "Admin";

    public void Login(AuthResponse resp)
    {
        Token = resp.Token;
        Name  = resp.Name;
        Email = resp.Email;
        Role  = resp.Role;
    }

    public void Logout()
    {
        Token = null;
        Name  = null;
        Email = null;
        Role  = null;
    }
}
