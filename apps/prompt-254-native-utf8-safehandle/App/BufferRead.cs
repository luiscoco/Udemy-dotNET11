using System.Text;
public static class BufferRead
{
    public static unsafe (string Text,int Bytes,bool ShortRejected) Read(OwnedBuffer handle)
    {
        int length=Native.Length(handle);var bytes=new byte[length+1];int copied,shortResult;
        fixed(byte* destination=bytes){shortResult=Native.Copy(handle,destination,1);copied=Native.Copy(handle,destination,bytes.Length);}
        Check.That(copied==length,"native length and copied byte count agree");
        return (Encoding.UTF8.GetString(bytes,0,copied),length,shortResult<0);
    }
}
