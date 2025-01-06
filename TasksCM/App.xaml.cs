namespace TasksCM;

public partial class App : Application
{
    public App(TasksCMWindow tasksCMWindow)
    {
        InitializeComponent();
        TasksCMWindow = tasksCMWindow;
    }

    public TasksCMWindow TasksCMWindow { get; }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        TasksCMWindow.Page = new AppShell();
        return TasksCMWindow;
        //return new Window(new AppShell());
    }
}