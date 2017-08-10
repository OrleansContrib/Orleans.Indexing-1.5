using System;

namespace Orleans.Indexing
{
    /// <summary>
    /// This class is a wrapper around another IMemberUpdate, which represents a
    /// tentative update to an index, which should be specially taken care of by index
    /// so that the change is not visible by others, but still blocks further violation
    /// of constraints such as uniqueness constraint
    /// </summary>
    [Serializable]
    public class MemberUpdateTentative : IMemberUpdate
    {
        private IMemberUpdate _update;
        public MemberUpdateTentative(IMemberUpdate update)
        {
            _update = update;
        }
        public object GetBeforeImage()
        {
            return _update.GetBeforeImage();
        }

        public object GetAfterImage()
        {
            return _update.GetAfterImage();
        }

        public IndexOperationType GetOperationType()
        {
            return _update.GetOperationType();
        }

        public override string ToString()
        {
            return MemberUpdate.ToString(this);
        }
    }
}
