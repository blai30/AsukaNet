namespace Asuka.Configuration
{
    public class FileLoggerOptions
    {
        public string OutputDirectory { get; set; }
        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }
        public int MaxFileSizeKb { get; set; }
    }
}
