namespace GitOps.Updater.Tests.Commands
{
    public class UpdateImageArgBuilder
    {
        public enum Arg
        {
            GitPatName,
            GitPat,
            GitServer,
            GitProjectPath,
            GitBranch,
            GitUserEmail,
            GitUserName
        }

        private Dictionary<Arg, string[]> _defaultArgs = new Dictionary<Arg, string[]>
        {
            [Arg.GitPatName] = ["--gitpatname", "pat"],
            [Arg.GitPat] = ["--gitpat", "1"],
            [Arg.GitServer] = ["--gitserverhost", "server"],
            [Arg.GitProjectPath] = ["--gitprojectpath", "project"],
            [Arg.GitBranch] = ["--gitbranch", "main",],
            [Arg.GitUserEmail] = ["--gituseremail", "a@a.com"],
            [Arg.GitUserName] = ["--gitusername", "a"],
        };

        private List<string> _args = [];

        public static UpdateImageArgBuilder Create()
        {
            var argBuilder = new UpdateImageArgBuilder();
            argBuilder.AddCommand("image", "update");
            return argBuilder;
        }

        private void AddCommand(params string[] command)
        {
            _args.AddRange(command);
        }

        public void AddTenantFile(string file)
        {
            _args.AddRange(["-f", file]);
        }

        public void AddVersion(string version)
        {
            _args.AddRange(["-v", version]);
        }

        public void AddImageYamlPath(string path)
        {
            _args.AddRange(["--imageyaml", path]);
        }

        public void AddTemplateYamlPath(string path)
        {
            _args.AddRange(["--templateyaml", path]);
        }

        public void AddTemplateDirectoryPattern(string pattern)
        {
            _args.AddRange(["--template", pattern]);
        }

        public void AddValuesFilePattern(string pattern)
        {
            _args.AddRange(["--values", pattern]);
        }

        public void AddDefaultValuesFilePattern(string pattern)
        {
            _args.AddRange(["--default", pattern]);
        }

        public void AddGit(params Arg[] ignore)
        {
            foreach (var arg in _defaultArgs.Keys)
            {
                if (!ignore.Contains(arg))
                {
                    _args.AddRange(_defaultArgs[arg]);
                }
            }
        }

        public void AddGitMessage(string message)
        {
            _args.AddRange(["--gitmessage", message]);
        }

        public void AddDryRun()
        {
            _args.Add("--dryrun");
        }

        public void AddCreateIfMissing()
        {
            _args.Add("-c");
        }

        public string[] Build()
        {
            return [.. _args];
        }
    }
}
