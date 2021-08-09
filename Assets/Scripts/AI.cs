using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Inventory for AI
/// </summary>
public class Inventory 
{
    /// <summary>
    /// Items which are stored by the AI
    /// </summary>
    [System.Serializable]
    public struct Item 
    {
        public string name;
        public string description;
        public int quantity;
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
    /// Converts list of items to string for gpt3
    /// </summary>
    /// <param name="items"> The items on that page </param>
    /// <returns></returns>
    private static string ItemListToString(List<Inventory.Item> items, string prompt)
    {
        string outString = "";

        //Information
        foreach (Inventory.Item item in items)
        {
            outString += string.Format("Item: {0} Description: {1} Quantity: {2} \\n###\\n", item.name, item.description, item.quantity);
        }

        List<string> questions = new List<string>() { "How many {0}s do you have?", "I have {0} {1}s", "How much {0} do you have?", "I have {0} {1}", "How many {0}s are there?", "There are {0} {1}s", "Is there a {0}?", "There is {0} {1}" };
        List<string> fakeItems = new List<string>() { "Bazooka", "Plutonium", "Hydrochloric Acid", "Super TNT" };

        List<Item> alreadyAdded = new List<Item>();

        //Questions
        for (int i = 0; i < 6; i++)
        {
            //Randomly select one item
            int index = rng.Next(0, items.Count - 1);
            Inventory.Item temp = items[index];

            bool found = rng.Next(0, 100) >= 50;

            if (found && !alreadyAdded.Contains(temp))
            {
                outString += string.Format("Question: {0}\\nAnswer: {1}\\n###\\n", string.Format(questions[(i * 2) % 4], temp.name), string.Format(questions[(i * 2 + 1) % 4], temp.quantity, temp.name));
                alreadyAdded.Add(temp);
            }
            else if(!found)
            {
                if(rng.Next(0, 100) >= 50) 
                {
                    outString += string.Format("Question: Do you have {0} {1}s? \\nAnswer: No I only have {2}\\n###\\n", temp.quantity+10,temp.name, temp.quantity);
                }
                else 
                {
                    outString += string.Format("Question: {0}\\nAnswer: None\\n###\\n", string.Format(questions[(i * 2) % 4], fakeItems[i % 4]));
                }
                
            }

        }


        outString += string.Format("Question: {0}\\nAnswer: ", prompt);

        return outString;
    }



    /// <summary>
    /// Pass inventory information along with the request to gpt3
    /// </summary>
    /// <param name="listen"> The request being made </param>
    /// <returns> Something </returns>
    public static string ParseInventoryQuestionRequest(string listen, List<Inventory.Item> items)
    {
        /*
         Item: <item name> Description: <description> Quantity: <quantity>
         .
         (max item count per page)
         .
         Question: How many apples are there?
         Answer: There are x apples.
         Question: What do the apples look like?
         Answer: The apples are ...
         Question: Are there any gold bricks?
         Answer: None
         */
        //If None is triggered check next page


        List<Inventory.Item> currentPage = new List<Inventory.Item>();
        List<Inventory.Item> allItems = new List<Inventory.Item>(items);
        Shuffle(ref allItems);
        int maxItemsPerPage = 10;
        bool found = false;
        int pageNumber = 0;
        string prompt = "";
        while (!found && pageNumber <= allItems.Count / maxItemsPerPage)
        {
            //Populate page to send to gpt
            currentPage.Clear();
            for (int i = 0; i < Mathf.Min(items.Count, maxItemsPerPage); i++)
            {
                currentPage.Add(allItems[pageNumber * maxItemsPerPage + i]);
            }

            //Convert item list into gpt3 compatible prompt
            string request = ItemListToString(currentPage, listen);

            Debug.Log(request);
            //Request from gpt3
            prompt = GPT3.Request(request, "curie", 75, true).Split("\n".ToCharArray())[0];
                                                                //prompt = request;

            found = prompt != "None";
            pageNumber++;
        }

        //Could be expensive if wrong engine is used
        if(prompt == "") 
        {
            prompt = ParseInventoryQuestionRequest(listen, items);
        }



        return prompt;
    }



}



/// <summary>
/// All the processing done for the knowledge of the AI
/// </summary>
public class Knowledge 
{
    /// <summary>
    /// Types of fields of information which the AI has access to (Used for initial classification)
    /// </summary>
    public enum CoarseTextType { None, Question, Command, BackStory};

    /// <summary>
    /// The type of question. Personal: e.g "who are you?", "what are you doing?" etc. Environment: e.g "Where is ...?", "Is ... here?"
    /// </summary>
    public enum QuestionType { None, Environment, Personal, Inventory};

    /// <summary>
    /// The type of command. Action: e.g "Dance", "Follow me" etc. Environment: e.g "Show me where ...", "Point at ..."
    /// </summary>
    public enum CommandType { None, Environment, Action };


    //Coarse Question knowledge that all AI's have
    public static List<Text> presetCoarseQuestion = new List<Text>()
    {
        new Text(){
            type=CoarseTextType.Question,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="Hello. Where can I find the pasta aisle?",
            answer="Question"
        },
        new Text(){
            type=CoarseTextType.Question,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="What do you think about this topic?",
            answer="Question"
        },
        new Text(){
            type=CoarseTextType.Question,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="Is this the right way?",
            answer="Question"
        },
        new Text(){
            type=CoarseTextType.Question,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="Could you please follow me?",
            answer="Question"
        },
        new Text(){
            type=CoarseTextType.Question,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="Where is the restroom?",
            answer="Question"
        },
        new Text(){
            type=CoarseTextType.Question,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="What's the point of this contraption?",
            answer="Question"
        },
        new Text(){
            type=CoarseTextType.Question,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="Why are you serious?",
            answer="Question"
        },
        new Text(){
            type=CoarseTextType.Question,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="Where are the bananas?",
            answer="Question"
        },
        new Text(){
            type=CoarseTextType.Question,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="Whats the sword called?",
            answer="Question"
        },
    };


    //Coarse Command knowledge that all AI's have
    public static List<Text> presetCoarseCommand = new List<Text>()
    {
        new Text(){
            type=CoarseTextType.Command,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="Follow me now",
            answer="Command"
        },
        new Text(){
            type=CoarseTextType.Command,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="Go to the market and get some fruits",
            answer="Command"
        },
        new Text(){
            type=CoarseTextType.Command,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="Point at the apples",
            answer="Command"
        },
        new Text(){
            type=CoarseTextType.Command,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="Show me where the restroom is",
            answer="Command"
        },
        new Text(){
            type=CoarseTextType.Command,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="Explain this to me",
            answer="Command"
        },
        new Text(){
            type=CoarseTextType.Command,
            qType=QuestionType.None,
            cType=CommandType.None,
            prompt="Explain why we need two of these",
            answer="Command"
        },
    };

    //Question knowledge that all AI's have
    public static List<Text> presetQuestion = new List<Text>()
    {
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.Environment,
            cType=CommandType.None,
            prompt="Hello. Where can I find the pasta aisle?",
            answer="Environment"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.Personal,
            cType=CommandType.None,
            prompt="Who are you?",
            answer="Personal"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.Environment,
            cType=CommandType.None,
            prompt="Where are you?",
            answer="Environment"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.Environment,
            cType=CommandType.None,
            prompt="Where am I?",
            answer="Environment"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.Environment,
            cType=CommandType.None,
            prompt="Where can I find exaclibur?",
            answer="Environment"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.Environment,
            cType=CommandType.None,
            prompt="Where is Tommy?",
            answer="Environment"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.Personal,
            cType=CommandType.None,
            prompt="Why are you doing this?",
            answer="Personal"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.Personal,
            cType=CommandType.None,
            prompt="Are you alright?",
            answer="Personal"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.Personal,
            cType=CommandType.None,
            prompt="What caused you to do that?",
            answer="Personal"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.Inventory,
            cType=CommandType.None,
            prompt="What do you have on you?",
            answer="Inventory"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.Inventory,
            cType=CommandType.None,
            prompt="Do you have any gold?",
            answer="Inventory"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.Inventory,
            cType=CommandType.None,
            prompt="How much money do you have?",
            answer="Inventory"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.Inventory,
            cType=CommandType.None,
            prompt="Whats the sword called?",
            answer="Inventory"
        },
        
    };


    //Command knowledge that all AI's have
    public static List<Text> presetCommand = new List<Text>()
    {
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.None,
            cType=CommandType.Environment,
            prompt="Point at the green box",
            answer="Environment"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.None,
            cType=CommandType.Action,
            prompt="Dance",
            answer="Action"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.None,
            cType=CommandType.Action,
            prompt="Hide",
            answer="Action"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.None,
            cType=CommandType.Action,
            prompt="Follow me",
            answer="Action"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.None,
            cType=CommandType.Action,
            prompt="Follow me to the cabin",
            answer="Action"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.None,
            cType=CommandType.Environment,
            prompt="Show me how to get to the castle",
            answer="Environment"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.None,
            cType=CommandType.Environment,
            prompt="Run back home",
            answer="Environment"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.None,
            cType=CommandType.Environment,
            prompt="Run to your house",
            answer="Environment"
        },
        new Text(){
            type=CoarseTextType.None,
            qType=QuestionType.None,
            cType=CommandType.Environment,
            prompt="Run to the hill",
            answer="Environment"
        },
    };


    [System.Serializable]
    public struct Text
    {
        public CoarseTextType type;
        public QuestionType qType;
        public CommandType cType;
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



    /// <summary>
    /// Create question Prompt text to be sent to Open-AI
    /// </summary>
    /// <param name="prompts"> List of example prompts </param>
    /// <param name="prompt"> Prompt to complete/classify</param>
    public static string CreateQuestionClassificationPrompt(List<Text> prompts)
    {
        //Shuffle prompts
        Shuffle(ref prompts);

        string outString = "";

        foreach (Text t in prompts)
        {
            if (t.qType != QuestionType.None)
            {
                outString += string.Format("[\"{0}\", \"{1}\"],\n", t.prompt, t.answer);
            }
        }

        outString = outString.Remove(outString.Length - 1);
        outString = outString.Remove(outString.Length - 1);

        return outString;

    }



    /// <summary>
    /// Create command Prompt text to be sent to Open-AI
    /// </summary>
    /// <param name="prompts"> List of example prompts </param>
    /// <param name="prompt"> Prompt to complete/classify</param>
    public static string CreateCommandClassificationPrompt(List<Text> prompts)
    {
        //Shuffle prompts
        Shuffle(ref prompts);

        string outString = "";

        foreach (Text t in prompts)
        {
            if (t.cType != CommandType.None)
            {
                outString += string.Format("[\"{0}\", \"{1}\"],\n", t.prompt, t.answer);
            }
        }

        outString = outString.Remove(outString.Length - 1);
        outString = outString.Remove(outString.Length - 1);

        return outString;

    }

}

public class AI : MonoBehaviour
{


    /// <summary>
    /// The knowledge of the AI
    /// </summary>
    [SerializeField]
    public List<Knowledge.Text> customKnowledge;

    /// <summary>
    /// Internal knowledge of the AI
    /// </summary>
    private List<Knowledge.Text> knowledge;


    //This will be removed later
    public string tempInput = "";

    [SerializeField]
    public Knowledge.Text backStory;

    /// <summary>
    /// The items which the AI has
    /// </summary>
    [SerializeField]
    public List<Inventory.Item> inventory;

    /// <summary>
    /// Internal representation of previous dialog (Will replace soon I think)
    /// </summary>
    private List<string> previousDialog = new List<string>();

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
        foreach (Knowledge.Text text in Knowledge.presetQuestion)
        {
            knowledge.Add(text);
        }
        foreach (Knowledge.Text text in Knowledge.presetCommand)
        {
            knowledge.Add(text);
        }
        foreach (Knowledge.Text text in customKnowledge)
        {
            knowledge.Add(text);
        }

    }


    /// <summary>
    /// Called to interact with the AI
    /// </summary>
    /// <param name="listen"> Whats being said to the AI </param>
    public void Interact(string listen) 
    {
        //Coarse classify whats being said
        string coarse = Knowledge.CreateCoarseClassificationPrompt(knowledge);
        string answer = GPT3.RequestClassification(coarse, listen, new List<string>() { "Question", "Command" }, "ada", "curie", 5, true);

        if(answer == "Question") 
        {
            Question(listen);
        }
        else if (answer == "Command") 
        {
            Command(listen);
        }

    }


    /// <summary>
    /// Function thats called when whats being said is classified as a command
    /// </summary>
    /// <param name="listen"> Whats being said to the AI </param>
    private void Command(string listen) 
    {
        Debug.Log("Command!");
        //Classify whats being said
        string command = Knowledge.CreateCommandClassificationPrompt(knowledge);
        string answer = GPT3.RequestClassification(command, listen, new List<string>() { "Environment", "Action" }, "ada", "curie", 5, true);


        if (answer == "Environment")
        {
            Debug.Log("Environment!");
        }
        else if (answer == "Action")
        {
            Debug.Log("Action!");
        }

    }


    /// <summary>
    /// Function thats called when whats being said is classified as a question
    /// </summary>
    /// <param name="listen"> Whats being said to the AI </param>
    private void Question(string listen)
    {
        string player = "Progyo";
        Debug.Log("Question!");
        //Classify whats being said
        string question = Knowledge.CreateQuestionClassificationPrompt(knowledge);
        string answer = GPT3.RequestClassification(question, listen, new List<string>() { "Environment", "Personal", "Inventory" }, "ada", "curie", 5, true);
        Debug.Log(answer);

        if (answer == "Environment") 
        {
            Debug.Log(answer);
        }
        else if(answer == "Personal") 
        {
            Debug.Log("Personal!");
            
            string prompt = "";
            if (previousDialog.Count == 0) 
            {
                prompt = string.Format("{0}\\nA player named {1} approaches you and asks: {2}\\n###\\nYou respond by saying: ", backStory.prompt, player, listen);
            }
            else 
            {
                string previous = "";

                foreach(string s in previousDialog) 
                {
                    previous += s + "\\n###\\n";
                }

                prompt = string.Format("{0}\\nYou and a player named {1} have been talking already. This was your previous dialog. {2}{1} now asks you: {3}\\n###\\nYou respond by saying: ", backStory.prompt, player, previous,listen);
            }
            Debug.Log("Prompt \n " + prompt);
            answer = GPT3.Request(prompt, "davinci", 75, true).Split("\n".ToCharArray())[0];   

            Debug.Log(answer);
        }
        else if(answer == "Inventory") 
        {

            string output = Inventory.ParseInventoryQuestionRequest(listen, inventory);
            Debug.Log(output);
        }

        previousDialog.Add(string.Format("{0}: {1}\\n###\\nYou: {2}", player, listen, answer));

    }




    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            Interact(tempInput);
        }
    }

}
