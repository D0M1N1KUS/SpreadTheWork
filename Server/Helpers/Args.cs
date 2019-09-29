using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Server.Helpers
{
    /// <summary>
    /// Interprets commandline arguments.
    /// -a [Assembly location] -> loads the assembly from provided location.
    /// -args [[Arg1]] [[Arg2]] [[Arg3]] ... -> parameters for the algorithm provided by the assembly.
    ///     Each args parameter needs to be enclosed by square brackets []. 
    /// -argfile [File location] -> file containing data used by the algorithm.
    /// -c [Number of clients] -> expected number of clients to join.
    /// </summary>
    public class Args
    {
        private NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string[] cmdArgs;
        private Dictionary<string, bool> foundArgs = new Dictionary<string, bool>()
            {{"-a", false}};
        private List<string> knownArgs = new List<string>()
        {
            "-a", "-args", "-argfile", "-c"
        };

        public Assembly LoadedAssembly { get; private set; } = null;
        public List<string> Parameters { get; private set; }
        public string ArgfileLocation { get; private set; }
        public int ExpectedNumberOfClients { get; private set; }
        public bool InitializationSucceeded { get; }
        
        public Args(string[] args)
        {
            cmdArgs = args;
            InitializationSucceeded = true;
            if (!allNecessaryArgsArePresent())
            {
                Logger.Error($"Not all necessary arguments are present!");
                InitializationSucceeded = false;
            }
            else
            {
                try
                {
                    for (var i = 0; i < cmdArgs.Length; i++)
                        argActions(i);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    InitializationSucceeded = false;
                }
            }
            
        }

        private bool allNecessaryArgsArePresent()
        {
            var noDoubleArgsPresent = true;
            
            foreach (var arg in cmdArgs)
            {
                if (arg[0] == '-')
                {
                    if (foundArgs.ContainsKey(arg))
                    {
                        if (!foundArgs[arg])
                            foundArgs[arg] = true;
                        else
                        {
                            Logger.Error($"Multiple occurances found for \"{arg}\"");
                            noDoubleArgsPresent = false;
                        }
                    }
                    else
                    {
                        Logger.Warn($"Unknown argument \"{arg}\"");
                    }
                }
            }

            var allNecessaryArgsPresent = true;
            foreach (var arg in foundArgs)
            {
                allNecessaryArgsPresent = allNecessaryArgsPresent && arg.Value;
            }
            
            return noDoubleArgsPresent && allNecessaryArgsPresent;
        }

        private int argActions(int i)
        {
            if (!isArgument(i))
                return i;
            
            switch (cmdArgs[i])
            {
                case "-a":
                    LoadedAssembly = Assembly.Load(cmdArgs[i + 1]);
                    return i + 1;
                case "-args":
                    readParameterArgs(i+1);
                    return i + Parameters.Count;
                case "-argfile":
                    checkArgfilePath(i + 1);
                    return i + 1;
                case "-c":
                    readExpectedNumberOfClients(i + 1);
                    return i + 1;
                default:
                    Logger.Warn($"Uknown argument \"{cmdArgs[i]}\". Skipping...");
                    return i;
            }
        }

        private void readExpectedNumberOfClients(int i)
        {
            if(int.TryParse(cmdArgs[i], out var n))
                
        }

        private void checkArgfilePath(int i)
        {
            if(i >= cmdArgs.Length || isArgument(i))
                throw new Exception("No file path provided after -argfile!");
            if(!File.Exists(cmdArgs[i]))
                throw new Exception($"File \"{cmdArgs[i]}\" does not exist.");

            ArgfileLocation = cmdArgs[i];
        }

        private bool isArgument(int i)
        {
            return cmdArgs[i].First() == '-';
        }

        private void readParameterArgs(int firstArg)
        {
            Parameters = new List<string>();
            var errorsOccurred = false;
            
            for (var i = firstArg; cmdArgs[i][0] != '-' && i < cmdArgs.Length; i++)
            {
                var argLength = cmdArgs[i].Length;
                if (argLength <= 2 || cmdArgs[i].First() != '[' || cmdArgs[i].Last() != ']')
                {
                    Logger.Error($"Error in parameter {i - firstArg + 1} after -args! \"{cmdArgs[i]}\" is not a valid parameter!");
                    errorsOccurred = true;
                }
                else
                {
                    var buffer = new char[cmdArgs[i].Length - 2];
                    cmdArgs[i].CopyTo(1, buffer, 0, argLength);
                    Parameters.Add(new string(buffer));
                }
            }

            if (errorsOccurred)
                throw new Exception($"Errors occurred while parsing parameters");
            if(Parameters.Count < 1)
                throw new Exception($"No parameters provided after -args");
        }
    }
}