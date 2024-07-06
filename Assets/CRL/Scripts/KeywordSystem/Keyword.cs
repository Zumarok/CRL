namespace Crux.CRL.KeywordSystem
{
    public struct Keyword
    {
        public readonly string name;
        public readonly string description;

        public Keyword(string name, string description)
        {
            this.name = name;
            this.description = description;
        }
    }
}
