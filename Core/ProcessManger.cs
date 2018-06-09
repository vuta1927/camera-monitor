
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using ProcessMonitor.Models;

namespace ProcessMonitor.Core
{
    public class ProcessManager
    {
        private static List<MProcess> _processes = new List<MProcess>();
        private static string filePath;
        public ProcessManager(string fileName)
        {
            var rootPath = Directory.GetCurrentDirectory();
            filePath = rootPath + "/" + fileName;

            if (!File.Exists(filePath))
            {
                File.CreateText(filePath).Close();
            }
        }

        public List<MProcess> GetAll()
        {
            return _processes;
        }
        public void RunAll()
        {
            using (var f = new StreamReader(filePath))
            {
                var json = f.ReadToEnd();
                _processes = JsonConvert.DeserializeObject<List<MProcess>>(json);
            }

            if (!_processes.Any()) return;

            foreach (var p in _processes)
            {
                p.Process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = p.Application,
                        Arguments = String.Format(p.Agurment),
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                try
                {
                    p.Process.Start();
                    p.Process.OutputDataReceived += OutputHandler;
                    p.Process.ErrorDataReceived += ErrortHandler;
                    p.Process.BeginOutputReadLine();
                    p.Process.BeginErrorReadLine();
                    p.Process.WaitForExit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        public void Run(int id)
        {
            foreach (var p in _processes)
            {
                if (p.Id == id)
                {
                    try
                    {
                        p.Process.Start();
                        p.Process.OutputDataReceived += OutputHandler;
                        p.Process.ErrorDataReceived += ErrortHandler;
                        p.Process.BeginOutputReadLine();
                        p.Process.BeginErrorReadLine();
                        p.Process.WaitForExit();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
        }
        public void EndAll()
        {
            if (_processes.Any())
            {
                foreach (var p in _processes)
                {
                    try
                    {
                        p.Process.Kill();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }
        public int Add(string fileName, string arguments)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,//"/bin/bash",
                    Arguments = String.Format(arguments),//$"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            var newProcess = new MProcess()
            {
                Id = _processes.Count + 1,
                Agurment = arguments,
                Process = process
            };

            _processes.Add(newProcess);

            using (StreamWriter file = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, _processes);
            }

            return newProcess.Id;
        }

        public void Stop(int id)
        {
            foreach (var p in _processes)
            {
                if (p.Id == id)
                {
                    try
                    {
                        p.Process.Kill();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
        }

        public void Remove(int id)
        {
            foreach (var p in _processes)
            {
                if (p.Id == id)
                {
                    try
                    {
                        p.Process.Kill();
                        _processes.Remove(p);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    using (StreamWriter file = File.CreateText(filePath))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        //serialize object directly into file stream
                        serializer.Serialize(file, _processes);
                    }
                }
            }
        }

        private static void OutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                Console.WriteLine(outLine.Data);
            }
        }

        private static void ErrortHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                Console.WriteLine(outLine.Data);
            }
        }
    }
}
