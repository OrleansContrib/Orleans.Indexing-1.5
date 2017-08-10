using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Orleans.Indexing
{
    /// <summary>
    /// A node in the linked list of workflowRecords.
    /// 
    /// This linked list makes the traversal more efficient.
    /// </summary>
    [Serializable]
    internal class IndexWorkflowRecordNode
    {
        internal IndexWorkflowRecord WorkflowRecord;

        internal IndexWorkflowRecordNode Prev = null;
        internal IndexWorkflowRecordNode Next = null;

        /// <summary>
        /// This constructor creates a punctuation node
        /// </summary>
        public IndexWorkflowRecordNode() : this(null)
        {
        }

        public IndexWorkflowRecordNode(IndexWorkflowRecord workflow)
        {
            WorkflowRecord = workflow;
        }

        public void Append(IndexWorkflowRecordNode elem, ref IndexWorkflowRecordNode tail)
        {
            var tmpNext = Next;
            if (tmpNext != null)
            {
                elem.Next = tmpNext;
                tmpNext.Prev = elem;
            }
            elem.Prev = this;
            Next = elem;

            if (tail == this)
            {
                tail = elem;
            }
        }

        public IndexWorkflowRecordNode AppendPunctuation(ref IndexWorkflowRecordNode tail)
        {
            //we never append a punctuation to an existing punctuation.
            //It should never be requested
            if (IsPunctuation()) throw new Exception("Adding a punctuation to a work-flow queue that already has a punctuation is not allowed.");

            var punctuation = new IndexWorkflowRecordNode();
            Append(punctuation, ref tail);
            return punctuation;
        }

        public void Remove(ref IndexWorkflowRecordNode head, ref IndexWorkflowRecordNode tail)
        {
            if (Prev == null) head = Next;
            else Prev.Next = Next;

            if (Next == null) tail = Prev;
            else Next.Prev = Prev;

            Clean();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Clean()
        {
            WorkflowRecord = null;
            Next = null;
            Prev = null;
        }

        internal bool IsPunctuation()
        {
            return WorkflowRecord == null;
        }

        public override string ToString()
        {
            int count = 0;
            StringBuilder res = new StringBuilder();
            IndexWorkflowRecordNode curr = this;
            do
            {
                ++count;
                res.Append(curr.IsPunctuation() ? "::Punc::" : curr.WorkflowRecord.ToString()).Append(",\n");
                curr = curr.Next;
            } while (curr != null);
            res.Append("Number of elements: ").Append(count);
            return res.ToString();
        }

        public string ToStringReverse()
        {
            int count = 0;
            StringBuilder res = new StringBuilder();
            IndexWorkflowRecordNode curr = this;
            do
            {
                ++count;
                res.Append(curr.IsPunctuation() ? "::Punc::" : curr.WorkflowRecord.ToString()).Append(",\n");
                curr = curr.Prev;
            } while (curr != null);
            res.Append("Number of elements: ").Append(count);
            return res.ToString();
        }
    }
}
