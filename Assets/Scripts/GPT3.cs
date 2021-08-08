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
    public static List<string> engineIds = new List<string>();

    /// <summary>
    /// The http client that fetches data from open ai
    /// </summary>
    private static HttpClient httpClient = new HttpClient();

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
    /// Request stuff from Open-AI
    /// </summary>
    /// <param name="prompt"> The prompt </param>
    /// <param name="engine"> The engine to use </param>
    /// <param name="max_tokens"> The max tokens to generate </param>
    /// <param name="parse"> If set to true, will only return the text and not the full json </param>
    /// <returns> Returns the request either raw or parsed </returns>
    public static string Request(string prompt, string engine, int max_tokens=5, bool parse=false) 
    {
        if(engineIds.Count == 0) 
        {
            GetEngines();
        }
        

        string response = "";

        if (engineIds.Contains(engine)) 
        {
            using (var request = new HttpRequestMessage(new HttpMethod("POST"), string.Format("https://api.openai.com/v1/engines/{0}/completions",engine)))
            {
                request.Headers.TryAddWithoutValidation("Authorization", string.Format("Bearer {0}", token));

                request.Content = new StringContent("{\n  \"prompt\": \""+ prompt + "\",\n  \"max_tokens\": "+max_tokens+ ",\n  \"stop\": \"###\" \n}");
                request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");

                System.Threading.Tasks.Task<HttpResponseMessage> res = httpClient.SendAsync(request);
                res.Wait();
                System.Threading.Tasks.Task<string> res2 = res.Result.Content.ReadAsStringAsync();
                res2.Wait();
                response = res2.Result;
            }
            try 
            {
                if (parse)
                {
                    JToken data = JObject.Parse(response);
                    return data["choices"][0]["text"].Value<string>();
                }
            }
            catch(System.Exception e)
            {
                return response;
            }


        }
        else 
        {
            return "None";
        }

        return response;
    }

    /// <summary>
    /// Request stuff from Open-AI
    /// </summary>
    /// <param name="prompt"> The prompt needs to have format "[Prompt,Classification],\n ..., [Prompt, Classification] "</param>
    /// <param name="engine"> The engine to use </param>
    /// <param name="max_tokens"> The max tokens to generate </param>
    /// <param name="parse"> If set to true, will only return the text and not the full json </param>
    /// <returns> Returns the request either raw or parsed </returns>
    public static string RequestClassification(string prompt, string query, List<string> labels, string search_engine="ada", string engine="curie", int max_tokens = 5, bool parse = false)
    {
        if (engineIds.Count == 0)
        {
            GetEngines();
        }


        string response = "";

        if (engineIds.Contains(engine) && engineIds.Contains(search_engine))
        {
            //Turn list of labels into suitable string
            string stringLabels = "";
            foreach(string label in labels) 
            {
                stringLabels += "\""+label+"\",";
            }
            stringLabels = stringLabels.Remove(stringLabels.Length - 1);

            using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.openai.com/v1/classifications"))
            {
                request.Headers.TryAddWithoutValidation("Authorization", string.Format("Bearer {0}", token));

                request.Content = new StringContent("{\n    \"examples\": [\n"+prompt+"],\n    \"query\": \""+query+"\",\n    \"search_model\": \""+search_engine+"\",\n    \"model\": \""+engine+"\",\n    \"labels\":["+stringLabels+"]\n  }");
                request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");

                System.Threading.Tasks.Task<HttpResponseMessage> res = httpClient.SendAsync(request);
                res.Wait();
                System.Threading.Tasks.Task<string> res2 = res.Result.Content.ReadAsStringAsync();
                res2.Wait();
                response = res2.Result;
            }
            

            if (parse)
            {
                JToken data = JObject.Parse(response);
                return data["label"].Value<string>();
            }

        }
        else
        {
            return "None";
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
