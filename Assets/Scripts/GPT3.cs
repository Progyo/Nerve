using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;

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
    /// List filled by GetEngines
    /// </summary>
    private static List<string> engineIds = new List<string>();

    /// <summary>
    /// Load api key
    /// </summary>
    private static void Init() 
    {
        //Only fetch token if it hasn't already been fetched
        if(token == "")
        {
            StreamReader stream = File.OpenText("../secret.json");
            string text = stream.ReadToEnd();

            //https://stackoverflow.com/questions/16652763/parsing-json-to-not-objects-in-net-4-0
            JToken data = JObject.Parse(text);
            token = (string) data["key"];
        }
    }

    /// <summary>
    /// Request from Open-AI
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    public static string Request(string prompt, string engine, int max_tokens=5, bool parse=false) 
    {
        if(engineIds.Count == 0) 
        {
            GetEngines();
        }
        

        string response = "";

        if (engineIds.Contains(engine)) 
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), string.Format("https://api.openai.com/v1/engines/{0}/completions",engine)))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", string.Format("Bearer {0}", token));

                    request.Content = new StringContent("{\n  \"prompt\": \""+ prompt + "\",\n  \"max_tokens\": "+max_tokens+"\n}");
                    request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");

                    System.Threading.Tasks.Task<HttpResponseMessage> res = httpClient.SendAsync(request);
                    res.Wait();
                    System.Threading.Tasks.Task<string> res2 = res.Result.Content.ReadAsStringAsync();
                    res2.Wait();
                    response = res2.Result;
                }
            }
        }
        else 
        {
            return "None";
        }

        if (parse) 
        {
            JToken data = JObject.Parse(response);
            return data["choices"][0]["text"].Value<string>();
        }


        return response;
    }


    /// <summary>
    /// Return all engines
    /// </summary>
    /// <returns></returns>
    private static string GetEngines() 
    {
        Init();

        //https://curl.olsh.me/
        //request stuff
        string response = "";
        using (var httpClient = new HttpClient())
        {
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.openai.com/v1/engines"))
            {
                request.Headers.TryAddWithoutValidation("Authorization", string.Format("Bearer {0}", token));

                System.Threading.Tasks.Task<HttpResponseMessage> res = httpClient.SendAsync(request);
                res.Wait();
                System.Threading.Tasks.Task<string> res2 = res.Result.Content.ReadAsStringAsync();
                res2.Wait();
                response = res2.Result;
            }
        }
        //Process request

        engineIds = new List<string>();
        JToken data = JObject.Parse(response);
        foreach(JToken temp in data["data"]) 
        {
            engineIds.Add(temp["id"].Value<string>());
        }

        return response;
    }


}
