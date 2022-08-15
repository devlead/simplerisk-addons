﻿namespace GUIClient.Services;

public interface IMutableConfigurationService
{
    bool IsInitialized { get; }

    void Initialize();

    string GetConfigurationValue(string name);
    
    void SetConfigurationValue(string name, string value);
}