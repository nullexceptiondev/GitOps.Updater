using GitOps.Updater.Cli.Commands;
using GitOps.Updater.Cli.Helpers;
using GitOps.Updater.Cli.Infrastructure;
using FluentAssertions;
using GitOps.Updater.Tests.Infrastructure;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using static GitOps.Updater.Tests.Commands.UpdateImageArgBuilder;

namespace GitOps.Updater.Tests.Commands
{
    public class UpdateImageCommandTests : BaseCommandTests
    {
        private readonly MockFileSystem _fileSystem;
        private readonly FakeGitClient _gitClient;

        public UpdateImageCommandTests()
        {
            _gitClient = (FakeGitClient)Factory.ResolveService<GitClient>();
            _fileSystem = (MockFileSystem)_gitClient.FileSystem;
        }

        [Fact]
        public void Execute_MissingTenantFileParam()
        {
            var argBuilder = Create();
            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(-1);
            result.Output.Should().Contain("Tenant file not specified");
        }

        [Fact]
        public void Execute_MissingVersionParam()
        {
            var argBuilder = Create();
            argBuilder.AddTenantFile("t");
            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(-1);
            result.Output.Should().Contain("Error: Version not specified");
        }

        [Theory]
        [InlineData(Arg.GitPatName, "Git personal access token name not specified")]
        [InlineData(Arg.GitPat, "Git personal access token not specified")]
        [InlineData(Arg.GitServer, "Git host not specified")]
        [InlineData(Arg.GitProjectPath, "Git project path not specified")]
        [InlineData(Arg.GitBranch, "Git branch not specified")]
        [InlineData(Arg.GitUserEmail, "Git user email not specified")]
        [InlineData(Arg.GitUserName, "Git user name not specified")]
        public void Execute_MissingGitParam(Arg arg, string errorMessage)
        {
            var argBuilder = Create();
            argBuilder.AddTenantFile("t");
            argBuilder.AddVersion("1");
            argBuilder.AddImageYamlPath("i");
            argBuilder.AddTemplateDirectoryPattern("t");
            argBuilder.AddValuesFilePattern("v");
            argBuilder.AddGit(arg);
            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(-1);
            result.Output.Should().Contain($"Error: {errorMessage}");
        }

        [Fact]
        public void Execute_MissingTemplateDirectoryParam()
        {
            var argBuilder = Create();
            argBuilder.AddTenantFile("t");
            argBuilder.AddVersion("1");
            argBuilder.AddImageYamlPath("i");
            argBuilder.AddValuesFilePattern("v");
            argBuilder.AddGit();
            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(-1);
            result.Output.Should().Contain($"Error: Template directory pattern not specified");
        }

        [Fact]
        public void Execute_MissingValuesDirectoryParam()
        {
            var argBuilder = Create();
            argBuilder.AddTenantFile("t");
            argBuilder.AddVersion("1");
            argBuilder.AddImageYamlPath("i");
            argBuilder.AddTemplateDirectoryPattern("v");
            argBuilder.AddGit();
            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(-1);
            result.Output.Should().Contain($"Error: Values file pattern not specified");
        }

        [Fact]
        public void Execute_MissingImageYamlParam()
        {
            var argBuilder = Create();
            argBuilder.AddTenantFile("t");
            argBuilder.AddVersion("1");
            argBuilder.AddTemplateDirectoryPattern("v");
            argBuilder.AddGit();
            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(-1);
            result.Output.Should().Contain($"Error: Image yaml path not specified");
        }

        [Fact]
        public void Execute_InvalidTemplateDirectoryPatternParam()
        {
            var argBuilder = Create();
            argBuilder.AddTenantFile("t");
            argBuilder.AddVersion("1");
            argBuilder.AddImageYamlPath("path");
            argBuilder.AddTemplateDirectoryPattern("/{v:XXX}");
            argBuilder.AddGit();
            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(-1);
            result.Output.Should().Contain($"Error: Template directory pattern requires a separator between version segments");
        }

        [Fact]
        public void Execute_InvalidValuesFilePatternParam()
        {
            var argBuilder = Create();
            argBuilder.AddTenantFile("t");
            argBuilder.AddVersion("1");
            argBuilder.AddImageYamlPath("path");
            argBuilder.AddTemplateDirectoryPattern("/{v:X.X.X}");
            argBuilder.AddValuesFilePattern("/{v:XXX}");
            argBuilder.AddGit();
            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(-1);
            result.Output.Should().Contain($"Error: Values file pattern requires a separator between version segments");
        }

        [Fact]
        public async Task Execute_HelmNested_RequiresMove()
        {
            var imageYamlPath = "/child/image2";

            _gitClient.WorkingDirectory = _fileSystem.Directory.CreateTempSubdirectory().FullName;

            var tenantFile = "/files/tenants.csv";
            _fileSystem.AddFile(tenantFile, new MockFileData("dev,tenant1,1.0.1.0"));

            var tenant1Values100 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.0/dev/tenant1/values-tenant1.yaml");
            var dir101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1");
            var default101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/default.yaml");

            _fileSystem.AddFileFromEmbeddedResource(tenant1Values100, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Values1.yaml");
            _fileSystem.AddDirectory(dir101);
            _fileSystem.AddFileFromEmbeddedResource(default101, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Default1.yaml");

            var argBuilder = Create();
            argBuilder.AddTenantFile(tenantFile);
            argBuilder.AddVersion("1.0.1.0-prod");
            argBuilder.AddTemplateDirectoryPattern("helm-deployments/helm-{vX.X.X}");
            argBuilder.AddValuesFilePattern("helm-deployments/helm-{vX.X.X}/{environment}/{tenant}/values-{tenant}.yaml");
            argBuilder.AddDefaultValuesFilePattern("helm-deployments/helm-{vX.X.X}/default.yaml");
            argBuilder.AddImageYamlPath(imageYamlPath);
            argBuilder.AddGit();
            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(0);

            _fileSystem.File.Exists(tenant1Values100).Should().BeFalse(); // Yaml image = 1.0.0.0

            var tenant1Values101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/dev/tenant1/values-tenant1.yaml");
            _fileSystem.File.Exists(tenant1Values101).Should().BeTrue();

            var yamlValuesFile = await YamlHelper.ReadYamlFile(_fileSystem, tenant1Values101);
            var imageValue = YamlHelper.QueryYaml(yamlValuesFile, imageYamlPath);

            imageValue.Should().Be("1.0.1.0-prod");
            var settings = new VerifySettings();
            settings.ScrubInlineDateTimes("O");
            await Verify(result.Output, settings);
        }

        [Fact]
        public async Task Execute_HelmSeparate()
        {
            var imageYamlPath = "/child/image2";

            _gitClient.WorkingDirectory = _fileSystem.Directory.CreateTempSubdirectory().FullName;

            var tenantFile = "/files/tenants.csv";
            _fileSystem.AddFile(tenantFile, new MockFileData("dev,tenant1,1.0.*.0"));

            var valuesFile01 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"environments/dev/tenant1/values.yaml");
            var dir101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1");
            var defaultFile101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/default.yaml");
            _fileSystem.AddFileFromEmbeddedResource(valuesFile01, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Values1.yaml");
            _fileSystem.AddDirectory(dir101);
            _fileSystem.AddFileFromEmbeddedResource(defaultFile101, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Default1.yaml");

            var argBuilder = Create();
            argBuilder.AddTenantFile(tenantFile);
            argBuilder.AddVersion("1.0.1.0");
            argBuilder.AddTemplateDirectoryPattern("helm-deployments/helm-{vX.X.X}");
            argBuilder.AddValuesFilePattern("environments/{environment}/{tenant}/values.yaml");
            argBuilder.AddDefaultValuesFilePattern("helm-deployments/helm-{vX.X.X}/default.yaml");
            argBuilder.AddImageYamlPath(imageYamlPath);
            argBuilder.AddGit();
            argBuilder.AddGitMessage("test message");

            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(0);

            _fileSystem.File.Exists(valuesFile01).Should().BeTrue();

            var yamlValuesFile = await YamlHelper.ReadYamlFile(_fileSystem, valuesFile01);
            var imageValue = YamlHelper.QueryYaml(yamlValuesFile, imageYamlPath);

            imageValue.Should().Be("1.0.1.0");

            result.Output.Should().Contain("Message - test message");
        }

        [Fact]
        public async Task Execute_HelmSeparate_DowngradeError()
        {
            var imageYamlPath = "/image";

            _gitClient.WorkingDirectory = _fileSystem.Directory.CreateTempSubdirectory().FullName;

            var tenantFile = "/files/tenants.csv";
            _fileSystem.AddFile(tenantFile, new MockFileData("dev,tenant1,1.0.*.0"));

            var valuesFile01 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"environments/dev/tenant1/values.yaml");
            var dir101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1");
            var defaultFile101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/default.yaml");
            _fileSystem.AddFileFromEmbeddedResource(valuesFile01, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Values3.yaml");
            _fileSystem.AddDirectory(dir101);
            _fileSystem.AddFileFromEmbeddedResource(defaultFile101, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Default1.yaml");

            var argBuilder = Create();
            argBuilder.AddTenantFile(tenantFile);
            argBuilder.AddVersion("1.0.1.0-dev");
            argBuilder.AddTemplateDirectoryPattern("helm-deployments/helm-{vX.X.X}");
            argBuilder.AddValuesFilePattern("environments/{environment}/{tenant}/values.yaml");
            argBuilder.AddDefaultValuesFilePattern("helm-deployments/helm-{vX.X.X}/default.yaml");
            argBuilder.AddImageYamlPath(imageYamlPath);
            argBuilder.AddGit();
            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(0);

            _fileSystem.File.Exists(valuesFile01).Should().BeTrue();

            var yamlValuesFile = await YamlHelper.ReadYamlFile(_fileSystem, valuesFile01);
            var imageValue = YamlHelper.QueryYaml(yamlValuesFile, imageYamlPath);

            imageValue.Should().Be("1.0.3.0-dev");
        }

        [Fact]
        public async Task Execute_HelmSeparate_WithTemplateYamlPath()
        {
            var imageYamlPath = "/tenant/image_version";
            var templateYamlPath = "/tenant/manifest_version";

            _gitClient.WorkingDirectory = _fileSystem.Directory.CreateTempSubdirectory().FullName;

            var tenantFile = "/files/tenants.csv";
            _fileSystem.AddFile(tenantFile, new MockFileData("dev,tenant1,1.0.*.0"));

            var valuesFile01 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"environments/dev/tenant1/values.yaml");
            var dir101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1");
            var defaultFile101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/default.yaml");
            _fileSystem.AddFileFromEmbeddedResource(valuesFile01, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Values2.yaml");
            _fileSystem.AddDirectory(dir101);

            var argBuilder = Create();
            argBuilder.AddTenantFile(tenantFile);
            argBuilder.AddVersion("1.0.1.0-dev");
            argBuilder.AddTemplateDirectoryPattern("helm-deployments/helm-{vX.X.X}");
            argBuilder.AddValuesFilePattern("environments/{environment}/{tenant}/values.yaml");
            argBuilder.AddImageYamlPath(imageYamlPath);
            argBuilder.AddTemplateYamlPath(templateYamlPath);
            argBuilder.AddGit();
            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(0);

            _fileSystem.File.Exists(valuesFile01).Should().BeTrue();

            var yamlValuesFile = await YamlHelper.ReadYamlFile(_fileSystem, valuesFile01);
            var imageValue = YamlHelper.QueryYaml(yamlValuesFile, imageYamlPath);
            var templateValue = YamlHelper.QueryYaml(yamlValuesFile, templateYamlPath);

            imageValue.Should().Be("1.0.1.0-dev");
            templateValue.Should().Be("1.0.1");
        }

        [Fact]
        public async Task Execute_HelmSeparate_RequiresCreate()
        {
            var imageYamlPath = "/child/image2";

            _gitClient.WorkingDirectory = _fileSystem.Directory.CreateTempSubdirectory().FullName;

            var tenantFile = "/files/tenants.csv";
            _fileSystem.AddFile(tenantFile, new MockFileData("dev,tenant1,1.0.*.0"));

            var valuesFile01 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"environments/dev/tenant1/values.yaml");
            var dir101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1");
            var defaultFile101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/default.yaml");

            _fileSystem.AddDirectory(dir101);
            _fileSystem.AddFileFromEmbeddedResource(defaultFile101, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Default1.yaml");

            var argBuilder = Create();
            argBuilder.AddTenantFile(tenantFile);
            argBuilder.AddVersion("1.0.1.0");
            argBuilder.AddTemplateDirectoryPattern("helm-deployments/helm-{vX.X.X}");
            argBuilder.AddValuesFilePattern("environments/{environment}/{tenant}/values.yaml");
            argBuilder.AddDefaultValuesFilePattern("helm-deployments/helm-{vX.X.X}/default.yaml");
            argBuilder.AddImageYamlPath(imageYamlPath);
            argBuilder.AddGit();
            argBuilder.AddCreateIfMissing();
            var args = argBuilder.Build();

            _fileSystem.File.Exists(valuesFile01).Should().BeFalse();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(0);

            _fileSystem.File.Exists(valuesFile01).Should().BeTrue();

            var yamlValuesFile = await YamlHelper.ReadYamlFile(_fileSystem, valuesFile01);
            var imageValue = YamlHelper.QueryYaml(yamlValuesFile, imageYamlPath);

            imageValue.Should().Be("1.0.1.0");
        }

        [Fact]
        public void Execute_HelmSeparate_IgnoreCreate()
        {
            var imageYamlPath = "/child/image2";

            _gitClient.WorkingDirectory = _fileSystem.Directory.CreateTempSubdirectory().FullName;

            var tenantFile = "/files/tenants.csv";
            _fileSystem.AddFile(tenantFile, new MockFileData("dev,tenant1,1.0.*.0"));

            var valuesFile01 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"environments/dev/tenant1/values.yaml");
            var dir101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1");
            var defaultFile101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/default.yaml");

            _fileSystem.AddDirectory(dir101);
            _fileSystem.AddFileFromEmbeddedResource(defaultFile101, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Default1.yaml");

            var argBuilder = Create();
            argBuilder.AddTenantFile(tenantFile);
            argBuilder.AddVersion("1.0.1.0");
            argBuilder.AddTemplateDirectoryPattern("helm-deployments/helm-{vX.X.X}");
            argBuilder.AddValuesFilePattern("environments/{environment}/{tenant}/values.yaml");
            argBuilder.AddDefaultValuesFilePattern("helm-deployments/helm-{vX.X.X}/default.yaml");
            argBuilder.AddImageYamlPath(imageYamlPath);
            argBuilder.AddGit();
            var args = argBuilder.Build();

            _fileSystem.File.Exists(valuesFile01).Should().BeFalse();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(0);

            _fileSystem.File.Exists(valuesFile01).Should().BeFalse();

            result.Output.Should().Contain("Unable to find any values file");
        }

        [Fact]
        public async Task Execute_HelmNested_SkipFolderVersion()
        {
            var imageYamlPath = "/child/image2";

            _gitClient.WorkingDirectory = _fileSystem.Directory.CreateTempSubdirectory().FullName;

            var tenantFile = "/files/tenants.csv";
            _fileSystem.AddFile(tenantFile, new MockFileData("dev,tenant1,1.0.*.0"));

            var tenant1Values100 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.0/dev/tenant1/values-tenant1.yaml");
            var dir102 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.12");
            var defaultFile102 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.12/default.yaml");
            _fileSystem.AddFile(tenant1Values100, new MockFileData(""));
            _fileSystem.AddDirectory(dir102);
            _fileSystem.AddFileFromEmbeddedResource(defaultFile102, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Values1.yaml");

            var argBuilder = Create();
            argBuilder.AddTenantFile(tenantFile);
            argBuilder.AddVersion("1.0.12.0");
            argBuilder.AddTemplateDirectoryPattern("helm-deployments/helm-{vX.X.X}");
            argBuilder.AddValuesFilePattern("helm-deployments/helm-{vX.X.X}/{environment}/{tenant}/values-{tenant}.yaml");
            argBuilder.AddDefaultValuesFilePattern("helm-deployments/helm-{vX.X.X}/default.yaml");
            argBuilder.AddImageYamlPath(imageYamlPath);
            argBuilder.AddGit();
            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(0);

            _fileSystem.File.Exists(tenant1Values100).Should().BeFalse();

            var tenant1Values102 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.12/dev/tenant1/values-tenant1.yaml");
            _fileSystem.File.Exists(tenant1Values102).Should().BeTrue();

            var yamlValuesFile = await YamlHelper.ReadYamlFile(_fileSystem, tenant1Values102);
            var imageValue = YamlHelper.QueryYaml(yamlValuesFile, imageYamlPath);

            imageValue.Should().Be("1.0.12.0");
        }

        [Fact]
        public async Task Execute_HelmNested_SkipVersion()
        {
            var imageYamlPath = "/child/image2";

            _gitClient.WorkingDirectory = _fileSystem.Directory.CreateTempSubdirectory().FullName;

            var tenantFile = "/files/tenants.csv";
            _fileSystem.AddFile(tenantFile, new MockFileData("dev,tenant1,1.0.*.0"));

            var tenant1Values100 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.0/dev/tenant1/values-tenant1.yaml");
            var dir101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1");
            var dir102 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.2");
            var defaultFile102 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.2/default.yaml");
            _fileSystem.AddFile(tenant1Values100, new MockFileData(""));
            _fileSystem.AddDirectory(dir101);
            _fileSystem.AddDirectory(dir102);
            _fileSystem.AddFileFromEmbeddedResource(defaultFile102, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Values1.yaml");

            var argBuilder = Create();
            argBuilder.AddTenantFile(tenantFile);
            argBuilder.AddVersion("1.0.2.0");
            argBuilder.AddTemplateDirectoryPattern("helm-deployments/helm-{vX.X.X}");
            argBuilder.AddValuesFilePattern("helm-deployments/helm-{vX.X.X}/{environment}/{tenant}/values-{tenant}.yaml");
            argBuilder.AddDefaultValuesFilePattern("helm-deployments/helm-{vX.X.X}/default.yaml");
            argBuilder.AddImageYamlPath(imageYamlPath);
            argBuilder.AddGit();
            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(0);

            _fileSystem.File.Exists(tenant1Values100).Should().BeFalse();

            var tenant1Values102 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.2/dev/tenant1/values-tenant1.yaml");
            _fileSystem.File.Exists(tenant1Values102).Should().BeTrue();

            var yamlValuesFile = await YamlHelper.ReadYamlFile(_fileSystem, tenant1Values102);
            var imageValue = YamlHelper.QueryYaml(yamlValuesFile, imageYamlPath);

            imageValue.Should().Be("1.0.2.0");
        }

        [Fact]
        public void Execute_Git_CloneRepo_Failed()
        {
            var argBuilder = Create();
            argBuilder.AddTenantFile("test.txt");
            argBuilder.AddVersion("1.0.1.0");
            argBuilder.AddTemplateDirectoryPattern("helm-deployments/helm-{vX.X.X}");
            argBuilder.AddValuesFilePattern("helm-deployments/helm-{vX.X.X}/{environment}/{tenant}/values-{tenant}.yaml");
            argBuilder.AddDefaultValuesFilePattern("helm-deployments/helm-{vX.X.X}/default.yaml");
            argBuilder.AddImageYamlPath("/child/image2");
            argBuilder.AddGit();
            var args = argBuilder.Build();

            _gitClient.CloneSuccessful = false;
            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(1);

            result.Output.Should().Contain("Clone failed");
        }

        [Fact]
        public void Execute_Git_PushFiles_Failed()
        {
            var imageYamlPath = "/child/image2";

            _gitClient.WorkingDirectory = _fileSystem.Directory.CreateTempSubdirectory().FullName;

            var tenantFile = "/files/tenants.csv";
            _fileSystem.AddFile(tenantFile, new MockFileData("dev,tenant1,1.0.1.0"));

            var tenant1Values100 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.0/dev/tenant1/values-tenant1.yaml");
            var dir101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1");
            var default101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/default.yaml");

            _fileSystem.AddFileFromEmbeddedResource(tenant1Values100, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Values1.yaml");
            _fileSystem.AddDirectory(dir101);
            _fileSystem.AddFileFromEmbeddedResource(default101, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Default1.yaml");

            var argBuilder = Create();
            argBuilder.AddTenantFile(tenantFile);
            argBuilder.AddVersion("1.0.1.0");
            argBuilder.AddTemplateDirectoryPattern("helm-deployments/helm-{vX.X.X}");
            argBuilder.AddValuesFilePattern("helm-deployments/helm-{vX.X.X}/{environment}/{tenant}/values-{tenant}.yaml");
            argBuilder.AddDefaultValuesFilePattern("helm-deployments/helm-{vX.X.X}/default.yaml");
            argBuilder.AddImageYamlPath(imageYamlPath);
            argBuilder.AddGit();
            var args = argBuilder.Build();

            _gitClient.PushSuccessful = false;
            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(1);
            result.Output.Should().Contain("Push failed");
        }

        [Fact]
        public async Task Execute_HelmNested_RequiresCreate()
        {
            var imageYamlPath = "/child/image2";

            _gitClient.WorkingDirectory = _fileSystem.Directory.CreateTempSubdirectory().FullName;

            var tenantFile = "/files/tenants.csv";
            _fileSystem.AddFile(tenantFile, new MockFileData("dev,tenant1,1.0.1.0"));

            var dir101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1");
            var default101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/default.yaml");

            _fileSystem.AddDirectory(dir101);
            _fileSystem.AddFileFromEmbeddedResource(default101, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Default1.yaml");

            var argBuilder = Create();
            argBuilder.AddTenantFile(tenantFile);
            argBuilder.AddVersion("1.0.1.0");
            argBuilder.AddTemplateDirectoryPattern("helm-deployments/helm-{vX.X.X}");
            argBuilder.AddValuesFilePattern("helm-deployments/helm-{vX.X.X}/{environment}/{tenant}/values-{tenant}.yaml");
            argBuilder.AddDefaultValuesFilePattern("helm-deployments/helm-{vX.X.X}/default.yaml");
            argBuilder.AddImageYamlPath(imageYamlPath);
            argBuilder.AddGit();
            argBuilder.AddCreateIfMissing();
            var args = argBuilder.Build();

            var tenant1Values101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/dev/tenant1/values-tenant1.yaml");

            _fileSystem.File.Exists(tenant1Values101).Should().BeFalse();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(0);

            _fileSystem.File.Exists(tenant1Values101).Should().BeTrue();

            var yamlValuesFile = await YamlHelper.ReadYamlFile(_fileSystem, tenant1Values101);
            var imageValue = YamlHelper.QueryYaml(yamlValuesFile, imageYamlPath);

            imageValue.Should().Be("1.0.1.0");

            result.Output.Should().NotContain("Dry run results");
            result.Output.Should().Contain("Pushing files to Git Repository");
        }

        [Fact]
        public void Execute_HelmNested_ValuesMissing()
        {
            _gitClient.WorkingDirectory = _fileSystem.Directory.CreateTempSubdirectory().FullName;

            var tenantFile = "/files/tenants.csv";
            _fileSystem.AddFile(tenantFile, new MockFileData("dev,tenant1,1.0.1.0"));

            var dir101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1");
            var default101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/default.yaml");

            _fileSystem.AddDirectory(dir101);
            _fileSystem.AddFileFromEmbeddedResource(default101, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Default1.yaml");

            var argBuilder = Create();
            argBuilder.AddTenantFile(tenantFile);
            argBuilder.AddVersion("1.0.1.0");
            argBuilder.AddTemplateDirectoryPattern("helm-deployments/helm-{vX.X.X}");
            argBuilder.AddValuesFilePattern("helm-deployments/helm-{vX.X.X}/{environment}/{tenant}/values-{tenant}.yaml");
            argBuilder.AddDefaultValuesFilePattern("helm-deployments/helm-{vX.X.X}/default.yaml");
            argBuilder.AddImageYamlPath("/child/image2");
            argBuilder.AddGit();
            var args = argBuilder.Build();

            var tenant1Values101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/dev/tenant1/values-tenant1.yaml");

            _fileSystem.File.Exists(tenant1Values101).Should().BeFalse();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(0);
            result.Output.Should().Contain("Unable to find any values file");

            _fileSystem.File.Exists(tenant1Values101).Should().BeFalse();
        }

        [Fact]
        public void Execute_RuleFailure()
        {
            _gitClient.WorkingDirectory = _fileSystem.Directory.CreateTempSubdirectory().FullName;

            var tenantFile = "/files/tenants.csv";
            _fileSystem.AddFile(tenantFile, new MockFileData("dev,tenant1,1.0.*.0"));

            var valuesFile01 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"environments/dev/tenant1/values.yaml");
            var dir101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1");
            var defaultFile101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/default.yaml");
            _fileSystem.AddFileFromEmbeddedResource(valuesFile01, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Values1.yaml");
            _fileSystem.AddDirectory(dir101);
            _fileSystem.AddFileFromEmbeddedResource(defaultFile101, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Default1.yaml");

            var argBuilder = Create();
            argBuilder.AddTenantFile(tenantFile);
            argBuilder.AddVersion("2.0.1.0");
            argBuilder.AddTemplateDirectoryPattern("helm-deployments/helm-{vX.X.X}");
            argBuilder.AddValuesFilePattern("environments/{environment}/{tenant}/values.yaml");
            argBuilder.AddDefaultValuesFilePattern("helm-deployments/helm-{vX.X.X}/default.yaml");
            argBuilder.AddImageYamlPath("/child/image2");
            argBuilder.AddGit();
            var args = argBuilder.Build();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(0);

            result.Output.Should().Contain("Rule violation '1.0.*.0'");
        }

        [Fact]
        public async Task Execute_HelmNested_RequiresCreate_DryRun()
        {
            var imageYamlPath = "/child/image2";

            _gitClient.WorkingDirectory = _fileSystem.Directory.CreateTempSubdirectory().FullName;

            var tenantFile = "/files/tenants.csv";
            _fileSystem.AddFile(tenantFile, new MockFileData("dev,tenant1,1.0.1.0"));

            var dir101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1");
            var default101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/default.yaml");

            _fileSystem.AddDirectory(dir101);
            _fileSystem.AddFileFromEmbeddedResource(default101, Assembly.GetExecutingAssembly(), "GitOps.Updater.Tests.Files.Default1.yaml");

            var argBuilder = Create();
            argBuilder.AddTenantFile(tenantFile);
            argBuilder.AddVersion("1.0.1.0");
            argBuilder.AddTemplateDirectoryPattern("helm-deployments/helm-{vX.X.X}");
            argBuilder.AddValuesFilePattern("helm-deployments/helm-{vX.X.X}/{environment}/{tenant}/values-{tenant}.yaml");
            argBuilder.AddDefaultValuesFilePattern("helm-deployments/helm-{vX.X.X}/default.yaml");
            argBuilder.AddImageYamlPath(imageYamlPath);
            argBuilder.AddGit();
            argBuilder.AddCreateIfMissing();
            argBuilder.AddDryRun();
            var args = argBuilder.Build();

            var tenant1Values101 = _fileSystem.Path.Combine(_gitClient.WorkingDirectory, @"helm-deployments/helm-1.0.1/dev/tenant1/values-tenant1.yaml");

            _fileSystem.File.Exists(tenant1Values101).Should().BeFalse();

            var result = Factory.CommandAppTester.Run(args);

            result.ExitCode.Should().Be(0);

            _fileSystem.File.Exists(tenant1Values101).Should().BeTrue();

            var yamlValuesFile = await YamlHelper.ReadYamlFile(_fileSystem, tenant1Values101);
            var imageValue = YamlHelper.QueryYaml(yamlValuesFile, imageYamlPath);

            imageValue.Should().Be("1.0.1.0");

            result.Output.Should().NotContain("Pushing files to Git Repository");
            result.Output.Should().Contain("Dry run results");
        }
    }
}