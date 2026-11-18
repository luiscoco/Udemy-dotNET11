using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
public sealed class OwnedBuffer:SafeHandleZeroOrMinusOneIsInvalid
{
    public OwnedBuffer():base(true){}
    protected override bool ReleaseHandle(){Native.Release(handle);return true;}
}
public static partial class Native
{
    [LibraryImport("course_native",EntryPoint="course_create",StringMarshalling=StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs=[typeof(CallConvCdecl)])]
    public static partial OwnedBuffer Create(string value);
    [LibraryImport("course_native",EntryPoint="course_length")]
    public static partial int Length(OwnedBuffer buffer);
    [LibraryImport("course_native",EntryPoint="course_copy")]
    public static unsafe partial int Copy(OwnedBuffer buffer,byte* destination,int capacity);
    [LibraryImport("course_native",EntryPoint="course_release")]
    public static partial void Release(IntPtr buffer);
    [LibraryImport("course_native",EntryPoint="course_live")]
    public static partial int Live();
}
