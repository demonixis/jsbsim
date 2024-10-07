// Copyright Epic Games, Inc. All Rights Reserved.

using System.IO;
using UnrealBuildTool;

public class JSBSim : ModuleRules
{
    // Set to true for static linking, only for entire open source projects.
    private const bool bUnixStaticLinking = false;

    public JSBSim(ReadOnlyTargetRules Target) : base(Target)
    {
        Type = ModuleType.External;
        
        if (!IsJSBSimSupported(Target.Platform)) return;

        if (Target.Platform == UnrealTargetPlatform.Win64)
            SetupWindowsPlatform(Target.Configuration == UnrealTargetConfiguration.DebugGame);
        else
            SetupUnixPlatform(Target.Platform, Target.RelativeEnginePath);
    }

    private void SetupWindowsPlatform(bool bDebug)
    {
        string JSBSimLocalFolder = "JSBSim";
        string LibFolderName = "Lib";

        // When working in debug mode, try to use the Debug version of JSBSim
        if (bDebug)
        {
            // Source\ThirdParty\JSBSim\LibDebug
            string DebugLibsPath = Path.Combine(ModuleDirectory, JSBSimLocalFolder, LibFolderName + "Debug");
            if (Directory.Exists(DebugLibsPath))
            {
                System.Console.WriteLine(string.Format("Found Debug libraries for JSBSim in {0}", DebugLibsPath));
                LibFolderName += "Debug";
            }
        }

        // Include headers
        PublicSystemIncludePaths.Add(Path.Combine(ModuleDirectory, JSBSimLocalFolder, "Include"));

        // Link Lib
        string LibPath = Path.Combine(ModuleDirectory, JSBSimLocalFolder, LibFolderName);
        PublicAdditionalLibraries.Add(Path.Combine(LibPath, "JSBSim.lib"));

        // Stage DLL along the binaries files
        string DllFullPath = CheckForFile(LibPath, "JSBSim.dll");
        RuntimeDependencies.Add("$(BinaryOutputDir)/" + "JSBSim.dll", DllFullPath);
    }

    private void SetupUnixPlatform(UnrealTargetPlatform InPlatform, string RelativeEnginePath)
    {
        string JSBSimLocalFolder = "JSBSim";
        string LibFolderName = "Lib";
        string LibPath = Path.Combine(ModuleDirectory, JSBSimLocalFolder, LibFolderName, $"{InPlatform}");
        
        // Include headers
        string IncludePath = Path.Combine(ModuleDirectory, JSBSimLocalFolder, "Include");
        PublicSystemIncludePaths.Add(IncludePath);

		bool bApplePlatform = InPlatform == UnrealTargetPlatform.Mac ||
			InPlatform == UnrealTargetPlatform.IOS ||
			InPlatform == UnrealTargetPlatform.VisionOS;

        string libExt = bApplePlatform ? "dylib" : "so";

        if (bUnixStaticLinking)
            libExt = "a";
        
        LibPath = CheckForFile(LibPath, $"libJSBSim.{libExt}");
        
        PublicAdditionalLibraries.Add(LibPath);
        
        if (!bUnixStaticLinking)
        {   
            RuntimeDependencies.Add(LibPath);
        
            if (InPlatform == UnrealTargetPlatform.Android)
            {
                string PluginPath = Utils.MakePathRelativeTo(ModuleDirectory, RelativeEnginePath);
                AdditionalPropertiesForReceipt.Add("AndroidPlugin", Path.Combine(PluginPath, "JSBSim/JSBSim_APL.xml"));
            }
        }
    }

    private static string CheckForFile(string path, string file)
    {
        string filePath = Path.Combine(path, file);

        if (File.Exists(filePath)) 
        {
            System.Console.WriteLine($"JSBSim Path Check OK: {filePath}");
            return filePath;
        }
        
        string Err = $"{file} not found at {path}";
        System.Console.WriteLine(Err);

        throw new BuildException(Err);
    }
    
    private static void IsJSBSimSupported(UnrealTargetPlatform InPlatform)
    {
        // Base support
        bool bJSBSimSupported = InPlatform == UnrealTargetPlatform.Win64 ||
                                InPlatform == UnrealTargetPlatform.Mac || 
                                InPlatform == UnrealTargetPlatform.Linux;

        string pluginRoot = Path.GetFullPath(Path.Combine(ModuleDirectory, "..", ".."));
        string configPath = Path.Combine(pluginRoot, "PluginConfig.ini");

        // Check for experimental support
        if (File.Exists(configPath)) 
        {
            string[] lines = File.ReadAllLines(configPath);

            foreach (var line in lines)
            {
                if (!line.Contains("=")) continue;

                string[] parts = line.Split('=');

                if (parts.Length != 2) continue;

                string key = parts[0].Trim();
                string value = parts[1].Trim().ToLower();

                if ((key == "bExperimentalAndroidSupport" && value == "true" && InPlatform == UnrealTargetPlatform.Android) ||
                    (key == "bExperimentalIOSSupport" && value == "true" && InPlatform == UnrealTargetPlatform.IOS) ||
                    (key == "bExperimentalVisionOSSupport" && value == "true" && InPlatform == UnrealTargetPlatform.VisionOS))
                {
                    bJSBSimSupported = true;
                }
            }
        }

        // Add a symbol to enable/disable JSBSim specific code
        PublicDefinitions.Add($"JSBSIM_SUPPORTED={(bJSBSimSupported ? 1 : 0)}");

        return bJSBSimSupported;
	}
}
