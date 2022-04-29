using robotManager.Helpful;
using System.Collections.Generic;
using WholesomeDungeonCrawler.Helpers;

namespace WholesomeDungeonCrawler.Dungeonlogic
{
    public class Profile
    {
        private int _currentStep;
        private int _totalSteps;
        private LogicRunner _logicRunner;


        public Step[] Steps { get; set; }
        public Dungeon Dungeon { get; set; }

        internal Profile( LogicRunner logicRunner, string profileName = "Unnamed")
        {
            Name = profileName;
            _logicRunner = logicRunner;
        }

        public string Name { get; set; }
        public string CurrentState { get; private set; } = "Idle profile";

        public virtual bool OverrideNeedToRun => GetCurrentStep()?.OverrideNeedToRun ?? false;

        protected virtual Step[] GetSteps() => new Step[0];

        public Step GetCurrentStep() => _currentStep < _totalSteps ? Steps?[_currentStep] : null;

        public bool IsFinished() => _currentStep >= _totalSteps;

        public void Load()
        {
            _logicRunner.CheckUpdate(this);
        }

        public void Reset()
        {
            Steps = GetSteps();
            _currentStep = 0;
            _totalSteps = Steps.Length;
        }

        protected virtual void UpdateSteps() { }

        public bool Pulse()
        {
            if (IsFinished()) return true;

            UpdateSteps();

            Step step = Steps[_currentStep];

            if (!step.IsCompleted)
            {
                CurrentState = $"Executing step {step.Name} ({_currentStep + 1}/{_totalSteps}).";
                if (step.Pulse()) _currentStep++;
            }
            else
            {
                Logging.WriteDebug($"[LogicRunner] Skipping step {step.Name} ({_currentStep + 1}/{_totalSteps}).");
                _currentStep++;
            }

            return IsFinished();
        }
    }
}
