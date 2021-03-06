﻿/* 
Copyright (c) 2014, Gnomesoft, LLC.
All rights reserved.
 
Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:
  
 * Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
   
 * Redistributions in binary form must reproduce the above copyright notice, this
   list of conditions and the following disclaimer in the documentation and/or
   other materials provided with the distribution.
  
 * Neither the name of the Gnomesoft, LLC. nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Management;

namespace WmiCrawler
{
    /// <summary>
    /// WMI Crawler is a simple command line application that crawls for WMI Namespaces, 
    /// Classes, and Class Properties on the current system. It is rather simplistic in 
    /// its output and is easy to parse the output to find a namespace or a class you 
    /// are looking for by piping the output to another command or redirecting the 
    /// output to a text file.
    /// </summary>
    /// <remarks>
    /// https://github.com/Gnomesoft/WmiCrawler
    /// </remarks>
    class Program
    {
        private const string NameSpaceFormatter = @"{0}\{1}";
        private static bool _showClasses;
        private static bool _showProperties;
        private static bool _showPropQualifiers;

        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                string command = arg.ToLower().Trim();

                switch (command)
                {
                    case "-?":
                    case "/?":
                    case "-help":
                    case "--help":
                    case "/help":
                    case "help":
                    {
                        PrintHelp();
                        break;
                    }
                    case "-sc":
                    case "--sc":
                    case "/sc":
                    case "--show-classes":
                    case "-show-classes":
                    case "/show-classes":
                    {
                        _showClasses = true;
                        break;
                    }
                    case "-sp":
                    case "--sp":
                    case "/sp":
                    case "--show-properties":
                    case "-show-properties":
                    case "/show-properties":
                    {
                        _showProperties = true;
                        break;
                    }
                    case "-sq":
                    case "--sq":
                    case "/sq":
                    case "--show-property-qualifiers":
                    case "-show-property-qualifiers":
                    case "/show-property-qualifiers":
                    {
                        _showPropQualifiers = true;
                        break;
                    }
                    default:
                    {
                        PrintHelp();
                        break;
                    }
                }
            }

            PrintAllNamespacesAndClasses();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("WMI Crawler");
            Console.WriteLine();
            Console.WriteLine("wmicrawler [options]");
            Console.WriteLine();
            Console.WriteLine("Crawls only the namespaces if no options are provided.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine();
            Console.WriteLine("  -sc or --show-classes                Crawls the classes within each namespace.");
            Console.WriteLine("  -sp or --show-properties             Crawls the properties for each class, works only with --show-classes.");
            Console.WriteLine("  -sq or --show-property-qualifiers    Crawls the qualifiers for each property, works only with --show-properties.");

            System.Environment.Exit(0);
        }

        private static void PrintAllNamespacesAndClasses()
        {
            ManagementClass nsClass = new ManagementClass(new ManagementScope("root"), new ManagementPath("__namespace"), null);
            foreach (var ns in nsClass.GetInstances())
            {
                string childNs = ns["name"].ToString();
                PrintChildNamespaces(ns as ManagementObject);
            }
        }

        private static void PrintChildNamespaces(ManagementObject ns)
        {
            string currentNamespace = string.Format(NameSpaceFormatter, ns.Path.NamespacePath, ns["name"]);
            Console.WriteLine(currentNamespace);
            string newBaseNs = string.Format(NameSpaceFormatter, ns.Path.NamespacePath, ns["name"]);
            try
            {
                ManagementClass nsClass = new ManagementClass(new ManagementScope(newBaseNs), new ManagementPath("__namespace"), null);
                // GetInstances throws on ACCESS DENIED
                var children = nsClass.GetInstances();
                // Only gets to here if access to namespace is allowed
                PrintClasses(currentNamespace);
                foreach (var childNs in nsClass.GetInstances())
                {
                    PrintChildNamespaces(childNs as ManagementObject);
                }
            }
            catch (ManagementException ex)
            {
                Console.WriteLine("\tNamespace Error: [{0}]", ex.Message.Trim().ToUpper());
            }
        }

        private static void PrintClasses(string ns)
        {
            if (!_showClasses) return;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(new ManagementScope(ns), new WqlObjectQuery("select * from meta_class"), null);
                foreach (var wmiClass in searcher.Get())
                {
                    Console.WriteLine("\tCLASS: {0}", wmiClass["__CLASS"]);
                    PrintClassMetaData(wmiClass as ManagementObject);
                }
            }
            catch (ManagementException ex)
            {
                Console.WriteLine("\tClass Error: [{0}]", ex.Message.Trim().ToUpper());
            }
        }

        private static void PrintClassMetaData(ManagementObject wmiClass)
        {
            if (!_showProperties) return;
            var properties = wmiClass.Properties;

            foreach (var prop in properties)
            {
                Console.WriteLine("\t\tPROPERTY: Name[{3}] Type[{0}] IsArray[{1}] IsLocal[{2}] Origin[{4}] ", prop.Type, prop.IsArray, prop.IsLocal, prop.Name, prop.Origin);
                PrintPropertyQualifiers(prop.Qualifiers);
            }

        }

        private static void PrintPropertyQualifiers(QualifierDataCollection qualifiers)
        {
            if (!_showPropQualifiers) return;
            foreach (var qual in qualifiers)
            {
                Console.WriteLine("\t\t\tQUALIFIER: Name[{3}]  IsAmended[{0}] IsLocal[{1}] IsOverridable[{2}] PropagatesToInstance[{4}] PropagatesToSubclass[{5}]", qual.IsAmended, qual.IsLocal, qual.IsOverridable, qual.Name, qual.PropagatesToInstance, qual.PropagatesToSubclass);
                Console.WriteLine("\t\t\t    VALUE: {0}", qual.Value);
            }
        }

    }
}
