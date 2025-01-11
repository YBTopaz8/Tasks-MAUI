namespace TasksCM;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(HomePageD), typeof(HomePageD));
        
    }
}
