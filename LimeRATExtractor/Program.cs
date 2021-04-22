using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;

namespace LimeRATExtractor
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 2)
            {
                Console.WriteLine(">> r1n9w0rm - Extract config for LimeRAT.");
                Console.WriteLine("Usage: " + System.AppDomain.CurrentDomain.FriendlyName + " <Input File> <Output File>");
                System.Environment.Exit(1);
            }

            string inputPath = args[0];
            string outputPath = args[1];

            bool extracted = extract(inputPath, outputPath);

        }

        class Config
        {
            public string host;
            public int port;
            public string id;
            public string currentMutex;
            public string key;
            public string splitter;
        }

        static bool extract(string input, string output)
        {
            Assembly a = LoadAssembly(input);

            Module[] modules = a.GetModules();
            var types = modules[0].GetTypes();

            foreach (Type t in types)
            {
                string typeName = t.ToString();
                if (typeName != "Lime.Settings.Config")
                {
                    continue;
                }

                var config = new Config();

                config.host = (string)t.GetField("host", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                config.port = Int32.Parse((string)t.GetField("port", BindingFlags.Public | BindingFlags.Static).GetValue(null));
                config.id = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String((string)t.GetField("id", BindingFlags.Public | BindingFlags.Static).GetValue(null)));
                config.currentMutex = (string)t.GetField("currentMutex", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                config.key = (string)t.GetField("key", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                config.splitter = (string)t.GetField("splitter", BindingFlags.Public | BindingFlags.Static).GetValue(null);

                string jsonOutput = JsonConvert.SerializeObject(config);

                Console.WriteLine(jsonOutput);

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(output, true))
                {
                    file.WriteLine(jsonOutput);
                }

                break;
            }
            return true;
        }

        static Assembly LoadAssembly(string input)
        {
            Assembly a = null;
            try
            {
                a = Assembly.Load(System.IO.File.ReadAllBytes(input));
            }
            catch (BadImageFormatException)
            {
                var assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(input);

                using (var memoryStream = new System.IO.MemoryStream())
                {
                    assembly.Write(memoryStream);
                    byte[] asssemblyBytes = memoryStream.ToArray();
                    a = Assembly.Load(asssemblyBytes);
                }
            }

            return a;
        }

    }
}
