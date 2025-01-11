using Syncfusion.Maui.Toolkit.Chips;

namespace TasksCM.Pages.Desktop;

public partial class HomePageD : ContentPage
{
    //Pass as a parameter in the ctor. Allowing this class to access the ViewModel all the time when needed*
    public HomePageD(ViewModel viewModel)
	{
		InitializeComponent();
        ViewModel = viewModel;

        BindingContext = viewModel; //IMPORTANT!!!
    }

    //This is the method that will be called when the page is appearing
    // this is like the one method you will frequently override in the page for a beginning 
    // It's quick an dirty, but it works and it's a good place to start!!
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (TaskUpdatesColView is not null)
        {
            ViewModel.TaskUpdatesColView = TaskUpdatesColView;
        }
    }
    public ViewModel ViewModel { get; }


    private async void SettingsAction(object sender, EventArgs e)
    {
        var send = (SfChip)sender;
        _ = int.TryParse(send.CommandParameter.ToString(), out int selectedStatView);

        switch (selectedStatView)
        {
            case 0: 
                    await SwitchUI(0);
                await ViewModel.LoginUser(LoginUname.Text, LoginPass.Text);
                break;
            case 1:                 
                
                await SwitchUI(1);

                await ViewModel.SignUpUser(SignUpUname.Text, SignUpPass.Text, SignUpEmail.Text);

                break;
            default:
                break;
        }
    }

    //Helper method to switch between the UIs "Elegantly"
    private async Task SwitchUI(int selectedStatView)
    {
        switch (selectedStatView)
        {
            case 0:
                break;

            case 1:
                break;
            default:

                break;
        }


        var viewss = new Dictionary<int, View>
        {   
            {0, LoginParseUI},
            {1, SignUpParseUI},
        };
        if (!viewss.ContainsKey(selectedStatView))
            return;

        await Task.WhenAll
            (viewss.Select(kvp =>
            kvp.Key == selectedStatView
            ? kvp.Value.AnimateFadeInFront()
            : kvp.Value.AnimateFadeOutBack()));
    }

    private async void SignLoginUp_Clicked(object sender, EventArgs e)
    {
        var send = (SfChip)sender;
        _ = int.TryParse(send.CommandParameter.ToString(), out int selectedStatView);
        await SwitchUI(selectedStatView);
    }


    private async void SongShellChip_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.Chips.SelectionChangedEventArgs e)
    {
        var selectedTab = SettingsTab.SelectedItem;
        var send = (SfChipGroup)sender;
        SfChip? selected = send.SelectedItem as SfChip;
        if (selected is null)
        {
            return;
        }
        _ = int.TryParse(selected.CommandParameter.ToString(), out int selectedStatView);
        await SwitchUI(selectedStatView);
        return;
    }

    public async void LoginButton_Clicked(object sender, EventArgs e)
    {        
        await ViewModel.LoginUser(LoginUname.Text, LoginPass.Text);
    }
    public async void SignUpButton_Clicked(object sender, EventArgs e)
    {
        await ViewModel.SignUpUser(SignUpUname.Text, SignUpPass.Text,SignUpEmail.Text);
    }

    private async void SaveTaskBtn_Clicked(object sender, EventArgs e)
    {

        TaskObject task = new();
        task.Name= taskName.Text;
        task.Description = taskDesc.Text;
        task.AdditionalNotes = taskAddNotes.Text;
        task.Deadline = taskDeadLine.Date;


        await ViewModel.UpSertTask(task);
    }
    private async void UpdateTask_Clicked(object sender, EventArgs e)
    {

        var mod = (Button)sender;
        var s = (TaskObject)mod.BindingContext;
        await ViewModel.UpdateTask(s.TaskId);
    }
    private void Button_Clicked(object sender, EventArgs e)
    {
        var mod = (Button)sender;
        ViewModel.SelectedTaskObject = (TaskObject)mod.BindingContext;

        ViewModel.DeleteTask();
    }

    private async void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        await ViewModel.UpdateTaskState();
    }
}

//* Watch out for your context and ensure they're synchronized. !