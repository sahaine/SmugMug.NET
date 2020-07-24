﻿// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using SmugMug.Shared.Descriptors;
using SmugMugShared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SmugMugCodeGen
{
    class Program
    {
        private static Options _options;

        private static void PrintUsage()
        {
            ConsolePrinter.Write(ConsoleColor.White, "Usage:");
            ConsolePrinter.Write(ConsoleColor.Cyan, "smugmugcodegen.exe <outputFolder> [GenerateManualFiles] [<metadataFiles>] ");
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            // The usage of this tool is going to be:
            // smugmugcodegen.exe <outputFolder> <GenerateManualFiles> [<metadataFiles>] 
            if (args.Length < 2)
            {
                PrintUsage();
                Environment.Exit(-1);
            }

            //get the output directory.
            _options = new Options(args);

            Dictionary<string, Entity> metadata = LoadMetadataFromFile(_options.InputFiles);

            // Make sure the output directories exist
            if (Helpers.TryCreateDirectory(_options.OutputDir) == false)
                Environment.Exit(-2);
            if (Helpers.TryCreateDirectory(_options.OutputDirEnums) == false)
                Environment.Exit(-2);

            CodeGen cg = new CodeGen(metadata);

            WriteClasses(metadata);

            WriteEnums();
#if DEBUG
            Console.WriteLine("Complete");
            Console.ReadKey();
#endif 

        }

        private static void WriteEnums()
        {
            Dictionary<string, string> enumTypeDefs = CodeGen.BuildEnums();

            foreach (var item in enumTypeDefs)
            {
                File.WriteAllText(Path.Combine(_options.OutputDirEnums, item.Key + "Enum.cs"), item.Value);
                ConsolePrinter.Write(ConsoleColor.Green, "Generated enum {0}", item.Key);
            }
            ConsolePrinter.Write(ConsoleColor.White, "Generated {0} enums", enumTypeDefs.Count);
        }

        private static void WriteClasses(Dictionary<string, Entity> metadata)
        {
            foreach (var item in metadata)
            {
                ConsolePrinter.Write(ConsoleColor.Green, "Generating class {0}", item.Key);

                StringBuilder additionalMethodUsings = new StringBuilder();

                additionalMethodUsings.Append("using System.Threading.Tasks;");

                string className = Helpers.NormalizeString(item.Value.Name);

                StringBuilder properties = CodeGen.BuildProperties(item.Value.Properties.OrderBy(p => p.Name), item.Value);

                StringBuilder methods = new StringBuilder();
                methods.AppendLine(string.Format(Constants.ConstructorDefinition, className));

                string parameters = CodeGen.BuildMethodReturningParametersToSendInRequest(item.Value);
                if (!string.IsNullOrEmpty(parameters))
                {
                    methods.AppendLine(parameters);
                    additionalMethodUsings.AppendLine();
                    additionalMethodUsings.Append("using System.Collections.Generic;");
                }
                methods.Append(CodeGen.BuildMethods(item.Value.Methods));

                string objectDirName = Path.Combine(_options.OutputDir, className);
                Directory.CreateDirectory(objectDirName);

                StringBuilder sb = new StringBuilder();
                string classDefinition = GetClassDefinition(className, properties.ToString().TrimEnd(), string.Empty, string.Empty);
                sb.Append(classDefinition);
                File.WriteAllText(Path.Combine(objectDirName, item.Key + ".properties.cs"), sb.ToString());

                sb = new StringBuilder();
                classDefinition = GetClassDefinition(className, methods.ToString().TrimEnd(), string.Empty, additionalMethodUsings.ToString());
                sb.Append(classDefinition);
                File.WriteAllText(Path.Combine(objectDirName, item.Key + ".methods.cs"), sb.ToString());

                if (_options.GenerateManualFiles)
                {
                    // These should only be used once, as they are meant to be changed while the API is taking shape
                    sb = new StringBuilder();
                    StringBuilder manualMethods = new StringBuilder();
                    manualMethods.Append(CodeGen.BuildManualMethods(item.Value.Methods));
                    classDefinition = GetClassDefinition(className, manualMethods.ToString().TrimEnd(), string.Empty, "using System.Threading.Tasks;");
                    sb.Append(classDefinition);
                    File.WriteAllText(Path.Combine(objectDirName, item.Key + ".manual.cs"), sb.ToString());
                }
            }
            ConsolePrinter.Write(ConsoleColor.White, "Generated {0} classes", metadata.Count);
        }

        private static string GetClassDefinition(string className, string members, string obsolete, string usings)
        {
            string additionalUsings = string.IsNullOrEmpty(usings) ? string.Empty : (Environment.NewLine + usings);
            return string.Format(Constants.ClassDefinition, className, members, obsolete, additionalUsings);
        }

        private static Dictionary<string, Entity> LoadMetadataFromFile(string[] files)
        {
            Dictionary<string, Entity> data = null;

            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    ConsolePrinter.Write(ConsoleColor.Yellow, "Cannot find file {0}. Skipping...", file);
                    continue;
                }

                try
                {
                    var jsonSerSettings = new JsonSerializerSettings();
                    jsonSerSettings.TypeNameHandling = TypeNameHandling.All;
                    Newtonsoft.Json.JsonSerializer jsonSer = Newtonsoft.Json.JsonSerializer.CreateDefault(jsonSerSettings);

                    using (StreamReader sr = new StreamReader(file))
                    using (JsonReader jr = new JsonTextReader(sr))
                    {
                        ConsolePrinter.Write(ConsoleColor.White, "Loading metadata from file {0}", file);
                        var currentData = jsonSer.Deserialize<Dictionary<string, Entity>>(jr);

                        // If this is the first file, nothing to merge.
                        if (data == null)
                        {
                            data = currentData;
                            continue;
                        }

                        // we need to merge the 2 entities.
                        MergeMetadataInfo(data, currentData);
                    }
                }
                catch (Exception e)
                {
                    ConsolePrinter.Write(ConsoleColor.Red, "Error processing file {0} ({1}). Skipping.", file, e.Message);
                }
            }

            return data;
        }

        private static void MergeMetadataInfo(Dictionary<string, Entity> current, Dictionary<string, Entity> other)
        {
            foreach (var key in other.Keys)
            {
                if (current.ContainsKey(key))
                {
                    // We need to merge the 2 
                    var currentEntity = current[key];
                    var otherEntity = other[key];

                    currentEntity.MergeWith(otherEntity);
                }
                else
                {
                    // this is a new thing in other, and not in current. 
                    // Add it to current.
                    current.Add(key, other[key]);
                }
            }
        }
    }
}
