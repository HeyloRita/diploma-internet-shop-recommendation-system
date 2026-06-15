using System.Windows.Controls;

namespace InternetShop.WPF.Services;

public class NavigationService
{
    private ContentControl? _host;

    public void Initialize(ContentControl host) => _host = host;

    public void Navigate(UserControl view)
    {
        if (_host is not null)
            _host.Content = view;
    }
}
