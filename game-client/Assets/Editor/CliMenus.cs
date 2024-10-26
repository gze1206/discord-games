using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Directory = System.IO.Directory;
using File = UnityEngine.Windows.File;

namespace Editor;

public class CliMenus
{
    private const string DotnetPath =
#if UNITY_EDITOR_WIN
        "dotnet";
#elif UNITY_EDITOR_OSX
        "/usr/local/share/dotnet/dotnet";
#endif
    
    [MenuItem("discord-games/Core/Build (Debug)")]
    public static void BuildCoreDebug()
    {
        BuildCore("Debug");
    }
    
    [MenuItem("discord-games/Core/Build (Release)")]
    public static void BuildCoreRelease()
    {
        BuildCore("Release");
    }

    private static void BuildCore(string env)
    {
        var process = new Process();
        process.StartInfo.FileName = DotnetPath;
        process.StartInfo.Arguments = $"build -c {env}";
        process.StartInfo.WorkingDirectory = "../game-server/Core";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.OutputDataReceived += (_, args) =>
        {
            if (string.IsNullOrEmpty(args.Data)) return;
            Debug.Log(args.Data);
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (string.IsNullOrEmpty(args.Data)) return;
            Debug.LogError(args.Data);
        };
        
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        var buildDir = new DirectoryInfo($"../game-server/Core/bin/{env}/netstandard2.1");
        var outputDirPath = Path.Combine(Application.dataPath, $"Plugins/Core/{env}");
        Debug.Assert(buildDir.Exists);
        Directory.CreateDirectory(outputDirPath);

        foreach (var file in buildDir.EnumerateFiles())
        {
            var dest = Path.Combine(outputDirPath, file.Name);
            file.CopyTo(dest, overwrite: true);
            Debug.Assert(File.Exists(dest));
        }
        
        AssetDatabase.Refresh();
    }
}