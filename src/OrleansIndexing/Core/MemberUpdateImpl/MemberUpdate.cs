using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    /// <summary>
    /// MemberUpdate is a generic implementation of IMemberUpdate
    /// that relies on a copy of beforeImage and afterImage, without
    /// keeping any semantic information about the actual change that
    /// happened.
    /// This class assumes that befImg and aftImg passed to it won't be
    /// altered later on, so they are immutable.
    /// </summary>
    [Serializable]
    public class MemberUpdate : IMemberUpdate
    {
        private object _befImg;
        private object _aftImg;
        private IndexOperationType _opType;

        public MemberUpdate(object befImg, object aftImg, IndexOperationType opType)
        {
            _opType = opType;
            if (opType == IndexOperationType.Update || opType == IndexOperationType.Delete)
            {
                _befImg = befImg;
            }
            if (opType == IndexOperationType.Update || opType == IndexOperationType.Insert)
            {
                _aftImg = aftImg;
            }
        }

        public MemberUpdate(object befImg, object aftImg) : this(befImg, aftImg, GetOperationType(befImg, aftImg))
        {
        }

        private static IndexOperationType GetOperationType(object befImg, object aftImg)
        {
            if(befImg == null)
            {
                if (aftImg == null) return IndexOperationType.None;
                else return IndexOperationType.Insert;
            }
            else
            {
                if (aftImg == null) return IndexOperationType.Delete;
                else if(befImg.Equals(aftImg)) return IndexOperationType.None;
                else return IndexOperationType.Update;
            }
        }

        /// <summary>
        /// Exposes the stored before-image.
        /// </summary>
        /// <returns>the before-image of the indexed attribute(s)
        /// that is before applying the current update</returns>
        public object GetBeforeImage()
        {
            return (_opType == IndexOperationType.Update || _opType == IndexOperationType.Delete) ? _befImg : null;
        }

        public object GetAfterImage()
        {
            return (_opType == IndexOperationType.Update || _opType == IndexOperationType.Insert) ? _aftImg : null;
        }

        public IndexOperationType GetOperationType()
        {
            return _opType;
        }

        public override string ToString()
        {
            return ToString(this);
        }

        internal static string ToString(IMemberUpdate update)
        {
            switch (update.GetOperationType())
            {
                case IndexOperationType.None: return update.GetType().Name + ": No operation";
                case IndexOperationType.Insert: return update.GetType().Name + ": Inserted " + update.GetAfterImage();
                case IndexOperationType.Delete: return update.GetType().Name + ": Deleted " + update.GetBeforeImage();
                case IndexOperationType.Update: return update.GetType().Name + ": Updated " + update.GetBeforeImage() + " into " + update.GetAfterImage();
                default: return update.GetType().Name + ": Unsupported operation";
            }
        }

        internal static string UpdatesToString(IDictionary<IIndexableGrain, IList<IMemberUpdate>> iUpdates)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var grainUpdate in iUpdates)
            {
                sb.Append(Environment.NewLine).Append(grainUpdate.Key).Append(" =>");
                sb.Append(UpdatesToString(grainUpdate.Value));
            }
            return sb.ToString();
        }

        internal static string UpdatesToString(IList<IMemberUpdate> updates)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var updt in updates)
            {
                sb.Append(Environment.NewLine).Append("\t").Append(updt);
            }
            return sb.ToString();
        }
    }
}
