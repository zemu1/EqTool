﻿using EQTool.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace EQTool
{
    public class Program
    {
        private const string configFile = "EQTool.exe.config";

        [STAThread]
        public static void Main()
        {
            try
            {
                _ = OnResolveAssembly(null, new ResolveEventArgs("System.Threading.Tasks.Extensions"));
                AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
                var debugging = false;
#if DEBUG
                debugging = true;
#endif
                if (!debugging)
                {
                    if (!File.Exists(configFile))
                    {
                        UpdateConfig();
                        Thread.Sleep(1000);
                        return;
                    }
                    else
                    {
                        var fileondisk = File.ReadAllText(configFile);
                        if (fileondisk != Resources.App)
                        {
                            UpdateConfig();
                            Thread.Sleep(1000);
                            return;
                        }
                    }
                }

                App.Main();
            }
            catch (Exception e)
            {
                File.WriteAllText("Errors", e.ToString());
            }
        }

        private static void UpdateConfig()
        {
            File.WriteAllText(configFile, Resources.App);
            var path = System.IO.Directory.GetCurrentDirectory() + "/EQTool.exe";
            _ = System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var assemblyName = new AssemblyName(args.Name);
            var path = assemblyName.Name + ".dll";

            using (var stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                {
                    return null;
                }

                var assemblyRawBytes = new byte[stream.Length];
                _ = stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                if (args.Name.StartsWith("System.Threading.Tasks.Extensions"))
                {
                    try
                    {
                        File.WriteAllBytes("System.Threading.Tasks.Extensions.dll", assemblyRawBytes);
                    }
                    catch { }
                }
                return Assembly.Load(assemblyRawBytes);
            }
        }
    }
}
