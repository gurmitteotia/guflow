namespace NetPlayground
{
    public class ActivityName
    {
        public ActivityName(string name, string version, string postionalName="")
        {
            Name = name;
            Version = version;
            PostionalName = postionalName;
        }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public string PostionalName { get; private set; }
    }
}