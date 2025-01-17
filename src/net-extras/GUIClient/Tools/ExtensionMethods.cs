﻿namespace GUIClient.Tools;
using System.Text.Json;

public static class ExtensionMethods
{
    public static T? DeepCopy<T>(this T self)
    {
        //var serialized = JsonConvert.SerializeObject(self);
        var serialized = JsonSerializer.Serialize(self);
        return JsonSerializer.Deserialize<T>(serialized);
        //return JsonConvert.DeserializeObject<T>(serialized);
    }
}

