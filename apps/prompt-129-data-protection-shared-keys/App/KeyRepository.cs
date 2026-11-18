using System.Security.AccessControl;
using System.Security.Principal;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
public static class KeyRepository
{
    public static bool Restrict(string directory)
    {
        Directory.CreateDirectory(directory);
        if(OperatingSystem.IsWindows())
        {
            using var identity=WindowsIdentity.GetCurrent();var sid=identity.User!;
            var info=new DirectoryInfo(directory);var security=info.GetAccessControl(AccessControlSections.Access);
            security.SetAccessRuleProtection(true,false);
            security.AddAccessRule(new FileSystemAccessRule(sid,FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit,PropagationFlags.None,AccessControlType.Allow));
            info.SetAccessControl(security);
            foreach(FileSystemAccessRule rule in info.GetAccessControl().GetAccessRules(true,true,typeof(SecurityIdentifier)))
                if(!rule.IdentityReference.Equals(sid))return false;
            return true;
        }
        File.SetUnixFileMode(directory,UnixFileMode.UserRead|UnixFileMode.UserWrite|UnixFileMode.UserExecute);
        return File.GetUnixFileMode(directory)==(UnixFileMode.UserRead|UnixFileMode.UserWrite|UnixFileMode.UserExecute);
    }
    public static ServiceProvider Build(string directory,X509Certificate2 certificate,string application)
    {
        var services=new ServiceCollection();services.AddLogging();
        services.AddDataProtection().SetApplicationName(application)
            .PersistKeysToFileSystem(new DirectoryInfo(directory)).ProtectKeysWithCertificate(certificate)
            .SetDefaultKeyLifetime(TimeSpan.FromDays(14));
        return services.BuildServiceProvider();
    }
}
