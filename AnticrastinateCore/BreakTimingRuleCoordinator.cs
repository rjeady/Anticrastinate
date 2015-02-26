using System;
using System.Runtime.Remoting.Messaging;
using System.Timers;
using AnticrastinateCore.Commons;

namespace AnticrastinateCore
{
    internal class BreakTimingRuleCoordinator
    {
        private readonly CountdownTimer timer;
        private bool breakTimeUp;
        private readonly ProgramRuleEnforcer programRuleEnforcer;
        private readonly WebsiteRuleEnforcer websiteRuleEnforcer;

        public BreakTimingRuleCoordinator(TimeSpan breakTime, RuleSet workRuleSet, RuleSet breakRuleSet,
            ProgramRuleEnforcer programRuleEnforcer, WebsiteRuleEnforcer websiteRuleEnforcer)
        {
            WorkRuleSet = workRuleSet;
            BreakRuleSet = breakRuleSet;
            this.programRuleEnforcer = programRuleEnforcer;
            this.websiteRuleEnforcer = websiteRuleEnforcer;
            
            timer = new CountdownTimer(breakTime.TotalMilliseconds);
            timer.Elapsed += HandleTimerElapsed;
        }

        public RuleSet WorkRuleSet { get; set; }
        public RuleSet BreakRuleSet { get; set; }

        public bool OnBreak { get; private set; }

        public TimeSpan RemainingBreakTime
        {
            get { return new TimeSpan(0, 0, 0, 0, breakTimeUp ? 0 : (int)timer.TimeRemaining); }
        }

        public bool BeginBreak()
        {
            if (breakTimeUp)
                return false;

            OnBreak = true;
            programRuleEnforcer.RuleSet = BreakRuleSet;
            websiteRuleEnforcer.RuleSet = BreakRuleSet;
            timer.Start();
            return true;
        }

        public void EndBreak()
        {
            OnBreak = false;
            programRuleEnforcer.RuleSet = WorkRuleSet;
            websiteRuleEnforcer.RuleSet = WorkRuleSet;

            timer.Pause();
        }

        private void HandleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            EndBreak();
            breakTimeUp = true;
        }
    }
}