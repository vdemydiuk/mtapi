
namespace MTApiService
{
    public class MtConnectionProfile
    {
        public MtConnectionProfile(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
