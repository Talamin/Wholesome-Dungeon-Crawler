using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using static wManager.Wow.Class.Npc;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    internal class JumpToStepStep : Step
    {
        private JumpToStepModel _jumpToStepModel;
        private string _stepToJumpTo;
        private IProfile _profile;

        public override string Name { get; }
        public override FactionType StepFaction { get; }
        public override LFGRoles StepRole { get; }

        public JumpToStepStep(JumpToStepModel jumpToStepModel, IProfile profile) : base(jumpToStepModel.CompleteCondition)
        {
            _jumpToStepModel = jumpToStepModel;
            _stepToJumpTo = _jumpToStepModel.StepToJumpTo;
            _profile = profile;
            Name = _jumpToStepModel.Name;
            StepFaction = _jumpToStepModel.StepFaction;
            StepRole = _jumpToStepModel.StepRole;
            PreEvaluationPass = EvaluateFactionCompletion() && EvaluateRoleCompletion();
        }

        public override void Initialize() { }

        public override void Dispose() { }

        public override void Run()
        {
            if (!PreEvaluationPass)
            {
                MarkAsCompleted();
                return;
            }

            if (EvaluateCompleteCondition())
            {
                _profile.JumpToStep(Name, _stepToJumpTo);
            }
            MarkAsCompleted();
        }
    }
}
