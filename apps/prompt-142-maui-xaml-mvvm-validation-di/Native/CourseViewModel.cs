using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
namespace NativeCourse;
public sealed class CourseViewModel : ObservableValidator
{
    private readonly CourseRepository repository;
    private string title = "";
    private string status = "Enter a course title.";
    public CourseViewModel(CourseRepository repository)
    {
        this.repository = repository;
        SaveCommand = new RelayCommand(Save, () => !HasErrors);
        ValidateAllProperties();
    }
    [Required(ErrorMessage = "A course title is required.")]
    [MinLength(3, ErrorMessage = "Use at least three characters.")]
    public string Title
    {
        get => title;
        set
        {
            SetProperty(ref title, value, true);
            OnPropertyChanged(nameof(ErrorsText));
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
    public string ErrorsText => string.Join(" ", GetErrors(nameof(Title)).Select(error => error.ErrorMessage));
    public string Status { get => status; private set => SetProperty(ref status, value); }
    public RelayCommand SaveCommand { get; }
    private void Save()
    {
        ValidateAllProperties();
        if (HasErrors) return;
        repository.Save(Title.Trim());
        Status = $"Saved {repository.LastTitle}";
    }
}
