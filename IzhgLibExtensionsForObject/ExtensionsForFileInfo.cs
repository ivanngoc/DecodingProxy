namespace System.IO
{
    public static class ExtensionsForFileInfo
    {
        public static string NameWithoutExtension(this FileInfo file)
        {
            return file.Name.Remove(file.Name.Length - file.Extension.Length);
        }
    }
}