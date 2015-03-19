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

        private const int CloseProgramTimeMs = 20 * 1000;
        private Timer killProgramsTimer;

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

                // unblock programs that are only blocked in the old RuleSet
                foreach (var program in ruleSet.BlockedPrograms.Except(value.BlockedPrograms))
                    Unblock(program);

                // block programs that are only blocked in the new RuleSet
                BlockPrograms(value.BlockedPrograms.Except(ruleSet.BlockedPrograms));

                ruleSet = value;
            }
        }

        private void BlockPrograms(IEnumerable<ProgramRule> newlyBlockedPrograms)
        {
            var processesToKill = new List<Process>();

            foreach (var program in newlyBlockedPrograms)
            {
                BlockInRegistry(program);

                Process[] procs = Process.GetProcessesByName(program.NameWithoutExtension);
                foreach (var proc in procs)
                {
                    try
                    {
                        // ask it to close nicely
                        proc.CloseMainWindow();
                        // may need to be killed
                        processesToKill.Add(proc);
                    }
                    catch (InvalidOperationException)
                    {
                        // process has already exited. Dispose the process object.
                        proc.Close();
                    }
                }
            }

            // give user a certain amount of time to close programs, then kill them.
            // first dipose of any existing timer - i.e. cancel any pending program kill operation
            if (killProgramsTimer != null)
                killProgramsTimer.Dispose();
            killProgramsTimer = new Timer(KillPrograms, processesToKill, CloseProgramTimeMs, Timeout.Infinite);
        }

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

        private void KillPrograms(object processes)
        {
            foreach (var process in (List<Process>)processes)
            {
                try
                {
                    if (!process.HasExited)
                        process.Kill();
                }
                catch (InvalidOperationException)
                {
                    // process has already exited, so do nothing.
                }
                catch (NotSupportedException)
                {
                    // we don't want to kill remote processes anyway.
                }
                catch (Win32Exception)
                {
                    // - process is already terminating (good),
                    // - or its exit code couldn't be retrieved in the HasExited call [huh? we didn't ask for the exit
                    //   code! Still hopefully that means it has exited/is exiting],
                    // - or process is a win16 exe / could not be terminated (nothing we can do).
                }
                finally
                {
                    // Dispose the process object.
                    process.Close();
                }
            }
        }

        /// <summary>
        /// Unblocks the specified program.
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
    }
}