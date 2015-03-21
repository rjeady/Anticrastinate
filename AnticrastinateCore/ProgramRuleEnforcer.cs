using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Win32;

namespace AnticrastinateCore
{
    /// <summary>
    /// Enforces program RuleSets, using the registry to block programs,
    /// by launching our specified executable as a 'debugger' instead.
    /// </summary>
    internal class ProgramRuleEnforcer
    {
        private const string RootKeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";
        private const string BlockingKeyName = "Debugger";
        // TODO: obtain correct path to program blocker executable.
        private const string BlockingKeyValue = @"C:\ProgramBlocker.exe";

        private ProcessCloser processCloser;
        
        private RuleSet ruleSet;

        public ProgramRuleEnforcer(RuleSet existingRuleSet)
        {
            ruleSet = existingRuleSet;
        }

        public RuleSet RuleSet
        {
            get { return ruleSet; }
            set
            {
                if (ruleSet == value)
                    return;

                processCloser.Abort();

                // unblock programs that are only blocked in the old RuleSet
                foreach (var program in ruleSet.BlockedPrograms.Except(value.BlockedPrograms))
                    Unblock(program);

                // block programs that are only blocked in the new RuleSet
                List<ProgramRule> newlyBlockedPrograms = value.BlockedPrograms.Except(ruleSet.BlockedPrograms).ToList();
                
                foreach (var program in newlyBlockedPrograms)
                    BlockInRegistry(program);

                processCloser = new ProcessCloser(newlyBlockedPrograms);

                ruleSet = value;
            }
        }

        /// <summary>
        /// Blocks the specified program in the registry.
        /// </summary>
        /// <param name="program">The program rule.</param>
        private void BlockInRegistry(ProgramRule program)
        {
            using (RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(RootKeyPath, true))
            using (RegistryKey programKey = baseKey.CreateSubKey(program.Name))
            {
                // this will have opened the program sub-key instead if it already exists
                programKey.SetValue(BlockingKeyName, BlockingKeyValue);
            }

            // TODO: handle failures cases:
            // - Image File Exceution Options key couldn't be opened
            // - program key couldn't be created
        }

        /// <summary>
        /// Unblocks the specified program in the registry.
        /// </summary>
        /// <param name="program">The program rule.</param>
        private void Unblock(ProgramRule program)
        {
            var baseKey = Registry.LocalMachine.OpenSubKey(RootKeyPath, true);

            // try to open subkey, null if it doesn't exist (so we don't need to do anything).
            var programKey = baseKey.OpenSubKey(program.Name, true);
            if (programKey != null)
            {
                // no need to throw if the value doesn't exist, this is good news.
                programKey.DeleteValue(BlockingKeyName, throwOnMissingValue: false);
            }

            // TODO: handle failures case -
            // Image File Exceution Options key couldn't be opened.
        }


        private class ProcessCloser
        {
            // time delay between asking user to close programs and asking the processes to close ourselves.
            private const int CloseProcessesDelayMs = 20 * 1000;
            // time delay between asking the processes to close and killing them.
            private const int KillProcessesDelayMs = 20 * 1000;

            private Timer closeProcessesTimer;
            private Timer killProcessesTimer;
            private volatile bool abort;

            private readonly List<Process> processes = new List<Process>(); 

            public ProcessCloser(IEnumerable<ProgramRule> newlyBlockedPrograms)
            {
                AskUserToClose(newlyBlockedPrograms);
            }

            public void Abort()
            {
                abort = true;
                // it is possible that the AskUserToClose or CloseProcesses methods could instantiate these timers
                // after we try to dispose of them, but they will be disposed of when the timer callback occurs anyway.
                if (closeProcessesTimer != null)
                    closeProcessesTimer.Dispose();
                if (killProcessesTimer != null)
                    killProcessesTimer.Dispose();
            }

            private void AskUserToClose(IEnumerable<ProgramRule> programRules)
            {
                var processesToClose = new List<Process>();

                foreach (var program in programRules)
                {
                    Process[] procs = Process.GetProcessesByName(program.NameWithoutExtension);
                    if (procs.Length > 0)
                    {
                        // TODO: ask user to close this program.
                        processesToClose.AddRange(procs);
                    }
                }
                // schedule the CloseProcesses method giving the user some time to close the programs themselves.
                if (!abort)
                    closeProcessesTimer = new Timer(CloseProcesses, processesToClose, CloseProcessesDelayMs, Timeout.Infinite);
            }
            
            private void CloseProcesses(object state)
            {
                closeProcessesTimer.Dispose();

                if (abort)
                    return;

                for (int i = 0; i < processes.Count; i++)
                {
                    try
                    {
                        // ask it to close nicely
                        processes[i].CloseMainWindow();
                    }
                    catch (InvalidOperationException)
                    {
                        // process has already closed. Dispose the process object and mark it for removal from our list.
                        processes[i].Dispose();
                        processes[i] = null;
                    }
                }

                // remove disposed processes from the kill list all at once.
                // this is more efficient than deleting the processes one at a time from the list.
                processes.RemoveAll(x => x == null);

                if (!abort)
                    killProcessesTimer = new Timer(KillProcesses, processes, KillProcessesDelayMs, Timeout.Infinite);
            }

            private void KillProcesses(object state)
            {
                killProcessesTimer.Dispose();

                if (abort)
                    return;

                foreach (Process process in processes)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (InvalidOperationException)
                    {
                        // process has already exited, so do nothing.
                    }
                    catch (Win32Exception)
                    {
                        // - process is already terminating (good),
                        // - or process is a win16 exe / could not be terminated (nothing we can do).
                    }
                    // no need to catch NotSupportedException, since we won't be trying to kill remote processes.
                    finally
                    {
                        // Dispose the process object.
                        process.Dispose();
                    }
                }
            }
        }
    }
}