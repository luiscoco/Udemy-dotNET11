using System.ComponentModel;
using System.Text.Json;
namespace NativeCourse;
public sealed record CourseRow(string Title, string Status);
public sealed class CourseForm : Form
{
    private readonly BindingList<CourseRow> rows = [new(".NET foundations","Ready"),new("Native controls","Ready")];
    private readonly BindingSource source = new();
    private readonly Label status = new() { AutoSize=true,Text="Ready",Font=new Font("Segoe UI",16) };
    private readonly Button start = new() { Text="Load courses",AutoSize=true,AccessibleName="Load courses" };
    private readonly Button cancel = new() { Text="Cancel work",AutoSize=true,AccessibleName="Cancel work" };
    private readonly Button ping = new() { Text="Check UI response",AutoSize=true,AccessibleName="Check UI response" };
    private readonly DataGridView grid = new() { Dock=DockStyle.Fill,AutoGenerateColumns=true,ReadOnly=true,AllowUserToAddRows=false,AccessibleName="Course list",AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill };
    private CancellationTokenSource? operation;
    private int pings;
    private int workerThreadId;
    private readonly List<string> checks = [];
    public CourseForm()
    {
        Text="Lesson 150 · Windows Forms course browser";
        Size=new Size(1100,760);
        AutoScaleMode=AutoScaleMode.Dpi;
        Font=new Font("Segoe UI",12);
        BackColor=Color.FromArgb(244,242,251);
        var layout=new TableLayoutPanel { Dock=DockStyle.Fill,Padding=new Padding(28),ColumnCount=1,RowCount=4 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,70));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,65));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,70));
        layout.Controls.Add(new Label { Text=".NET 11 · Responsive Windows Forms",AutoSize=true,Font=new Font("Segoe UI",22,FontStyle.Bold) },0,0);
        var buttons=new FlowLayoutPanel { Dock=DockStyle.Fill };
        buttons.Controls.AddRange([start,cancel,ping]);
        layout.Controls.Add(buttons,0,1);
        layout.Controls.Add(grid,0,2);
        layout.Controls.Add(status,0,3);
        Controls.Add(layout);
        source.DataSource=rows;
        grid.DataSource=source;
        start.Click+=async(_,_)=>await LoadAsync();
        cancel.Click+=(_,_)=>operation?.Cancel();
        ping.Click+=(_,_)=>{pings++;status.Text="UI responded while work was pending.";};
        Shown+=async(_,_)=>await RunVerificationAsync();
    }
    private async Task LoadAsync()
    {
        if(operation is not null) return;
        using var cancellation=new CancellationTokenSource();
        operation=cancellation;
        start.Enabled=false;
        var progress=new Progress<int>(value=>status.Text=$"Loading {value}%");
        try
        {
            var loaded=await Task.Run(async()=>
            {
                workerThreadId=Environment.CurrentManagedThreadId;
                var generated=new List<CourseRow>();
                var titles=new[]{"Binding","Async work","High DPI"};
                for(int step=0;step<titles.Length;step++)
                {
                    var digest=System.Text.Encoding.UTF8.GetBytes(titles[step]);
                    for(int round=0;round<2000;round++)
                    {
                        cancellation.Token.ThrowIfCancellationRequested();
                        digest=System.Security.Cryptography.SHA256.HashData(digest);
                    }
                    // The fixture delay makes the cancellation interaction repeatable.
                    await Task.Delay(100,cancellation.Token);
                    generated.Add(new CourseRow(titles[step],"Processed "+Convert.ToHexString(digest)[..8]));
                    ((IProgress<int>)progress).Report((step+1)*100/titles.Length);
                }
                cancellation.Token.ThrowIfCancellationRequested();
                return generated.ToArray();
            },cancellation.Token);
            foreach(var row in loaded) rows.Add(row);
            status.Text=$"Loaded {loaded.Length} courses. Total {rows.Count}.";
        }
        catch(OperationCanceledException) { status.Text="Cancelled. Existing rows retained."; }
        finally { operation=null;start.Enabled=true; }
    }
    private void Check(bool condition,string message)
    {
        if(!condition) throw new InvalidOperationException(message);
        checks.Add(message);
    }
    private async Task RunVerificationAsync()
    {
        var mode=Environment.GetEnvironmentVariable("COURSE_MODE");
        if(string.IsNullOrEmpty(mode)) return;
        bool passed=false;
        string? error=null;
        try
        {
            var pending=LoadAsync();
            await Task.Delay(45);
            ping.PerformClick();
            Check(pings==1,"the native UI processes another button while background work is pending");
            Check(workerThreadId!=Environment.CurrentManagedThreadId,"the CPU work executes on a worker thread while controls remain on the UI thread");
            cancel.PerformClick();
            await pending;
            Check(rows.Count==2,"cancelled work retains the existing bound rows");
            if(mode=="problem") Check(rows.Count>2,"incorrect expectation: cancelled background work appends rows");
            await LoadAsync();
            Check(rows.Count==5&&source.Count==5,"completed background work updates the bound list on the UI thread");
            Check(grid.Rows.Count==5,"the actual DataGridView displays all five bound records");
            Check(DeviceDpi>=96&&AutoScaleMode==AutoScaleMode.Dpi,"the native form reports DPI-aware scaling on this display");
            passed=true;
        }
        catch(Exception exception) { error=exception.Message; }
        var directory=Environment.GetEnvironmentVariable("COURSE_ARTIFACTS")!;
        using(var image=new Bitmap(Width,Height))
        {
            DrawToBitmap(image,new Rectangle(0,0,Width,Height));
            image.Save(Path.Combine(directory,passed?"native-success.png":"native-problem.png"),System.Drawing.Imaging.ImageFormat.Png);
        }
        var report=new {passed,mode,scenario="native-winforms-150",evidenceKind="Actual Windows Forms controls and DrawToBitmap capture",checks,error};
        var json=JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true});
        await File.WriteAllTextAsync(Path.Combine(directory,"native-"+mode+".json"),json);
        if(passed) await File.WriteAllTextAsync(Path.Combine(directory,"browser-ui-verification.json"),json);
        Environment.Exit(passed?0:1);
    }
    protected override void Dispose(bool disposing)
    {
        if(disposing){operation?.Cancel();source.Dispose();}
        base.Dispose(disposing);
    }
}
