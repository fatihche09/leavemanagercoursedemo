using System.Reflection;
using System.Xml.Linq;

namespace LeaveManager.Architecture.Tests;

// docs/architecture.md, Forbidden dependencies 1 and 4 (assembly-reference level).
// Checks both what the compiled assembly references and what its .csproj declares:
// the compiler drops a project reference no code uses yet, so the .csproj catches it early.
public class LayerDependencyTests
{
    // Ordered top → bottom: Api → Business → DataAccess → Entities.
    private static readonly string[] _layers =
    [
        "LeaveManager.Api",
        "LeaveManager.Business",
        "LeaveManager.DataAccess",
        "LeaveManager.Entities",
    ];

    public static TheoryData<string> LowerLayers => new(_layers.Skip(1));

    [Theory]
    [MemberData(nameof(LowerLayers))]
    public void LowerLayers_DoNotReferenceUpperLayers(string layer)
    {
        var upper = _layers.TakeWhile(l => l != layer).ToHashSet();

        var violations = AllReferences(layer).Where(upper.Contains).Distinct().ToList();

        Assert.True(violations.Count == 0, $"{layer} references upper layer(s): {string.Join(", ", violations)}");
    }

    // Allow-list, not a deny-list: Entities is plain C# on the base SDK, nothing else.
    [Fact]
    public void Entities_ReferencesNoProjectOrFramework()
    {
        var project = ProjectFile("LeaveManager.Entities");
        var violations = new List<string>();

        var sdk = project.Root!.Attribute("Sdk")?.Value;
        if (sdk != "Microsoft.NET.Sdk")
        {
            violations.Add($"Sdk=\"{sdk}\"");
        }

        violations.AddRange(project.Descendants()
            .Where(e => e.Name.LocalName is "ProjectReference" or "PackageReference" or "FrameworkReference" or "Reference")
            .Select(e => $"{e.Name.LocalName} {e.Attribute("Include")?.Value}"));

        violations.AddRange(Assembly.Load("LeaveManager.Entities").GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(name => !(name.StartsWith("System.", StringComparison.Ordinal)
                || name is "System" or "netstandard" or "mscorlib")));

        Assert.True(violations.Count == 0, $"LeaveManager.Entities references: {string.Join(", ", violations)}");
    }

    private static IEnumerable<string> AllReferences(string layer) =>
        Assembly.Load(layer).GetReferencedAssemblies().Select(a => a.Name!)
            .Concat(DeclaredProjectReferences(layer));

    private static IEnumerable<string> DeclaredProjectReferences(string layer) =>
        ProjectFile(layer).Descendants("ProjectReference")
            .Select(e => Path.GetFileNameWithoutExtension(e.Attribute("Include")!.Value.Replace('\\', '/')));

    private static XDocument ProjectFile(string layer) =>
        XDocument.Load(Path.Combine(RepoRoot(), "src", "backend", layer, $"{layer}.csproj"));

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "LeaveManager.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("LeaveManager.slnx not found above the test binaries.");
    }
}
