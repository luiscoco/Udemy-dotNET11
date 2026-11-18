using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
public static class OfflineSandbox
{
    public static async Task<int> Run(string executable,string modelPath,string work)
    {
        string profile="Course247_"+Guid.NewGuid().ToString("N");IntPtr sid=IntPtr.Zero,attributes=IntPtr.Zero,capabilities=IntPtr.Zero,environment=IntPtr.Zero;
        var granted=new List<(DirectoryInfo Directory,FileSystemAccessRule Rule)>();
        PROCESS_INFORMATION process=default;
        try
        {
            int hr=CreateAppContainerProfile(profile,profile,"Owned course offline worker",IntPtr.Zero,0,out sid);
            Marshal.ThrowExceptionForHR(hr);var identity=new SecurityIdentifier(sid);
            Marshal.ThrowExceptionForHR(GetAppContainerFolderPath(identity.Value,out IntPtr profilePath));
            string sandboxWork;
            try{sandboxWork=Path.Combine(Marshal.PtrToStringUni(profilePath)!,"AC","Temp");}
            finally{Marshal.FreeCoTaskMem(profilePath);}
            Directory.CreateDirectory(sandboxWork);
            foreach(var entry in new[]{(Path.GetDirectoryName(executable)!,FileSystemRights.ReadAndExecute),
                (modelPath,FileSystemRights.ReadAndExecute),(sandboxWork,FileSystemRights.Modify)})
            {
                var dir=new DirectoryInfo(entry.Item1);var acl=dir.GetAccessControl();
                var rule=new FileSystemAccessRule(identity,entry.Item2,InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,AccessControlType.Allow);
                acl.AddAccessRule(rule);dir.SetAccessControl(acl);granted.Add((dir,rule));
            }
            // Windows creates the temporary profile's AC storage with the appropriate integrity policy.
            var size=IntPtr.Zero;InitializeProcThreadAttributeList(IntPtr.Zero,1,0,ref size);
            attributes=Marshal.AllocHGlobal(size);
            if(!InitializeProcThreadAttributeList(attributes,1,0,ref size))throw new Win32Exception();
            var security=new SECURITY_CAPABILITIES {AppContainerSid=sid};
            capabilities=Marshal.AllocHGlobal(Marshal.SizeOf<SECURITY_CAPABILITIES>());Marshal.StructureToPtr(security,capabilities,false);
            if(!UpdateProcThreadAttribute(attributes,0,(IntPtr)0x00020009,capabilities,(IntPtr)Marshal.SizeOf<SECURITY_CAPABILITIES>(),IntPtr.Zero,IntPtr.Zero))throw new Win32Exception();
            var settings=new SortedDictionary<string,string>(StringComparer.OrdinalIgnoreCase)
            {
                ["SystemRoot"]=Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                ["WINDIR"]=Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                ["TEMP"]=sandboxWork,["TMP"]=sandboxWork,["LOCALAPPDATA"]=sandboxWork,["APPDATA"]=sandboxWork,
                ["COURSE_FOUNDRY_MODEL_PATH"]=modelPath,["COURSE_FOUNDRY_WORKER_DIR"]=sandboxWork,
                ["DOTNET_EnableDiagnostics"]="0",["COMPlus_EnableDiagnostics"]="0"
            };
            environment=Marshal.StringToHGlobalUni(string.Join('\0',settings.Select(p=>p.Key+"="+p.Value))+"\0\0");
            var startup=new STARTUPINFOEX {StartupInfo=new STARTUPINFO {cb=Marshal.SizeOf<STARTUPINFOEX>()},AttributeList=attributes};
            // Zero capabilities means the child receives no network capability. Host networking is untouched.
            if(!CreateProcess(executable,new StringBuilder("\""+executable+"\" offline-worker"),IntPtr.Zero,IntPtr.Zero,false,
                0x00080000|0x08000000|0x00000400,environment,sandboxWork,ref startup,out process))throw new Win32Exception();
            uint waited=await Task.Run(()=>WaitForSingleObject(process.Process,300000));
            if(waited!=0){TerminateProcess(process.Process,99);throw new TimeoutException("Owned offline worker exceeded five minutes");}
            if(!GetExitCodeProcess(process.Process,out uint code))throw new Win32Exception();
            foreach(string file in new[]{"worker-result.json","worker-error.txt"})
                if(File.Exists(Path.Combine(sandboxWork,file)))File.Copy(Path.Combine(sandboxWork,file),Path.Combine(work,file),true);
            return checked((int)code);
        }
        finally
        {
            if(process.Thread!=IntPtr.Zero)CloseHandle(process.Thread);
            if(process.Process!=IntPtr.Zero)CloseHandle(process.Process);
            foreach(var entry in granted.AsEnumerable().Reverse())
            {var acl=entry.Directory.GetAccessControl();acl.RemoveAccessRuleSpecific(entry.Rule);entry.Directory.SetAccessControl(acl);}
            if(environment!=IntPtr.Zero)Marshal.FreeHGlobal(environment);
            if(capabilities!=IntPtr.Zero)Marshal.FreeHGlobal(capabilities);
            if(attributes!=IntPtr.Zero){DeleteProcThreadAttributeList(attributes);Marshal.FreeHGlobal(attributes);}
            if(sid!=IntPtr.Zero){FreeSid(sid);Marshal.ThrowExceptionForHR(DeleteAppContainerProfile(profile));}
        }
    }
    [StructLayout(LayoutKind.Sequential)]private struct SECURITY_CAPABILITIES {public IntPtr AppContainerSid;public IntPtr Capabilities;public uint CapabilityCount;public uint Reserved;}
    [StructLayout(LayoutKind.Sequential)]private struct STARTUPINFO
    {
        public int cb;public IntPtr Reserved,Desktop,Title;public uint X,Y,XSize,YSize,XCount,YCount,Fill,Flags;
        public short Show,ReservedCount;public IntPtr ReservedBytes,StdInput,StdOutput,StdError;
    }
    [StructLayout(LayoutKind.Sequential)]private struct STARTUPINFOEX {public STARTUPINFO StartupInfo;public IntPtr AttributeList;}
    [StructLayout(LayoutKind.Sequential)]private struct PROCESS_INFORMATION {public IntPtr Process,Thread;public uint ProcessId,ThreadId;}
    [DllImport("userenv.dll",CharSet=CharSet.Unicode)]private static extern int CreateAppContainerProfile(string name,string display,string description,IntPtr capabilities,uint count,out IntPtr sid);
    [DllImport("userenv.dll",CharSet=CharSet.Unicode)]private static extern int DeleteAppContainerProfile(string name);
    [DllImport("userenv.dll",CharSet=CharSet.Unicode)]private static extern int GetAppContainerFolderPath(string sid,out IntPtr path);
    [DllImport("advapi32.dll")]private static extern IntPtr FreeSid(IntPtr sid);
    [DllImport("kernel32.dll",SetLastError=true)]private static extern bool InitializeProcThreadAttributeList(IntPtr list,int count,int flags,ref IntPtr size);
    [DllImport("kernel32.dll",SetLastError=true)]private static extern bool UpdateProcThreadAttribute(IntPtr list,uint flags,IntPtr attribute,IntPtr value,IntPtr size,IntPtr previous,IntPtr returned);
    [DllImport("kernel32.dll")]private static extern void DeleteProcThreadAttributeList(IntPtr list);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]private static extern bool CreateProcess(string app,StringBuilder command,IntPtr pa,IntPtr ta,bool inherit,uint flags,IntPtr env,string directory,ref STARTUPINFOEX startup,out PROCESS_INFORMATION process);
    [DllImport("kernel32.dll")]private static extern uint WaitForSingleObject(IntPtr handle,uint milliseconds);
    [DllImport("kernel32.dll")]private static extern bool GetExitCodeProcess(IntPtr process,out uint code);
    [DllImport("kernel32.dll")]private static extern bool TerminateProcess(IntPtr process,uint code);
    [DllImport("kernel32.dll")]private static extern bool CloseHandle(IntPtr handle);
}
