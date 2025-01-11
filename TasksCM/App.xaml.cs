namespace TasksCM;

public partial class App : Application
{
    //Let your app know what the main window is by consuming in the ctor
    public App(TasksCMWindow tasksCMWindow)
    {
        InitializeComponent();
        TasksCMWindow = tasksCMWindow;
        
        // this is one way of catching ALL (well, most) exceptions in the app, including though who want to crash
        AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

        ParseStaticMethods.InitializeParseClient();
    }

    public TasksCMWindow TasksCMWindow { get; }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        //Here we are putting the shell into the window.

        //Look at it this way; Your app is a collection of "containers" that hold your UI.
        //The "windows" renderer takes in "TasksCMWindow (which is a container)".
        //Inside TasksCMWindow, we then pass "
        TasksCMWindow.Page = new AppShell();

        return TasksCMWindow;
        //return new Window(new AppShell());
    }


    private void CurrentDomain_FirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
    {
        //this below will print some details about the exception to the debug log
        //this way, even when app crashes I still get to know what happened JUST before.
        Debug.WriteLine($"********** UNHANDLED EXCEPTION! Details: {e.Exception} | {e.Exception.InnerException?.Message} | {e.Exception.Source} " +
            $"| {e.Exception.StackTrace} | {e.Exception.Message} || {e.Exception.Data.Values} {e.Exception.HelpLink}");

        //You can also log the exception to a file or a service
        //LogException(e.Exception);
    }
}