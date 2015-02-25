using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace AnticrastinateCore
{
    class ProgramRuleEnforcer
    {
        private const string RootKeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";
        private const string BlockingKeyName = "Debugger";
        // TODO: obtain correct path to program blocker executable.
        private const string BlockingKeyValue = @"C:\ProgramBlocker.exe";

        public void Enforce(RuleSet newRuleSet, RuleSet oldRuleSet)
        {
            // compute a rule deltas.
            // unblock programs that are only in the old RuleSet
            foreach (var program in oldRuleSet.BlockedPrograms.Except(newRuleSet.BlockedPrograms))
                Unblock(program);
           
            // block programs that are only in the new RuleSet
            foreach (var program in newRuleSet.BlockedPrograms.Except(oldRuleSet.BlockedPrograms))
                Block(program);
        }

        private void Block(ProgramRule program)
        {
            var baseKey = Registry.LocalMachine.OpenSubKey(RootKeyPath, true);

            // will open sub-key if it already exists
            var appKey = baseKey.CreateSubKey(program.Name);
            appKey.SetValue(BlockingKeyName, BlockingKeyValue);
        }

        private void Unblock(ProgramRule program)
        {
            var baseKey = Registry.LocalMachine.OpenSubKey(RootKeyPath, true);

            // try to open subkey, null if it doesn't exist (so we don't need to do anything).
            var appKey = baseKey.OpenSubKey(program.Name, true);
            if (appKey != null)
            {
                // no need to throw if the value doesn't exist, this is good news.
                appKey.DeleteValue(BlockingKeyName, throwOnMissingValue: false);
            }
            
        }

    }
}
