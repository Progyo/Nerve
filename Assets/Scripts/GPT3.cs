using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


/// <summary>
/// Interface which connects to Open-AI's GPT-3
/// </summary>
public static class GPT3
{

    /// <summary>
    /// Open-AI API Key
    /// </summary>
    private static string token = "";
    



    /// <summary>
    /// Load api key
    /// </summary>
    private static void Init() 
    {
        //Only fetch token if it hasn't already been fetched
        if(token == "")
        {
            StreamReader stream = File.OpenText("Documents/Programming/Unity/GPT-3 AI/secret.json");
            string text = stream.ReadToEnd();

            //https://stackoverflow.com/questions/16652763/parsing-json-to-not-objects-in-net-4-0
            JToken data = JObject.Parse(text);
            token = (string) data["key"];
        }
    }

}
