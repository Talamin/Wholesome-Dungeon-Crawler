using WholesomeDungeonCrawler.Models;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    internal class JumpToStepStep : Step
    {
        private JumpToStepModel _jumpToStepModel;
        private string _stepToJumpTo;
        private IProfile _profile;

        public override string Name { get; }

        public JumpToStepStep(JumpToStepModel jumpToStepModel, IProfile profile) : base(jumpToStepModel.CompleteCondition)
        {
            _jumpToStepModel = jumpToStepModel;
            _stepToJumpTo = _jumpToStepModel.StepToJumpTo;
            _profile = profile;
            Name = _jumpToStepModel.Name;
        }

        public override void Initialize() { }

        public override void Dispose() { }

        public override void Run()
        {
            if (EvaluateCompleteCondition())
            {
                _profile.JumpToStep(Name, _stepToJumpTo);
            }
            IsCompleted = true;
        }
    }
}
