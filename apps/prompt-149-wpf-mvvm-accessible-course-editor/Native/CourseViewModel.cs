using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace NativeCourse;
public sealed class CourseViewModel : ObservableValidator
{
    private string title = "";
    private string result = "Enter a valid course title.";
    public CourseViewModel()
    {
        SaveCommand = new RelayCommand(Save,()=>!HasErrors);
        ValidateAllProperties();
    }
    [Required(ErrorMessage="A title is required.")]
    [MinLength(3,ErrorMessage="Use at least three characters.")]
    public string Title
    {
        get => title;
        set
        {
            SetProperty(ref title,value,true);
            OnPropertyChanged(nameof(ErrorsText));
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
    public string ErrorsText => string.Join(" ",GetErrors(nameof(Title)).Select(error=>error.ErrorMessage));
    public string Result { get=>result;private set=>SetProperty(ref result,value); }
    public RelayCommand SaveCommand { get; }
    private void Save()
    {
        ValidateAllProperties();
        if(!HasErrors) Result = "Saved "+Title.Trim();
    }
}
