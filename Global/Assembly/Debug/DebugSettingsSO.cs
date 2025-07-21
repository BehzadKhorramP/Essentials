using BEH;
using System;

[Serializable]
public class DebugSettings
{
    public bool IsDebug = true;
}

public class DebugSettingsSO : SwitchableSettingsSO<DebugSettings> { 
}
