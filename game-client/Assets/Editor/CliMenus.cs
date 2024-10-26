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
    
    [MenuItem("discord-games/Core/Build (Debug)", priority = 1)]
    public static void BuildCoreForDebug()
    {
        EditorUtility.DisplayProgressBar("Build Core", "디버그 빌드 시작", 0f);
        BuildCore("Debug");
        EditorUtility.DisplayProgressBar("Build Core", "임포트", 0.5f);
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    
    [MenuItem("discord-games/Core/Build (Release)", priority = 2)]
    public static void BuildCoreForRelease()
    {
        EditorUtility.DisplayProgressBar("Build Core", "릴리즈 빌드 시작", 0f);
        BuildCore("Release");
        EditorUtility.DisplayProgressBar("Build Core", "임포트", 0.5f);
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("discord-games/Core/Build (all)", priority = 0)]
    public static void BuildCoreForAll()
    {
        EditorUtility.DisplayProgressBar("Build Core", "디버그 빌드 시작", 0f);
        BuildCore("Debug");
        EditorUtility.DisplayProgressBar("Build Core", "릴리즈 빌드 시작", 0.3f);
        BuildCore("Release");
        EditorUtility.DisplayProgressBar("Build Core", "임포트", 0.6f);
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
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

        var hasOutput = false;
        var hasError = false;
        var stdout = new StringBuilder(env);
        var stderr = new StringBuilder(env);
        
        process.OutputDataReceived += (_, args) =>
        {
            if (string.IsNullOrEmpty(args.Data)) return;
            stdout.AppendLine(args.Data);
            hasOutput = true;
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (string.IsNullOrEmpty(args.Data)) return;
            stderr.AppendLine(args.Data);
            hasError = true;
        };
        
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        
        if (hasOutput) Debug.Log(stdout.ToString());
        if (hasError) Debug.LogError(stderr.ToString());

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
    }
}