using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All the processing done for the knowledge of the AI
/// </summary>
public class Knowledge 
{
    /// <summary>
    /// Types of fields of information which the AI has access to (Used for initial classification)
    /// </summary>
    public enum CoarseTextType { Question, Command, Dialog, Biography};

    //Question knowledge that all AI's have
    public static List<Text> presetCoarseQuestion = new List<Text>() 
    {
        new Text(){
            type=CoarseTextType.Question,
            prompt="Hello. Where can I find the pasta aisle?",
            answer="Question"
        },
        new Text(){
            type=CoarseTextType.Question,
            prompt="What do you think about this topic?",
            answer="Question"
        },
        new Text(){
            type=CoarseTextType.Question,
            prompt="Is this the right way?",
            answer="Question"
        },
        new Text(){
            type=CoarseTextType.Question,
            prompt="Could you please follow me?",
            answer="Question"
        },
        new Text(){
            type=CoarseTextType.Question,
            prompt="Where is the restroom?",
            answer="Question"
        },
        new Text(){
            type=CoarseTextType.Question,
            prompt="What's the point of this contraption?",
            answer="Question"
        },
    };


    //Command knowledge that all AI's have
    public static List<Text> presetCoarseCommand = new List<Text>()
    {
        new Text(){
            type=CoarseTextType.Command,
            prompt="Follow me now",
            answer="Command"
        },
        new Text(){
            type=CoarseTextType.Command,
            prompt="Go to the market and get some fruits",
            answer="Command"
        },
        new Text(){
            type=CoarseTextType.Command,
            prompt="Point at the apples",
            answer="Command"
        },
        new Text(){
            type=CoarseTextType.Command,
            prompt="Show me where the restroom is",
            answer="Command"
        },
        new Text(){
            type=CoarseTextType.Command,
            prompt="Go to the market and get some fruits",
            answer="Command"
        },
        new Text(){
            type=CoarseTextType.Command,
            prompt="Explain this to me",
            answer="Command"
        },
        new Text(){
            type=CoarseTextType.Command,
            prompt="Explain why we need two of these",
            answer="Command"
        },
    };


    [System.Serializable]
    public struct Text
    {
        public CoarseTextType type;
        public string prompt;
        public string answer;
    }


    private static System.Random rng = new System.Random();
    /// <summary>
    /// Shuffles a list
    /// https://stackoverflow.com/questions/273313/randomize-a-listt
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    private static void Shuffle<T>(ref List<T> list) 
    {
        
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Create Prompt text to be sent to Open-AI
    /// </summary>
    /// <param name="prompts"> List of example prompts </param>
    /// <param name="prompt"> Prompt to complete/classify</param>
    public static string CreateCoarseClassificationPrompt(List<Text> prompts, string prompt="") 
    {
        //Shuffle prompts
        Shuffle(ref prompts);

        string outString = "";

        foreach(Text t in prompts) 
        {
            if(t.type == CoarseTextType.Command || t.type == CoarseTextType.Question) 
            {
                if (prompt != "")
                {
                    //Text completion format
                    outString += string.Format("Input: {0}\nType: {1}\n###\n", t.prompt, t.answer);
                }
                else 
                {
                    //Text classification format
                    outString += string.Format("[\"{0}\", \"{1}\"],\n", t.prompt, t.answer);
                }
            }
        }



        if (prompt != "")
        {
            outString += string.Format("Input: {0}\nType: ", prompt);
        }
        else 
        {
            outString = outString.Remove(outString.Length - 1);
            outString = outString.Remove(outString.Length - 1);
        }

        return outString;

    }


}

public class AI : MonoBehaviour
{


    /// <summary>
    /// The knowledge of the AI
    /// </summary>
    [SerializeField]
    public List<Knowledge.Text> customknowledge;


    private List<Knowledge.Text> knowledge;

    void Start()
    {

        //Insert all preset knowledge and custom knowledge

        knowledge = new List<Knowledge.Text>();

        foreach(Knowledge.Text text in Knowledge.presetCoarseQuestion) 
        {
            knowledge.Add(text);
        }
        foreach (Knowledge.Text text in Knowledge.presetCoarseCommand)
        {
            knowledge.Add(text);
        }
        foreach (Knowledge.Text text in customknowledge)
        {
            knowledge.Add(text);
        }

        string tempText = Knowledge.CreateCoarseClassificationPrompt(knowledge);

        Debug.Log(tempText);

        Debug.Log(GPT3.RequestClassification(tempText, "What's the meaning behind life?", new List<string>() { "Question", "Command" }, "ada", "curie", 5, true));

    }

}
