using Shouldly;
using KSnitch;
using KSnitch.Tests.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using Xunit;
using VerifyXunit;
using Spectre.Verify.Extensions;

namespace KSnitch.Tests
{
    [UsesVerify]
    public class ProgramTests
    {
        [Fact]
        [Expectation("Baz", "Default")]
        public async Task Should_Return_Expected_Result_For_Baz_Not_Specifying_Framework()
        {
            // Given
            var fixture = new Fixture();
            var project = Fixture.GetPath("Baz/Baz.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project);

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Solution", "Default")]
        public async Task Should_Return_Expected_Result_For_Solution_Not_Specifying_Framework()
        {
            // Given
            var fixture = new Fixture();
            var solution = Fixture.GetPath("KSnitch.Tests.Fixtures.sln");

            // When
            var (exitCode, output) = await Fixture.Run(solution);

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Baz", "netstandard2.0")]
        public async Task Should_Return_Expected_Result_For_Baz_Specifying_Framework()
        {
            // Given
            var fixture = new Fixture();
            var project = Fixture.GetPath("Baz/Baz.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project, "--tfm", "netstandard2.0");

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Baz", "netstandard2.0_Strict")]
        public async Task Should_Return_Non_Zero_Exit_Code_For_Baz_When_Running_With_Strict()
        {
            // Given
            var fixture = new Fixture();
            var project = Fixture.GetPath("Baz/Baz.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project, "--tfm", "netstandard2.0", "--strict");

            // Then
            exitCode.ShouldBe(-1);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Baz", "Exclude_Autofac")]
        public async Task Should_Return_Expected_Result_For_Baz_When_Excluding_Library()
        {
            // Given
            var fixture = new Fixture();
            var project = Fixture.GetPath("Baz/Baz.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project, "--exclude", "Autofac");

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Baz", "Skip_Bar")]
        public async Task Should_Return_Expected_Result_For_Baz_When_Skipping_Project()
        {
            // Given
            var fixture = new Fixture();
            var project = Fixture.GetPath("Baz/Baz.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project, "--skip", "Bar");

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        public sealed class Fixture
        {
            public static string GetPath(string path)
            {
                var workingDirectory = Environment.CurrentDirectory;
                var solutionDirectory = Path.GetFullPath(Path.Combine(workingDirectory, "../../../../KSnitch.Tests.Fixtures"));
                return Path.GetFullPath(Path.Combine(solutionDirectory, path));
            }

            public static async Task<(int exitCode, string output)> Run(params string[] args)
            {
                var console = new FakeConsole(supportsAnsi: false);
                var exitCode = await Program.Run(args, c => c.ConfigureConsole(console));
                return (exitCode, console.Output.Trim());
            }
            public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
            {
                foreach (DirectoryInfo dir in source.GetDirectories())
                    if (dir.Name != "temp")
                    {
                        CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
                        foreach (FileInfo file in source.GetFiles())
                            file.CopyTo(Path.Combine(target.FullName, file.Name), true);
                    }
            }
        }

        [Fact]
        [Expectation("FSharp", "Default")]
        public async Task Should_Return_Expected_Result_For_FSharp_Not_Specifying_Framework()
        {
            // Given
            var fixture = new Fixture();
            var project = Fixture.GetPath("FSharp/FSharp.fsproj");

            // When
            var (exitCode, output) = await Fixture.Run(project);

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        [Fact]
        public async Task RemoveCanReferences()
        {
            var fixture = new Fixture();
            //copy solution to temp dir
            
            var solutionpath = Fixture.GetPath("");
            var targetpath = solutionpath + "\\temp";
            Directory.CreateDirectory(targetpath);
            Fixture.CopyFilesRecursively(new DirectoryInfo(solutionpath), new DirectoryInfo(targetpath));

            var paramstrings = new List<string>(); 
            paramstrings.Add(targetpath+ "\\KSnitch.Tests.Fixtures.sln");
            //paramstrings.Add("C:\\dev5.0_git\\kwtools\\kwtools.sln ");
            paramstrings.Add("-c");
            //paramstrings.Add("-n");
            // When
            var (exitCode, output) = await Fixture.Run(paramstrings.ToArray());

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
            Directory.Delete(targetpath,true);

        }

        [Fact]
        public async Task RemoveMightReferences()
        {
            var fixture = new Fixture();
            //copy solution to temp dir

            var solutionpath = Fixture.GetPath("");
            var targetpath = solutionpath + "\\temp";
            Directory.CreateDirectory(targetpath);
            Fixture.CopyFilesRecursively(new DirectoryInfo(solutionpath), new DirectoryInfo(targetpath));

            var paramstrings = new List<string>();
            paramstrings.Add(targetpath + "\\KSnitch.Tests.Fixtures.sln");
            paramstrings.Add("-m");
            // When
            var (exitCode, output) = await Fixture.Run(paramstrings.ToArray());

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
            Directory.Delete(targetpath, true);
        }
    }
}
