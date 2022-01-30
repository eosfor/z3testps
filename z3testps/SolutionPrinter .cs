using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.LinearSolver;
using Google.OrTools.Sat;

namespace z3testps
{
    public class SolutionPrinter: CpSolverSolutionCallback
    {
        private int solutionCount_;
        private int solutionLimit_;
        private SourceVMRecord[] _sourceVms;
        private TargetVMRecord[] _targetVms;
        private IntVar[,] _state;
        private List<VMMappingResult> _results;

        public SolutionPrinter(SourceVMRecord[] sourceVms, TargetVMRecord[] targetVms, IntVar[,] state, ref List<VMMappingResult> results, int limit = 5)
        {
            solutionCount_ = 0;
            solutionLimit_ = limit;
            _targetVms = targetVms;
            _sourceVms = sourceVms;
            _state = state;
            _results = results;
        }

        public override void OnSolutionCallback()
        {
            SourceVMRecord[] selectedSource = new SourceVMRecord[_sourceVms.Length];
            TargetVMRecord[] selectedTarget = new TargetVMRecord[_sourceVms.Length];
            for (int i = 0; i < _sourceVms.Length; i++)
            {
                for (int j = 0; j < _targetVms.Length; j++)
                {
                    if (BooleanValue(_state[i, j]))
                    {
                        
                        selectedSource[i] = _sourceVms[i];
                        selectedTarget[i] = _targetVms[j];
                    }
                }
            }
            
            _results.Add(new VMMappingResult(source: selectedSource, target: selectedTarget));
            solutionCount_++;
            if (solutionCount_ >= solutionLimit_)
            {
                StopSearch();
            }
        }
    }
}
