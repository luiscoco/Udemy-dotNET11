using System.Device.Gpio;
public sealed class SoftwareGpio:GpioDriver
{
    private readonly Dictionary<int,PinMode> modes=new();
    private readonly Dictionary<int,PinValue> values=new();
    private readonly Dictionary<int,PinChangeEventHandler> callbacks=new();
    protected override int PinCount=>32;
    protected override void OpenPin(int pin){modes[pin]=PinMode.Input;values[pin]=PinValue.High;}
    protected override void ClosePin(int pin){modes.Remove(pin);values.Remove(pin);callbacks.Remove(pin);}
    protected override void SetPinMode(int pin,PinMode mode)=>modes[pin]=mode;
    protected override PinMode GetPinMode(int pin)=>modes[pin];
    protected override bool IsPinModeSupported(int pin,PinMode mode)=>true;
    protected override PinValue Read(int pin)=>values[pin];
    protected override void Write(int pin,PinValue value)=>values[pin]=value;
    protected override WaitForEventResult WaitForEvent(int pin,PinEventTypes types,CancellationToken cancellation)=>throw new NotSupportedException("Use deterministic injected transitions.");
    protected override void AddCallbackForPinValueChangedEvent(int pin,PinEventTypes types,PinChangeEventHandler callback)=>callbacks[pin]=callback;
    protected override void RemoveCallbackForPinValueChangedEvent(int pin,PinChangeEventHandler callback)=>callbacks.Remove(pin);
    public void SetInput(int pin,PinValue value)
    {
        PinValue previous=values[pin];values[pin]=value;
        if(previous!=value&&callbacks.TryGetValue(pin,out var callback))
            callback(this,new PinValueChangedEventArgs(value==PinValue.Low?PinEventTypes.Falling:PinEventTypes.Rising,pin));
    }
}
