using System.Linq;
using Microsoft.Win32;

namespace AnticrastinateCore
{
    /// <summary>
    /// Enforces program RuleSets, using the registry to block programs,
    /// by launching our specified executable as a 'debugger' instead.
    /// </summary>
    class ProgramRuleEnforcer
    {
        private const string RootKeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";
        private const string BlockingKeyName = "Debugger";
        // TODO: obtain correct path to program blocker executable.
        private const string BlockingKeyValue = @"C:\ProgramBlocker.exe";

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
                if (ruleSet != value)
                {
                    // unblock programs that are only in the old RuleSet
                    foreach (var program in ruleSet.BlockedPrograms.Except(value.BlockedPrograms))
                        Unblock(program);

                    // block programs that are only in the new RuleSet
                    foreach (var program in value.BlockedPrograms.Except(ruleSet.BlockedPrograms))
                        Block(program);

                    ruleSet = value;
                }
            }
        }

        /// <summary>
        /// Blocks the specified program.
        /// </summary>
        /// <param name="program">The program rule.</param>
        private void Block(ProgramRule program)
        {
            var baseKey = Registry.LocalMachine.OpenSubKey(RootKeyPath, true);

            // will open sub-key if it already exists
            var programKey = baseKey.CreateSubKey(program.Name);
            programKey.SetValue(BlockingKeyName, BlockingKeyValue);

            // TODO: handle failures cases -
            // Image File Exceution Options key couldn't be opened,
            // or program key couldn't be created.
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
