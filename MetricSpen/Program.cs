using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace MetricSpen
{
    class Program
    {
        static string code;
        static List<string> listWithFunctions = new List<string>();
        static List<string> listWithGlobalVariables = new List<string>();
        public const int numberForCountingSpen = 2;


        static void deleteGarbage()
        {
            string literalRegularExpression = @"((\"".*\"")|(\'.*\')|(//.*))";
            string multilineRegularExpression = @"\/\*.*?\*\/";
            string tabeRegularExpression = @"\t|\*";
 
            string replace = "";
            code = Regex.Replace(code, literalRegularExpression, replace);
            code = Regex.Replace(code, multilineRegularExpression, replace, RegexOptions.Singleline);
            code = Regex.Replace(code, tabeRegularExpression, replace);
        }

        static void findFunctions()
    {
        string functionNameMainRegularExpression = @"(int|long|FILE|char|short|double|float|void|signed|unsigned)\s+([_a-z\*]\w+)\((.*)";
        string textFunctionRegularExpression = @"(int|long|char|FILE|short|double|float|void|signed|unsigned)\s+([_a-z\*][\w_]+)\((.*?)(?=((int|long|char|short|double|float|void|signed|unsigned)\s+([_a-z\*][\w_]+)\())";
        string replace = "";

        bool flagForCycle = true;

        while (flagForCycle)
        {
            Match findTextFunction = Regex.Match(code, textFunctionRegularExpression, RegexOptions.Singleline);

            string functionText = findTextFunction.Value;
            if (functionText != "")
            {
                listWithFunctions.Add(functionText);
                code = code.Replace(functionText, replace);
            }

            int functionCount = 0;

            foreach (Match endOfText in Regex.Matches(code, @"(int|long|FILE|char|short|double|float|void|signed|unsigned)\s+([_a-z\*]\w+)\(", RegexOptions.Singleline))
                functionCount++;
            if (functionCount == 1)
                flagForCycle = false;
        }

        Match section = Regex.Match(code, functionNameMainRegularExpression, RegexOptions.Singleline);
        listWithFunctions.Add(section.Value);
        code = code.Replace(section.Value, replace);
        
    }

      
        static void findLocalVariables()
        {
            string variablesRegularExpression = @"(?<=\bint|long|FILE|char|short|double|float|void|signed|unsigned\b |\, )(\*?\s+[_a-z]\w*)(?=\s*|\s*\,\s*)";
                string functionNameRegularExpression = @"(int|long|char|FILE|short|double|float|void|signed|unsigned)\s+([_a-z\*]\w+)\(";
                
                string replace = "";

                string[] arrayWithFunctions = new string[listWithFunctions.Count];
                listWithFunctions.CopyTo(arrayWithFunctions);


            for ( int functionCount = 0;  functionCount < listWithFunctions.Count;  functionCount++)
            {
                Match functionName = Regex.Match(arrayWithFunctions[functionCount], functionNameRegularExpression, RegexOptions.Singleline);
                string nameOfFunction = functionName.Value;
                nameOfFunction = nameOfFunction.Remove(nameOfFunction.Length-1, 1);
                arrayWithFunctions[functionCount] = Regex.Replace(arrayWithFunctions[functionCount], functionNameRegularExpression, replace);

                bool flagForCycle = true;

                while (flagForCycle)
                {
                    Match variable = Regex.Match(arrayWithFunctions[functionCount], variablesRegularExpression, RegexOptions.Singleline);
                    if (variable.Value != "")
                    {
                    int spen = 0;
                    
                    for (int variableCount = 0; variableCount < arrayWithFunctions[functionCount].Length; variableCount++)
                    {
                        spen = arrayWithFunctions[functionCount].Split(new string[] { variable.Value }, StringSplitOptions.None).Count() - numberForCountingSpen;
                    }

                    arrayWithFunctions[functionCount] = arrayWithFunctions[functionCount].Replace(variable.Value, replace);

                    Console.Write(nameOfFunction + "   ");
                    Console.Write(variable.Value + "   ");
                    Console.WriteLine(spen);
                    }

                    int variableCountInFunction = 0;

                    foreach (Match endOfFuction in Regex.Matches(arrayWithFunctions[functionCount], variablesRegularExpression, RegexOptions.Singleline))
                        variableCountInFunction++;
                    if (variableCountInFunction == 0)
                         flagForCycle = false;
                }
            }
        }

        static void findGlobalVariables()
        {
            string variablesRegularExpression = @"(?<=\bint|long|FILE|char|short|double|float|void|signed|unsigned\b |\, )(\*?\s+[_a-z]\w*)(?=\s*|\s*\,\s*)";
            string globalVariablesRegularExpression = @"(?<=\bint|long|char|FILE|short|double|float|void|signed|unsigned\b|\,)(\s+[_a-z]\w*)(?=\s*\=|\s*\,|\s*\;)";
            bool flagForCycle = true;
            string replace = "";
                       
            while (flagForCycle)
            {
                Match variable = Regex.Match(code, globalVariablesRegularExpression, RegexOptions.Singleline);
                if (variable.Value != "")
                {
                    listWithGlobalVariables.Add(variable.Value);
                    code = code.Replace(variable.Value, replace);
                   
                    int globalVariableCountInCode = 0;

                    foreach (Match endOfCode in Regex.Matches(code, globalVariablesRegularExpression, RegexOptions.Singleline))
                        globalVariableCountInCode++;
                    if (globalVariableCountInCode == 0)
                        flagForCycle = false;
                }
            }

            string[] arrayWithFunctions = new string[listWithFunctions.Count];
            listWithFunctions.CopyTo(arrayWithFunctions);

            List<string> listWithLocalVariables = new List<string>();

            string[] arrayWithGlobalVariables = new string[listWithGlobalVariables.Count];
            listWithGlobalVariables.CopyTo(arrayWithGlobalVariables);
            int spen = 0;

            for (int globalVariablesCount = 0; globalVariablesCount < listWithGlobalVariables.Count; globalVariablesCount++)
            {
                for (int functionCount = 0; functionCount < listWithFunctions.Count; functionCount++)
                {

                    while (flagForCycle)
                    {
                        Match variable = Regex.Match(arrayWithFunctions[functionCount], variablesRegularExpression, RegexOptions.Singleline);
                        if (variable.Value != "")
                        {
                            listWithLocalVariables.Add(variable.Value);
                            code = code.Replace(variable.Value, replace);

                            int localVariableCountInCode = 0;

                            foreach (Match endOfCode in Regex.Matches(code, variablesRegularExpression, RegexOptions.Singleline))
                                localVariableCountInCode++;
                            if (localVariableCountInCode == 0)
                                flagForCycle = false;
                        }
                    }
     
                        if (!(listWithLocalVariables.Contains(arrayWithGlobalVariables[globalVariablesCount])))
                        {
                            spen = arrayWithFunctions[functionCount].Split(new string[] {arrayWithGlobalVariables[globalVariablesCount]}, StringSplitOptions.None).Count();
                            if (spen == 1)
                                spen = 0;
                        }
                }
                Console.Write("Global variable ");
                Console.Write(arrayWithGlobalVariables[globalVariablesCount] + "   ");
                Console.WriteLine(spen);
                
                spen = 0;
            }
        }

        static void Main(string[] args)
        {
            StreamReader fileWithCode = new StreamReader("d:\\CodeSpen.txt");
            code = fileWithCode.ReadToEnd();
            fileWithCode.Close();

            deleteGarbage();
            findFunctions();
            findLocalVariables();
            findGlobalVariables();
            
            Console.ReadLine();
        }
    }
}
