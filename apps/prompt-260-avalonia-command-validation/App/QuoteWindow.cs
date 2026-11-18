using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
public sealed class QuoteWindow:Window
{
    public TextBox Quantity{get;}=new(){Text="1",Width=260,HorizontalAlignment=Avalonia.Layout.HorizontalAlignment.Left};
    public Button Calculate{get;}=new(){Content="Calculate quote",Padding=new Thickness(24,12)};
    public decimal Total{get;private set;}
    public QuoteWindow(bool broken)
    {
        Width=1000;Height=650;Title=".NET 11 • Avalonia command lab";Background=Brushes.White;
        var result=new TextBlock{Text="Enter quantity and calculate",FontSize=28};
        Calculate.Click+=(_,_)=>{
            if(!int.TryParse(Quantity.Text,out int count)||count<1||count>10){result.Text="Quantity must be 1 to 10";return;}
            Total=(broken?1:count)*12.50m;result.Text="Total: "+Total.ToString("0.00",System.Globalization.CultureInfo.InvariantCulture)+" EUR";
        };
        Content=new StackPanel{Margin=new Thickness(60),Spacing=28,Children={
            new TextBlock{Text="AVALONIA + .NET 11",FontSize=36,FontWeight=FontWeight.Bold},
            new TextBlock{Text="Actual controls • keyboard input • mouse command",FontSize=22},
            new TextBlock{Text="Unit price: 12.50 EUR | Quantity: 1–10",FontSize=24},Quantity,Calculate,result}};
    }
}
