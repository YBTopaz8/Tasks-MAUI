namespace TasksCM.Pages.Desktop;

public partial class TasksCMWindow : Window
{
	public TasksCMWindow()
	{
		InitializeComponent();
		Page = new ContentPage()
		{
			Content = new VerticalStackLayout
			{
				Children = {
					new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Welcome to .NET MAUI!"
					}
				}
			}
		};
	}
}