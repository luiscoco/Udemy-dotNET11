using System.Device.Gpio;
using Iot.Device.Button;
bool reversed=args.Contains("problem");
string probe;
try {using var physical=new GpioController();probe="Detected controller with "+physical.PinCount+" pins; not selected or written.";}
catch(Exception error) when(error is PlatformNotSupportedException or IOException or NotSupportedException or TypeInitializationException){probe=error.GetType().Name+": "+error.Message;}
Console.WriteLine("IOT physical probe: "+probe);
Console.WriteLine("IOT selected backend: explicit SOFTWARE driver; no physical wiring is demonstrated.");
using var driver=new SoftwareGpio();
using var controller=new GpioController(driver);
using var button=new GpioButton(17,isPullUp:!reversed,hasExternalResistor:true,gpio:controller,shouldDispose:false,debounceTime:TimeSpan.Zero);
driver.SetInput(17,PinValue.Low);
bool pressed=button.IsPressed;
Check.That(pressed,"EXT active-low button polarity was reversed");
driver.SetInput(17,PinValue.High);
Check.That(!button.IsPressed,"released button reads high");
Harness.Save("ext-evidence.json",new{passed=true,actualGpioLibrary=true,actualBinding=true,backend="software",physicalHardware=false,probe,pressed,released=!button.IsPressed});
Console.WriteLine("PASS GPIO controller, real GpioButton binding and active-low transitions");
