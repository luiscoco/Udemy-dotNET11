using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
namespace NativeCourse;
public partial class MainWindow : Window
{
    private readonly CourseViewModel viewModel;
    public List<string> Checks { get; } = [];
    public MainWindow(CourseViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel=viewModel;
        DataContext=viewModel;
    }
    private void Check(bool condition,string message)
    {
        if(!condition) throw new InvalidOperationException(message);
        Checks.Add(message);
    }
    public void Verify(string mode)
    {
        Check(viewModel.HasErrors&&!SaveButton.IsEnabled,"WPF validation disables the bound command for an invalid title");
        if(mode=="problem") Check(SaveButton.IsEnabled,"incorrect expectation: invalid WPF input enables Save");
        TitleBox.Text="WPF with .NET 11";
        TitleBox.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
        Check(viewModel.Title==TitleBox.Text,"the actual TextBox binding updates the view model");
        Check(SaveButton.IsEnabled,"property validation enables the bound Save command");
        SaveButton.Command.Execute(null);
        Check(ResultText.Text=="Saved WPF with .NET 11","the result binding displays the command output");
        Check(AutomationProperties.GetName(TitleBox)=="Course title","the editable control has an explicit accessibility name");
        Check(TryFindResource("CourseAccent") is not null,"the view resolves its shared application resource");
    }
}
