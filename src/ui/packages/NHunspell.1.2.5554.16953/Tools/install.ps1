param($installPath, $toolsPath, $package, $project)

# Hunspellx86.dll (32 Bit Windows)
$hunspellx86Dll = $project.ProjectItems.Item("Hunspellx86.dll")
$hunspellx86Dll.Properties.Item("BuildAction").Value = 0 # BuildAction = None
$hunspellx86Dll.Properties.Item("CopyToOutputDirectory").Value = 2 # CopyToOutputDirectory = Copy if newer


# Hunspellx64.dll (64 Bit Windows)
$hunspellx64Dll = $project.ProjectItems.Item("Hunspellx64.dll")
$hunspellx64Dll.Properties.Item("BuildAction").Value = 0 # BuildAction = None
$hunspellx64Dll.Properties.Item("CopyToOutputDirectory").Value = 2 # CopyToOutputDirectory = Copy if newer

