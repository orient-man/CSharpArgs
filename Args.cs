﻿using System;
using System.Collections.Generic;

namespace ConsoleApplication
{
    public class Args
    {
        private readonly Dictionary<Char, IArgumentMarshaler> marshalers;
        private readonly HashSet<Char> argsFound;
        private Iterator<String> currentArgument;

        public Args(String schema, String[] args)
        {
            marshalers = new Dictionary<char, IArgumentMarshaler>();
            argsFound = new HashSet<Char>();

            ParseSchema(schema);
            ParseArgumentStrings(args);
        }

        private void ParseSchema(String schema)
        {
            foreach (var element in schema.Split(','))
                if (element.Length > 0)
                    ParseSchemaElement(element.Trim());
        }

        private void ParseSchemaElement(String element)
        {
            var elementId = element[0];
            var elementTail = element.Substring(1);
            ValidateSchemaElementId(elementId);
            if (elementTail.Length == 0)
                marshalers.Add(elementId, new BooleanArgumentMarshaler());
            else if (elementTail == "*")
                marshalers.Add(elementId, new StringArgumentMarshaler());
            else if (elementTail == "#")
                marshalers.Add(elementId, new IntegerArgumentMarshaler());
            else if (elementTail == "##")
                marshalers.Add(elementId, new DoubleArgumentMarshaler());
            else if (elementTail == "[*]")
                marshalers.Add(elementId, new StringArrayArgumentMarshaler());
            else
                throw new ArgsException(
                    ErrorCode.InvalidArgumentFormat,
                    elementId,
                    elementTail);
        }

        private static void ValidateSchemaElementId(char elementId)
        {
            if (!Char.IsLetter(elementId))
                throw new ArgsException(ErrorCode.InvalidArgumentName, elementId, null);
        }

        private void ParseArgumentStrings(String[] argsList)
        {
            currentArgument = new Iterator<string>(argsList);
            while (currentArgument.HasNext())
            {
                var argString = currentArgument.Next();
                if (argString.StartsWith("-"))
                {
                    ParseArgumentCharacters(argString.Substring(1));
                }
                else
                {
                    currentArgument.Previous();
                    break;
                }
            }
        }

        private void ParseArgumentCharacters(String argChars)
        {
            for (var i = 0; i < argChars.Length; i++)
                ParseArgumentCharacter(argChars[i]);
        }

        private void ParseArgumentCharacter(char argChar)
        {
            if (!marshalers.ContainsKey(argChar))
            {
                throw new ArgsException(ErrorCode.UnexpectedArgument, argChar, null);
            }
            var m = marshalers[argChar];
            argsFound.Add(argChar);
            try
            {
                m.Set(currentArgument);
            }
            catch (ArgsException e)
            {
                e.ErrorArgumentId = argChar;
                throw;
            }
        }

        public bool Has(char arg)
        {
            return argsFound.Contains(arg);
        }

        public int NextArgument()
        {
            return currentArgument.NextIndex();
        }

        public bool GetBoolean(char arg)
        {
            return BooleanArgumentMarshaler.GetValue(marshalers[arg]);
        }

        public String GetString(char arg)
        {
            return StringArgumentMarshaler.GetValue(marshalers[arg]);
        }

        public int GetInt(char arg)
        {
            return IntegerArgumentMarshaler.GetValue(marshalers[arg]);
        }

        public double GetDouble(char arg)
        {
            return DoubleArgumentMarshaler.GetValue(marshalers[arg]);
        }

        public String[] GetStringArray(char arg)
        {
            return StringArrayArgumentMarshaler.GetValue(marshalers[arg]);
        }
    }
}