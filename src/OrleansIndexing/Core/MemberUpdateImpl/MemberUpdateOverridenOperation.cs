using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    /// <summary>
    /// This class is a wrapper around another IMemberUpdate, which overrides
    /// the actual operation in the original update
    /// </summary>
    [Serializable]
    public class MemberUpdateOverridenOperation : IMemberUpdate
    {
        private IMemberUpdate _update;
        private IndexOperationType _opType;
        public MemberUpdateOverridenOperation(IMemberUpdate update, IndexOperationType opType)
        {
            _update = update;
            _opType = opType;
        }
        public object GetBeforeImage()
        {
            return (_opType == IndexOperationType.Update || _opType == IndexOperationType.Delete) ? _update.GetBeforeImage() : null;
        }

        public object GetAfterImage()
        {
            return (_opType == IndexOperationType.Update || _opType == IndexOperationType.Insert) ? _update.GetAfterImage() : null;
        }

        public IndexOperationType GetOperationType()
        {
            return _opType;
        }
        
        public override string ToString()
        {
            return MemberUpdate.ToString(this);
        }
    }
}
