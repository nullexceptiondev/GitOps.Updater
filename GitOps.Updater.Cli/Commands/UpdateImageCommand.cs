﻿using GitOps.Updater.Cli.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Extensions;
using System.ComponentModel;
using System.Data;
using System.IO.Abstractions;
using YamlDotNet.RepresentationModel;
using gfs.YamlDotNet.YamlPath;
using GitOps.Updater.Cli.Extensions;
using GitOps.Updater.Cli.Infrastructure;

namespace GitOps.Updater.Cli.Commands;

public class UpdateImageCommand : AsyncCommand<UpdateImageCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-f <FILE>")]
        [Description("Path to tenants csv file")]
        public string TenantFile { get; set; }

        [CommandOption("-v|--version <VERSION>")]
        [Description("Version number")]
        public string Version { get; set; }

        [CommandOption("--template <TEMPLATEPATTERN>")]
        [Description("Directory path to versioned template directory")]
        public string TemplateDirectoryPattern { get; set; }

        [CommandOption("--values <VALUESPATTERN>")]
        [Description("File path to versioned tenants values file")]
        public string ValuesFilePattern { get; set; }

        [CommandOption("--default <DEFAULTVALUESPATTERN>")]
        [Description("Default file path to values template file")]
        public string DefaultValuesFilePattern { get; set; }

        [CommandOption("-c|--create <CREATE>")]
        [Description("Create tenant values file if missing?")]
        public bool CreateIfMissing { get; set; }

        [CommandOption("--dryrun <DRYRUN>")]
        [Description("Perform a dry run?")]
        public bool DryRun { get; set; }

        [Description("Path to image version in values file")]
        [CommandOption("--imageyaml <IMAGEYAML>")]
        public string ImageYamlPath { get; set; }

        [Description("Path to template version in values file")]
        [CommandOption("--templateyaml <TEMPLATEYAML>")]
        public string TemplateYamlPath { get; set; }

        [CommandOption("--gitpatname <GITPATNAME>")]
        [EnvironmentVariable("GIT_PAT_NAME")]
        [Description("")]
        public string GitPatName { get; set; }

        [CommandOption("--gitpat <GITPAT>")]
        [EnvironmentVariable("GIT_PAT")]
        [Description("")]
        public string GitPat { get; set; }

        [CommandOption("--gitserverhost <GITSERVERHOST>")]
        [EnvironmentVariable("CI_SERVER_HOST")]
        [Description("")]
        public string GitServerHost { get; set; }

        [CommandOption("--gitprojectpath <GITPROJECTPATH>")]
        [EnvironmentVariable("CI_PROJECT_PATH")]
        [Description("")]
        public string GitProjectPath { get; set; }

        [CommandOption("--gitbranch <GITBRANCH>")]
        [EnvironmentVariable("CI_COMMIT_BRANCH")]
        [Description("")]
        public string GitBranch { get; set; }

        [CommandOption("--gituseremail <GITUSEREMAIL>")]
        [EnvironmentVariable("GIT_USER_EMAIL")]
        [Description("")]
        public string GitUserEmail { get; set; }

        [CommandOption("--gitusername <GITUSERNAME>")]
        [EnvironmentVariable("GIT_USER_NAME")]
        [Description("")]
        public string GitUserName { get; set; }

        [CommandOption("--gitmessage <GITMESSAGE>")]
        [Description("")]
        public string GitMessage { get; set; }
    }

    private readonly IFileSystem _fileSystem;
    private readonly GitClient _gitClient;

    public UpdateImageCommand(IFileSystem fileSystem, GitClient gitClient)
    {
        _fileSystem = fileSystem;
        _gitClient = gitClient;
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (string.IsNullOrEmpty(settings.TenantFile))
            return ValidationResult.Error("Tenant file not specified");

        if (string.IsNullOrEmpty(settings.Version))
            return ValidationResult.Error("Version not specified");

        if (string.IsNullOrEmpty(settings.ImageYamlPath))
            return ValidationResult.Error("Image yaml path not specified");

        if (string.IsNullOrEmpty(settings.TemplateDirectoryPattern))
            return ValidationResult.Error("Template directory pattern not specified");

        var templateVersionInfo = VersionHelper.GetVersionInfoFromPattern(settings.TemplateDirectoryPattern, null);
        if (templateVersionInfo.VariableName.Contains("XX", StringComparison.OrdinalIgnoreCase))
            return ValidationResult.Error("Template directory pattern requires a separator between version segments");

        if (string.IsNullOrEmpty(settings.ValuesFilePattern))
            return ValidationResult.Error("Values file pattern not specified");

        var valuesVersionInfo = VersionHelper.GetVersionInfoFromPattern(settings.ValuesFilePattern, null);
        if (valuesVersionInfo.VariableName.Contains("XX", StringComparison.OrdinalIgnoreCase))
            return ValidationResult.Error("Values file pattern requires a separator between version segments");

        if (string.IsNullOrEmpty(settings.GitPatName))
            return ValidationResult.Error("Git personal access token name not specified");

        if (string.IsNullOrEmpty(settings.GitPat))
            return ValidationResult.Error("Git personal access token not specified");

        if (string.IsNullOrEmpty(settings.GitServerHost))
            return ValidationResult.Error("Git host not specified");

        if (string.IsNullOrEmpty(settings.GitProjectPath))
            return ValidationResult.Error("Git project path not specified");

        if (string.IsNullOrEmpty(settings.GitBranch))
            return ValidationResult.Error("Git branch not specified");

        if (string.IsNullOrEmpty(settings.GitUserEmail))
            return ValidationResult.Error("Git user email not specified");

        if (string.IsNullOrEmpty(settings.GitUserName))
            return ValidationResult.Error("Git user name not specified");

        return base.Validate(context, settings);
    }

    public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var gitRepoPath = _gitClient.CreateWorkingDirectory();
        try
        {           

            var logMessage = $"GitOps Updater - {settings.Version} - {DateTime.UtcNow:O}";
            var gitMessage = string.IsNullOrEmpty(settings.GitMessage) ?
                logMessage:
                settings.GitMessage;

            var cloneResult = await CloneRepo(settings);
            if (!cloneResult) throw new GitClientException("Clone failed");

            var valuesFileModifications = await UpdateEnvironmentTenantValuesFiles(gitRepoPath, settings);

            if (valuesFileModifications.Length > 0)
            {
                if (!settings.DryRun)
                {
                    if (valuesFileModifications.Any(v => v.Action.HasAnyFlag(ValuesFileAction.Created, ValuesFileAction.Moved, ValuesFileAction.Modified)))
                    {
                        //Not doing anything with the file names at the moment. We just stage any git changes (not sure if this is a good or bad idea)
                        var gitFileNames = valuesFileModifications.Select(f => f.ToFileName).ToArray();
                        var pushResult = await PushFilesToRepo(settings, gitMessage, gitFileNames);
                        if (!pushResult) throw new GitClientException("Push failed");
                    }
                }
                else
                {
                    AnsiConsole.WriteLine("Dry run results");
                }

                AnsiConsole.WriteLine($"Message - {gitMessage}");

                // log changes to console
                foreach (var valuesFileModification in valuesFileModifications)
                {
                    AnsiConsole.WriteLine($"--- {valuesFileModification.Environment} - {valuesFileModification.Tenant} ---");

                    var action = valuesFileModification.Action.ToFlagString(ignoreZero: true);
                    var fileName = "";
                    if (valuesFileModification.Action.HasFlag(ValuesFileAction.Moved))
                    {
                        fileName = !string.IsNullOrEmpty(valuesFileModification.FromFileName) ? RemoveGitRepoPath(gitRepoPath, valuesFileModification.FromFileName) : "Not Specified";
                        fileName += " -> ";
                        fileName += !string.IsNullOrEmpty(valuesFileModification.ToFileName) ? RemoveGitRepoPath(gitRepoPath, valuesFileModification.ToFileName) : "Not Specified";
                    }
                    else
                    {
                        fileName = !string.IsNullOrEmpty(valuesFileModification.ToFileName) ? RemoveGitRepoPath(gitRepoPath, valuesFileModification.ToFileName) : "Not Specified";
                    }

                    AnsiConsole.WriteLine($"{action} - {fileName} - {valuesFileModification.Message}");
                    AnsiConsole.WriteLine("------");
                }
            }
            else
            {
                AnsiConsole.WriteLine("No modifications to report");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine(ex.Message);
            return 1;
        }
        finally
        {
            _gitClient.DeleteWorkingDirectory();
        }

        return 0;
    }

    private async Task<bool> CloneRepo(Settings settings)
    {
        AnsiConsole.WriteLine("Cloning Git Repository");
        var gitUrl = $"https://{settings.GitPatName}:{settings.GitPat}@{settings.GitServerHost}/{settings.GitProjectPath}.git";
        var cloneResult = await _gitClient.CloneAsync(gitUrl, settings.GitBranch, 1);
        AnsiConsole.WriteLine("Finished Cloning Git Repository");

        return cloneResult;
    }

    private async Task<TenantValuesFileModification[]> UpdateEnvironmentTenantValuesFiles(string repoPath, Settings settings)
    {
        var modifiedFiles = new List<TenantValuesFileModification>();
        var fileLines = await _fileSystem.File.ReadAllLinesAsync(settings.TenantFile);
        foreach (var line in fileLines)
        {
            var lineSplit = line.Split(',');
            var fieldCount = lineSplit.Length;
            if (fieldCount >= 3)
            {
                var environmentName = lineSplit[0];
                var tenantName = lineSplit[1];
                var versionRule = lineSplit[2];
                var message = "";
                var action = ValuesFileAction.None;
                //var tenantValuesFile = "";
                var allowDowngrade = false;

                var fromFileName = "";
                var toFileName = "";

                // Skip the line if commented out
                if (environmentName.Trim().StartsWith('#')) continue;

                if (lineSplit.Length > 3 && !string.IsNullOrWhiteSpace(lineSplit[3]))
                    bool.TryParse(lineSplit[3], out allowDowngrade);

                if (VersionHelper.VersionRuleMatch(versionRule, settings.Version, out var versionNumber, out var versionTag))
                {
                    fromFileName = FindEnvironmentTenantValuesFile(repoPath,
                                                                    settings.TemplateDirectoryPattern,
                                                                    settings.ValuesFilePattern,
                                                                    environmentName,
                                                                    tenantName,
                                                                    versionNumber,
                                                                    settings.CreateIfMissing,
                                                                    out var requiresCreate,
                                                                    out var requiresMove,
                                                                    out var requiresMigration,
                                                                    out toFileName);

                    if (!string.IsNullOrEmpty(fromFileName))
                    {
                        if (requiresMigration && !string.IsNullOrEmpty(toFileName))
                        {
                            var expectedDirectory = _fileSystem.Path.GetDirectoryName(toFileName);
                            if (!_fileSystem.Directory.Exists(expectedDirectory))
                                _fileSystem.Directory.CreateDirectory(expectedDirectory!);

                            // move values file to correct helm folder
                            if (requiresMove)
                                _fileSystem.File.Move(fromFileName, toFileName);

                            // create missing values file
                            if (requiresCreate)
                                _fileSystem.File.Create(toFileName);

                            if (!string.IsNullOrEmpty(settings.DefaultValuesFilePattern))
                            {
                                // merge values file together with a version default to add in any new default values
                                var defaultValuesVersionInfo = VersionHelper.GetVersionInfoFromPattern(settings.DefaultValuesFilePattern, Version.Parse(versionNumber));

                                var defaultValuesFormatter = new StringFormatter(settings.DefaultValuesFilePattern);
                                defaultValuesFormatter.Set("{environment}", environmentName);
                                defaultValuesFormatter.Set("{tenant}", tenantName);
                                defaultValuesFormatter.Set(defaultValuesVersionInfo.VariableName, defaultValuesVersionInfo.Value);

                                var defaultValuesFile = _fileSystem.Path.Combine(repoPath, defaultValuesFormatter.ToString());
                                if (_fileSystem.File.Exists(defaultValuesFile))
                                {
                                    var defaultValuesFileContents = await _fileSystem.File.ReadAllTextAsync(defaultValuesFile);
                                    var tenantValuesFileContents = await _fileSystem.File.ReadAllTextAsync(toFileName);

                                    var mergedYaml = YamlHelper.MergeYaml(defaultValuesFileContents, tenantValuesFileContents);
                                    await _fileSystem.File.WriteAllTextAsync(toFileName, mergedYaml);
                                }
                            }
                        }

                        //update image tag
                        var updateResult = await UpdateValuesFile(toFileName, allowDowngrade, settings);

                        action = updateResult.Successful ? ValuesFileAction.Modified : ValuesFileAction.Errored;
                        message = updateResult.Successful ? $"Migrated from '{updateResult.PreviousImageValue}' to '{settings.Version}'" : updateResult.Message;

                        if (requiresCreate) action |= ValuesFileAction.Created;
                        if (requiresMove) action |= ValuesFileAction.Moved;
                    }
                    else
                    {
                        message = $"Unable to find any values file";
                    }
                }
                else
                {
                    message = $"Rule violation '{versionRule}'";
                    action = ValuesFileAction.RuleViolation;
                }

                modifiedFiles.Add(new TenantValuesFileModification(environmentName, tenantName, action, fromFileName, toFileName, message));
            }
        }

        return [.. modifiedFiles];
    }

    private async Task<ValuesFileModification> UpdateValuesFile(string valuesFileName, bool allowDowngrade, Settings settings)
    {
        var successful = false;
        var previousImageValue = "";
        var previousTemplateValue = "";
        var message = "";
        try
        {
            var tenantValuesFileContents = await _fileSystem.File.ReadAllTextAsync(valuesFileName);

            if (string.IsNullOrEmpty(tenantValuesFileContents)) throw new EmptyFileException("Values file is empty");

            var yaml = new YamlStream();
            yaml.Load(new StringReader(tenantValuesFileContents));

            var rootMappingNode = (YamlMappingNode)yaml.Documents[0].RootNode;
            var imageVersionNodes = rootMappingNode.Query(settings.ImageYamlPath);

            if (imageVersionNodes.Count() == 1 && imageVersionNodes.First() is YamlScalarNode imageScalarNode)
            {
                previousImageValue = imageScalarNode.Value;

                var currentVersionDetails = VersionHelper.SplitVersion(previousImageValue);
                var currentVersion = Version.Parse(currentVersionDetails.VersionNumber);

                var newVersionDetails = VersionHelper.SplitVersion(settings.Version);
                var newVersion = Version.Parse(newVersionDetails.VersionNumber);

                if (newVersion > currentVersion || allowDowngrade)
                {
                    imageScalarNode.Value = settings.Version;

                    if (!string.IsNullOrEmpty(settings.TemplateYamlPath))
                    {
                        var templateVersionNodes = rootMappingNode.Query(settings.TemplateYamlPath);

                        if (templateVersionNodes.Count() == 1 && templateVersionNodes.First() is YamlScalarNode templateScalarNode)
                        {
                            previousTemplateValue = templateScalarNode.Value;

                            // Check to see if the template yaml node has a version variable
                            var templateVersionInfo = VersionHelper.GetVersionInfoFromPattern(previousTemplateValue, newVersion);
                            if (string.IsNullOrEmpty(templateVersionInfo.VariableName))
                            {
                                // get matching template version from image version
                                templateVersionInfo = VersionHelper.GetVersionInfoFromPattern(settings.TemplateDirectoryPattern, newVersion);
                            }

                            templateScalarNode.Value = templateVersionInfo.Value;
                        }
                    }

                    using TextWriter writer = _fileSystem.File.CreateText(valuesFileName);
                    yaml.Save(writer, false);

                    successful = true;
                }
                else
                {
                    message = "Unable to downgrade";
                }
            }
            else
            {
                message = $"Unable to find yaml node from path '{settings.ImageYamlPath}'";
            }
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }

        return new ValuesFileModification(successful, previousImageValue, previousTemplateValue, message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rootDirectory"></param>
    /// <param name="templateDirectoryPattern"></param>
    /// <param name="valuesFilePattern"></param>
    /// <param name="environmentName"></param>
    /// <param name="tenantName"></param>
    /// <param name="versionNumber"></param>
    /// <param name="createIfMissing"></param>
    /// <param name="requiresMove"></param>
    /// <param name="expectedFileName">The expected file path location of the values file</param>
    /// <returns>The found file path location of the values file</returns>
    private string FindEnvironmentTenantValuesFile(string rootDirectory,
                                                   string templateDirectoryPattern,
                                                   string valuesFilePattern,
                                                   string environmentName,
                                                   string tenantName,
                                                   string versionNumber,
                                                   bool createIfMissing,
                                                   out bool requiresCreate,
                                                   out bool requiresMove,
                                                   out bool requiresMigration,
                                                   out string expectedFileName)
    {
        var versionNumberTracker = Version.Parse(versionNumber);

        requiresCreate = false;
        requiresMove = false;
        requiresMigration = false;
        expectedFileName = string.Empty;

        var valuesVersionInfo = VersionHelper.GetVersionInfoFromPattern(valuesFilePattern, versionNumberTracker);
        var valuesFileFormatter = new StringFormatter(valuesFilePattern);
        valuesFileFormatter.Set("{environment}", environmentName);
        valuesFileFormatter.Set("{tenant}", tenantName);
        valuesFileFormatter.Set(valuesVersionInfo.VariableName, valuesVersionInfo.Value);

        var tenantValuesFileName = _fileSystem.Path.Combine(rootDirectory, valuesFileFormatter.ToString());

        expectedFileName = tenantValuesFileName;

        var isNested = valuesFilePattern.Contains(templateDirectoryPattern, StringComparison.OrdinalIgnoreCase);

        if (_fileSystem.File.Exists(tenantValuesFileName))
        {
            requiresMigration = true;
            return tenantValuesFileName;
        }

        if (isNested)
        {
            // values file does not exist under the existing template folder so check others
            var templateVersionInfo = VersionHelper.GetVersionInfoFromPattern(templateDirectoryPattern, versionNumberTracker);
            var templateDirectoryFormatter = new StringFormatter(templateDirectoryPattern);
            templateDirectoryFormatter.Set("{environment}", environmentName);
            templateDirectoryFormatter.Set("{tenant}", tenantName);
            templateDirectoryFormatter.Set(templateVersionInfo.VariableName, templateVersionInfo.Value);

            var templateDirectory = _fileSystem.Path.Combine(rootDirectory, templateDirectoryFormatter.ToString());
            var parentDirectory = _fileSystem.Path.GetDirectoryName(templateDirectory);

            var directoryVersions = _fileSystem.Directory.EnumerateDirectories(parentDirectory!)
                .Select(dir => VersionHelper.GetVersionFromDirectory(templateDirectoryPattern, rootDirectory, dir)).OrderBy(v => v).ToArray();

            var currentPosition = Array.IndexOf(directoryVersions, versionNumberTracker) - 1;

            while (currentPosition >= 0)
            {
                versionNumberTracker = directoryVersions[currentPosition];
                valuesVersionInfo = VersionHelper.GetVersionInfoFromPattern(valuesFilePattern, versionNumberTracker);

                valuesFileFormatter.Set(valuesVersionInfo.VariableName, valuesVersionInfo.Value);

                tenantValuesFileName = _fileSystem.Path.Combine(rootDirectory, valuesFileFormatter.ToString());

                if (_fileSystem.File.Exists(tenantValuesFileName))
                {
                    requiresMove = true;
                    requiresMigration = true;
                    return tenantValuesFileName;
                }

                currentPosition--;
            }

            if (createIfMissing)
            {
                requiresCreate = true;
                requiresMigration = true;
                return expectedFileName;
            }

            return string.Empty;
        }
        else
        {
            if (createIfMissing)
            {
                requiresCreate = true;
                requiresMigration = true;
                return expectedFileName;
            }

            // no values file found
            return string.Empty;
        }
    }

    private async Task<bool> PushFilesToRepo(Settings settings, string message, string[] filenames)
    {
        AnsiConsole.WriteLine("Pushing files to Git Repository");
        try
        {
            var gitUrl = $"https://{settings.GitPatName}:{settings.GitPat}@{settings.GitServerHost}/{settings.GitProjectPath}.git";
            var gitBranch = $"HEAD:{settings.GitBranch}";

            await _gitClient.SetConfigAsync(settings.GitUserEmail, settings.GitUserName);
            var pushResult = await _gitClient.PushFilesAsync(gitUrl, gitBranch, filenames, message);

            if (pushResult)
            {
                AnsiConsole.WriteLine("Pushed files to Git Repository");

                return true;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine(ex.StackTrace ?? "Empty StackTrace");
            AnsiConsole.WriteException(ex);
        }

        return false;
    }

    private string RemoveGitRepoPath(string gitRepoPath, string fileName)
    {
        var tempFileName = fileName.Replace(gitRepoPath, "");
        if (tempFileName.StartsWithAny("\\", "/"))
            tempFileName = tempFileName.Remove(0, 1);

        return tempFileName;
    }

    public record TenantValuesFileModification(string Environment, string Tenant, ValuesFileAction Action, string FromFileName, string ToFileName, string Message);

    public record ValuesFileModification(bool Successful, string? PreviousImageValue, string? PreviousTemplateValue, string Message);

    [Flags]
    public enum ValuesFileAction
    {
        None = 0,
        RuleViolation = 1,
        Created = 2,
        Moved = 4,
        Modified = 8,
        Errored = 16
    }
}
