using CliWrap;
using Spectre.Console;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text;

namespace GitOps.Updater.Cli.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class LocalGitClient(IFileSystem fileSystem) : GitClient(fileSystem)
    {
        private string _workingDirectory;

        public override async Task SetConfigAsync(string userEmail, string userName)
        {
            var stdOutBuffer = new StringBuilder();

            void HandleLine(string line)
            {
                stdOutBuffer.AppendLine(line);
            }

            var userEmailConfig = await GetConfigAsync("user.email");
            if (string.IsNullOrEmpty(userEmailConfig))
                await SetConfigAsync("user.email", userEmail, GitConfigScope.Local, HandleLine);

            var userNameConfig = await GetConfigAsync("user.name");
            if (string.IsNullOrEmpty(userNameConfig))
                await SetConfigAsync("user.name", userName, GitConfigScope.Local, HandleLine);

            //AnsiConsole.WriteLine(stdOutBuffer.ToString());
        }

        public override async Task<bool> PushFilesAsync(string url, string branch, string[] files, string message)
        {
            var stdOutBuffer = new StringBuilder();

            void HandleLine(string line)
            {
                stdOutBuffer.AppendLine(line);
            }

            await AddFilesAsync(files, HandleLine);
            await CommitAsync(message, HandleLine);
            var pushResult = await PushAsync(url, branch, HandleLine);

            //AnsiConsole.WriteLine(stdOutBuffer.ToString());

            if (pushResult.ExitCode == 0) return true;

            return false;
        }

        private string FindGitExecutable()
        {
            // Look for system Git
            var systemPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Git", "cmd", "git.exe");
            if (File.Exists(systemPath))
            {
                return systemPath;
            }

            // Look for SourceTree's embedded Git
            var sourceTreePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Atlassian", "SourceTree", "git_local", "bin", "git.exe");
            if (File.Exists(sourceTreePath))
            {
                return sourceTreePath;
            }

            // Just hope it's on the path
            return "git";
        }

        private Task<CommandResult> SetConfigAsync(string setting, string value, GitConfigScope scope = GitConfigScope.Local, Action<string>? outputCallback = null)
        {
            return RunCommandAsync($"config --{scope.ToString().ToLowerInvariant()} {setting} {value}", outputCallback);
        }

        private async Task<string> GetConfigAsync(string setting)
        {
            var stdOutBuffer = new StringBuilder();

            void HandleLine(string line)
            {
                stdOutBuffer.AppendLine(line);
            }

            var result = await RunCommandAsync($"config --get {setting}", HandleLine);
            if (result.ExitCode == 0)
            {
                return stdOutBuffer.ToString();
            }
            else
            {
                var errorText = stdOutBuffer.ToString();
                if (!string.IsNullOrEmpty(errorText))
                {
                    AnsiConsole.WriteLine($"Unable to get config setting '{setting}");
                    AnsiConsole.WriteLine(stdOutBuffer.ToString());

                    throw new GitClientException(errorText);
                }
                return string.Empty;
            }
        }

        private Task<CommandResult> AddFilesAsync(string[] files, Action<string>? output = null)
        {
            return RunCommandAsync($"add -A", output);
        }

        private Task<CommandResult> CommitAsync(string message, Action<string>? output = null)
        {
            return RunCommandAsync($"commit -m \"{message}\"", output);
        }

        private Task<CommandResult> PushAsync(string url, string branch, Action<string>? output = null)
        {
            return RunCommandAsync($"push -o ci.skip {url} {branch}", output);
        }

        private Task<CommandResult> RunCommandAsync(string arguments, Action<string>? output)
        {
            return CliWrap.Cli.Wrap(FindGitExecutable())
                .WithArguments(arguments)
                .WithWorkingDirectory(_workingDirectory)
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(output == null ? PipeTarget.Null : PipeTarget.ToDelegate(output))
                .WithStandardErrorPipe(output == null ? PipeTarget.Null : PipeTarget.ToDelegate(output))
                .ExecuteAsync();
        }

        public override async Task<bool> CloneAsync(string url, string branch, int depth = 0)
        {
            var stdOutBuffer = new StringBuilder();

            void HandleLine(string line)
            {
                stdOutBuffer.AppendLine(line);
            }

            var cloneResult = await RunCommandAsync($"clone --depth={depth} -b {branch} {url} .", HandleLine);

            if (cloneResult.ExitCode == 0)
            {
                return true;
            }

            return false;
        }

        public override string CreateWorkingDirectory()
        {
            _workingDirectory = fileSystem.Directory.CreateTempSubdirectory().FullName;
            return _workingDirectory;
        }

        public override void DeleteWorkingDirectory()
        {
            //fileSystem.Directory.Delete(_workingDirectory, true);
        }

        public override string GetUrl(string url, string patName, string pat)
        {
            if (!string.IsNullOrEmpty(patName) && !string.IsNullOrEmpty(pat))
            {
                return url.Replace("://", $"://{patName}:{pat}@");
            }

            return url;
        }

        public enum GitConfigScope
        {
            System,
            Global,
            Local
        }
    }
}
