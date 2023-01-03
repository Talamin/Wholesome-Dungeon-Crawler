using WholesomeDungeonCrawler.Models;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    internal class JumpToStepStep : Step
    {
        private JumpToStepModel _jumpToStepModel;
        private string _stepToJumpTo;
        private IProfile _profile;

        public override string Name { get; }

        public JumpToStepStep(JumpToStepModel jumpToStepModel, IProfile profile)
        {
            _jumpToStepModel = jumpToStepModel;
            _stepToJumpTo = _jumpToStepModel.StepToJumpTo;
            _profile = profile;
            Name = _jumpToStepModel.Name;
        }

        public override void Run()
        {
            if (EvaluateCompleteCondition(_jumpToStepModel.CompleteCondition))
            {
                _profile.JumpToStep(Name, _stepToJumpTo);
            }
            IsCompleted = true;
        }
    }
}
