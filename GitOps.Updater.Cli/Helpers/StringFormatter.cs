namespace GitOps.Updater.Cli.Helpers
{
    public class StringFormatter
    {
        private string _input { get; set; }

        public Dictionary<string, object> Parameters { get; set; }

        public StringFormatter(string input)
        {
            _input = input;
            Parameters = new Dictionary<string, object>();
        }

        public void Set(string key, object val)
        {
            if (string.IsNullOrEmpty(key)) return; //Don't add empty keys. The replace will fail
            Parameters[key] = val;
        }

        public override string ToString()
        {
            return Parameters.Aggregate(_input, (current, parameter) => current.Replace(parameter.Key, parameter.Value.ToString()));
        }
    }
}
