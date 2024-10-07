// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System.IO;

public class JSBSimFlightDynamicsModel : ModuleRules
{
	public JSBSimFlightDynamicsModel(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;		
		
		PublicDependencyModuleNames.AddRange(new string[]
		{
			"Core", 
			"CoreUObject", 
			"Engine", 
			"GeoReferencing",
			"Projects"
		});

        if (!IsJSBSimSupported(Target.Platform)) return;
        
        PublicDependencyModuleNames.Add("JSBSim");
        
        // Stage JSBSim data files
        string JSBSimRedistFolder = Path.Combine(PluginDirectory, @"Resources\JSBSim\*");
        RuntimeDependencies.Add(JSBSimRedistFolder, StagedFileType.NonUFS);
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
