public static class CacheKeys
{
    public static string Course(string run,string tenant,int courseId)
        =>$"{run}:v1:tenant:{Uri.EscapeDataString(tenant)}:course:{courseId}";
}
