﻿// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json.Linq;
using SmugMug.Shared.Extensions;
using SmugMugShared.Extensions;
using System.IO;

namespace SmugMug.Shared.Descriptors
{
    public class Property
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Deprecated { get; set; }

        public Property()
        {
            Name = Description = Deprecated = string.Empty;
        }

        public Property(string name)
        {
            Name = name;
        }

        public Property(JObject obj)
            : this()
        {
            if (obj == null) return;
            Name = obj.GetValueAsString("Name");
            Description = obj.GetValueAsString("Description");
            Deprecated = obj.GetValueAsString("Deprecated");
        }

        public static Property FromJObject(JObject item)
        {
            string type = item.GetValueAsString("Type");
            if (string.IsNullOrEmpty(type))
            {
                type = "varchar";
            }

            switch (type.ToLower())
            {
                case "select":
                    {
                        return new SelectProperty(item);
                    }
                case "email":
                case "varchar":
                case "text":
                    {
                        return new StringProperty(item);
                    }
                case "fullurl":
                case "uri":
                    {
                        return new UriProperty(item);
                    }
                case "array":
                    {
                        return new ArrayProperty(item);
                    }
                case "decimal":
                    {
                        return new DecimalProperty(item);
                    }
                case "time":
                case "date":
                case "datetime":
                case "datetimeiso8601":
                    {
                        return new DateTimeProperty(item);
                    }
                case "unixtimestamp":
                case "timestamp":
                    {
                        return new TimeStampProperty(item);
                    }
                case "boolean":
                    {
                        return new BooleanProperty(item);
                    }
                case "integer":
                    {
                        return new IntegerProperty(item);
                    }
                case "hash":
                    {
                        return new HashProperty(item);
                    }
                case "float":
                    {
                        return new FloatProperty(item);
                    }
                default:
                    {
                        ConsolePrinter.Write(System.ConsoleColor.Red, "Found unknown property type '{0}'. Assuming string", type);
                        return new StringProperty(item);
                    }

            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
