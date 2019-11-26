﻿using System;
using System.Collections.Generic;
using System.Media;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace runapp
{
    class Main
    {
        Dictionary<string, string> launchArgs = new Dictionary<string, string>();

        string quote(string arg) {
            if(arg.StartsWith("\"")) return arg;
            return "\"" + arg + "\"";
        }

        string getKey(string key, string fallback)
        {
            return launchArgs.ContainsKey(key) ? launchArgs[key] : fallback;
        }

        string findOne(string[] arr, string key, string defaultVal)
        {
            if (key == null) return defaultVal;
            string one = arr.FirstOrDefault(s => s.StartsWith(key, StringComparison.CurrentCultureIgnoreCase));
            return one == null ? defaultVal : one;
        }

        bool isFalsy(string str)
        {
            return str == "no" || str == "0" || str == "false";
        }

        public Main()
        {
            Application.UseWaitCursor = false;

            string[] cmdArgs = Environment.GetCommandLineArgs();
            string fileName = Process.GetCurrentProcess().MainModule.FileName;
            //MessageBox.Show(Application.StartupPath + " " + Path.GetDirectoryName(fileName));
            string runDir = Path.GetDirectoryName(fileName);
            string runName = Path.GetFileNameWithoutExtension(fileName);
            string runConfig = Path.Combine(runDir, runName + ".arg");

            string configFile = cmdArgs.Length < 2
                ? File.Exists(runConfig) ? runConfig : "config.arg"
                : cmdArgs[1];

            string[] configArgs = null;
            int argIndex = 0;
            try
            {
                using (StreamReader r = new StreamReader(configFile))
                {
                    string content = r.ReadToEnd();
                    content = Regex.Replace(content, @"/\*(.*?)\*/", string.Empty, RegexOptions.Singleline);
                    configArgs = content.Split(new char[]{'\n'}, StringSplitOptions.None);
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                return;
            }

            if (configArgs == null || configArgs.Length<1) return;

            configArgs = configArgs
                .Select(arg => arg.Trim('\r', '\n'))
                .Where(arg => !String.IsNullOrEmpty(arg.Trim()) && !arg.StartsWith("//"))
                .ToArray();

            for (argIndex = 0; argIndex < configArgs.Length; argIndex++)
            {
                // :dir: c:\widnows
                // :show: 1
                string param = configArgs[argIndex];
                if (!param.StartsWith(":")) break;
                string[] arr = param.Split(new char[] { ':' }, 3);
                if (arr.Length >= 3)
                {
                    launchArgs.Add(arr[1], arr[2].TrimStart(' '));
                }
            }

            string exePath = configArgs[argIndex++];
            if(!Path.IsPathRooted(exePath) || exePath.StartsWith(".")) exePath = Path.Combine(runDir, exePath);

            if (!File.Exists(exePath))
            {
                //MessageBox.Show("Exe file not found!");
                //System.Environment.Exit(1);
                //return;
            }

            StringBuilder output = new StringBuilder();

            StringBuilder args = new StringBuilder();

            //MessageBox.Show("" + String.Join(",", configArgs) + configArgs[0]);
            for (int i = argIndex; i < configArgs.Length; i++)
            {
                args.Append(quote(configArgs[i]) + " ");
            }

            //MessageBox.Show(args.ToString());
            runExe(args.ToString(), exePath, getKey("shell", null), getKey("window", null), getKey("style", null), getKey("dir", runDir), null, null);
        }


        Process runExe(string arg, string exePath, string strShell, string strWindow, string strShow, string workingDir, Action<object, EventArgs> onExit, StringBuilder outputBuilder)
        {

            Process myProcess = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo();

            bool useShell = strShell !=null ? !isFalsy(strShell)
                : true;

            startInfo.UseShellExecute = useShell;

            startInfo.FileName = exePath;

            string style = findOne(new string[] { "Normal", "Minimized", "Maximized", "Hidden", "Hide" }, strShow, "Normal");
            //MessageBox.Show(style);
            bool isHide = style == "Hidden" || style == "Hide";
            startInfo.WindowStyle = isHide ? ProcessWindowStyle.Hidden
                : style == "Minimized" ? ProcessWindowStyle.Minimized
                : style == "Maximized" ? ProcessWindowStyle.Maximized
                : ProcessWindowStyle.Normal;

            // when UseShellExecute = true will be ignored
            startInfo.CreateNoWindow = isHide || isFalsy(strWindow);

            //MessageBox.Show("" +workingDir+ startInfo.WindowStyle + useShell+ isHide + isFalsy(strWindow));

            if (workingDir != null) startInfo.WorkingDirectory = workingDir;
            startInfo.Arguments = arg;

            if (outputBuilder != null)
            {
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                myProcess.OutputDataReceived += (sender, eventArgs) => outputBuilder.AppendLine(eventArgs.Data);
                myProcess.ErrorDataReceived += (sender, eventArgs) => outputBuilder.AppendLine(eventArgs.Data);
            }


            if (onExit != null)
            {
                myProcess.EnableRaisingEvents = true;
                myProcess.Exited += new EventHandler(onExit);
            }

            myProcess.StartInfo = startInfo;
            try
            {
                myProcess.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }

            if (outputBuilder != null)
            {
                myProcess.BeginOutputReadLine();
                myProcess.BeginErrorReadLine();
            }

            return myProcess;
        }

    }
}
