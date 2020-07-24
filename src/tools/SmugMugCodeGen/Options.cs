﻿// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace SmugMugCodeGen
{
    public class Options
    {
        public string OutputDir { get; private set; }
        public string OutputDirEnums { get; private set; }
        public bool GenerateManualFiles { get; private set; }

        public string[] InputFiles { get; private set; }

        public Options(string[] args)
        {
            OutputDir = args[0];
            OutputDirEnums = Path.Combine(OutputDir, @"..\Enums\");

            int index = 1;
            GenerateManualFiles = false;
            if (StringComparer.OrdinalIgnoreCase.Equals("generateManualFiles", args[1]))
            {
                GenerateManualFiles = true;
                index = 2;
            }

            // Copy the input files from the args array.
            InputFiles = new string[args.Length - index];
            Array.Copy(args, index, InputFiles, 0, args.Length - index);
        }
    }
}
