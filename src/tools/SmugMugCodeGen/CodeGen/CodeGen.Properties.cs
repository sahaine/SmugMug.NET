﻿// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SmugMug.Shared.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmugMugCodeGen
{
    public partial class CodeGen
    {
        private static string GetPropertyType(Property prop)
        {
            if (prop is BooleanProperty)
                return "bool";
            if (prop is DecimalProperty)
                return "decimal";
            if (prop is FloatProperty)
                return "float";
            if (prop is IntegerProperty)
                return "long";
            if (prop is ArrayProperty)
                return GetArrayPropertyType(prop as ArrayProperty);
            if (prop is SelectProperty)
                return GetSelectPropertyType(prop as SelectProperty);
            if (prop is DateTimeProperty || prop is TimeStampProperty)
                return "DateTime";
            if (prop is StringProperty || prop is UriProperty || prop is HashProperty)
                return "string";

            if (prop is UnknownTypeProperty)
                return "string";

            throw new ArgumentException("Unknown property type");
        }

        private static string GetArrayPropertyType(ArrayProperty ap)
        {
            if (ap.ItemType == "Varchar")
                return "string[]";

            return ap.ItemType + "[]";
        }

        private static string GetSelectPropertyType(SelectProperty prop)
        {
            string typeName = GetEnumTypeName(prop);

            if (renameMap.ContainsKey(typeName))
                typeName = renameMap[typeName];

            return typeName + "Enum";
        }

        public static StringBuilder BuildProperties(IEnumerable<Property> list, Entity parentEntity)
        {
            StringBuilder propertyFields = new StringBuilder();
            StringBuilder propertyAccesors = new StringBuilder();

            foreach (var prop in list)
            {
                string propName = Helpers.NormalizeString(prop.Name);
                string propType = GetPropertyType(prop);

                // we are going to skip over FormattedValues
                if (StringComparer.OrdinalIgnoreCase.Equals(propName, "FormattedValues"))
                    propType = "CaptionEntity";

                // the properties are going to be generated into 2 parts:
                //  1. At the top, the field declaration
                //  2. At the bottom, the actual implementation

                string fieldName = "_" + char.ToLower(propName[0]) + propName.Substring(1);
                propertyFields.AppendLine(string.Format(Constants.PropertyFieldDefinition, propType, fieldName));

                if (!string.IsNullOrWhiteSpace(prop.Deprecated))
                {
                    propertyAccesors.AppendLine(string.Format(Constants.PropertyDefintionsObsolete, prop.Deprecated));
                }

                // if the property is part of the patch/post list, then it should be generated as keeping track of changes
                if (IsPatchOrPost(prop.Name, parentEntity))
                {
                    propertyAccesors.AppendLine(string.Format(Constants.PropertyDefinition, propType, propName, fieldName));
                }
                else 
                {
                    propertyAccesors.AppendLine(string.Format(Constants.PropertyDefinitionReadOnly, propType, propName, fieldName));
                }

            }

            propertyFields.AppendLine();

            propertyFields.AppendLine(propertyAccesors.ToString());

            return propertyFields;
        }

        private static bool IsPatchOrPost(string propertyName, Entity entity)
        {
            if (entity.HttpMethodsAndParameters.ContainsKey("patch"))
            {
                return entity.HttpMethodsAndParameters["patch"].Any(p => StringComparer.OrdinalIgnoreCase.Equals(p.Name, propertyName));
            }
            if (entity.HttpMethodsAndParameters.ContainsKey("post"))
            {
                return entity.HttpMethodsAndParameters["post"].Any(p => StringComparer.OrdinalIgnoreCase.Equals(p.Name, propertyName));
            }
            return false;
        }
    }
}
