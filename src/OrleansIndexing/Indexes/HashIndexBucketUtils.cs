using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Providers;
using System.Collections.Concurrent;
using System.Threading;

namespace Orleans.Indexing
{
    internal static class HashIndexBucketUtils
    {
        /// <summary>
        /// This method contains the common functionality for updating
        /// hash-index bucket.
        /// </summary>
        /// <typeparam name="K">key type</typeparam>
        /// <typeparam name="V">value type</typeparam>
        /// <param name="updatedGrain">the updated grain that is being indexed</param>
        /// <param name="update">the update information</param>
        /// <param name="opType">the update operation type, which might be different
        /// from the update operation type inside the update parameter</param>
        /// <param name="State">the index bucket to be updated</param>
        /// <param name="isUniqueIndex">a flag to indicate whether the
        /// hash-index has a uniqueness constraint</param>
        internal static bool UpdateBucket<K, V>(V updatedGrain, IMemberUpdate iUpdate, HashIndexBucketState<K, V> State, bool isUniqueIndex, IndexMetaData idxMetaData) where V : IIndexableGrain
        {
            K befImg;
            HashIndexSingleBucketEntry<V> befEntry;
            bool fixIndexUnavailableOnDelete;
            return UpdateBucket(updatedGrain, iUpdate, State, isUniqueIndex, idxMetaData, out befImg, out befEntry, out fixIndexUnavailableOnDelete);
        }

        /// <summary>
        /// This method contains the common functionality for updating
        /// hash-index bucket.
        /// </summary>
        /// <typeparam name="K">key type</typeparam>
        /// <typeparam name="V">value type</typeparam>
        /// <param name="updatedGrain">the updated grain that is being indexed</param>
        /// <param name="update">the update information</param>
        /// <param name="opType">the update operation type, which might be different
        /// from the update operation type inside the update parameter</param>
        /// <param name="State">the index bucket to be updated</param>
        /// <param name="isUniqueIndex">a flag to indicate whether the 
        /// hash-index has a uniqueness constraint</param>
        /// <param name="befImg">output parameter: the before-image</param>
        /// <param name="befEntry">output parameter: the index entry containing the before-image</param>
        /// <param name="fixIndexUnavailableOnDelete">output parameter: this variable determines whether
        /// index was still unavailable when we received a delete operation</param>
        internal static bool UpdateBucket<K, V>(V updatedGrain, IMemberUpdate update, HashIndexBucketState<K, V> State, bool isUniqueIndex, IndexMetaData idxMetaData, out K befImg, out HashIndexSingleBucketEntry<V> befEntry, out bool fixIndexUnavailableOnDelete) where V : IIndexableGrain
        {
            fixIndexUnavailableOnDelete = false;
            befImg = default(K);
            befEntry = null;

            bool isTentativeUpdate = isUniqueIndex && (update is MemberUpdateTentative);
            IndexOperationType opType = update.GetOperationType();
            HashIndexSingleBucketEntry<V> aftEntry;
            if (opType == IndexOperationType.Update)
            {
                befImg = (K)update.GetBeforeImage();
                K aftImg = (K)update.GetAfterImage();
                if (State.IndexMap.TryGetValue(befImg, out befEntry) && befEntry.Values.Contains(updatedGrain))
                {   //Delete and Insert
                    if (State.IndexMap.TryGetValue(aftImg, out aftEntry))
                    {
                        if (aftEntry.Values.Contains(updatedGrain))
                        {
                            if (isTentativeUpdate)
                            {
                                aftEntry.setTentativeInsert();
                            }
                            else
                            {
                                aftEntry.clearTentativeFlag();
                                befEntry.Remove(updatedGrain, isTentativeUpdate, isUniqueIndex);
                            }
                        }
                        else
                        {
                            if (isUniqueIndex && aftEntry.Values.Count > 0)
                            {
                                throw new UniquenessConstraintViolatedException(string.Format("The uniqueness property of index is violated after an update operation for before-image = {0}, after-image = {1} and grain = {2}", befImg, aftImg, updatedGrain.GetPrimaryKey()));
                            }
                            befEntry.Remove(updatedGrain, isTentativeUpdate, isUniqueIndex);
                            aftEntry.Add(updatedGrain, isTentativeUpdate, isUniqueIndex);
                        }
                    }
                    else
                    {
                        aftEntry = new HashIndexSingleBucketEntry<V>();
                        befEntry.Remove(updatedGrain, isTentativeUpdate, isUniqueIndex);
                        aftEntry.Add(updatedGrain, isTentativeUpdate, isUniqueIndex);
                        State.IndexMap.Add(aftImg, aftEntry);
                    }
                }
                else
                {
                    //Insert
                    if (State.IndexMap.TryGetValue(aftImg, out aftEntry))
                    {
                        if (!aftEntry.Values.Contains(updatedGrain))
                        {
                            if (isUniqueIndex && aftEntry.Values.Count > 0)
                            {
                                throw new UniquenessConstraintViolatedException(string.Format("The uniqueness property of index is violated after an update operation for (not found before-image = {0}), after-image = {1} and grain = {2}", befImg, aftImg, updatedGrain.GetPrimaryKey()));
                            }
                            aftEntry.Add(updatedGrain, isTentativeUpdate, isUniqueIndex);
                        }
                        else if (isTentativeUpdate)
                        {
                            aftEntry.setTentativeInsert();
                        }
                        else
                        {
                            aftEntry.clearTentativeFlag();
                        }
                    }
                    else
                    {
                        aftEntry = new HashIndexSingleBucketEntry<V>();
                        aftEntry.Add(updatedGrain, isTentativeUpdate, isUniqueIndex);
                        State.IndexMap.Add(aftImg, aftEntry);
                    }
                }
            }
            else if (opType == IndexOperationType.Insert)
            { // Insert
                K aftImg = (K)update.GetAfterImage();
                if (State.IndexMap.TryGetValue(aftImg, out aftEntry))
                {
                    if (!aftEntry.Values.Contains(updatedGrain))
                    {
                        if (isUniqueIndex && aftEntry.Values.Count > 0)
                        {
                            throw new UniquenessConstraintViolatedException(string.Format("The uniqueness property of index is violated after an insert operation for after-image = {0} and grain = {1}", aftImg, updatedGrain.GetPrimaryKey()));
                        }
                        aftEntry.Add(updatedGrain, isTentativeUpdate, isUniqueIndex);
                    }
                    else if (isTentativeUpdate)
                    {
                        aftEntry.setTentativeInsert();
                    }
                    else
                    {
                        aftEntry.clearTentativeFlag();
                    }
                }
                else
                {
                    aftEntry = new HashIndexSingleBucketEntry<V>();
                    aftEntry.Add(updatedGrain, isTentativeUpdate, isUniqueIndex);
                    State.IndexMap.Add(aftImg, aftEntry);
                }
            }
            else if (opType == IndexOperationType.Delete)
            { // Delete
                befImg = (K)update.GetBeforeImage();

                if (State.IndexMap.TryGetValue(befImg, out befEntry) && befEntry.Values.Contains(updatedGrain))
                {
                    befEntry.Remove(updatedGrain, isTentativeUpdate, isUniqueIndex);
                    if (State.IndexStatus != IndexStatus.Available)
                    {
                        fixIndexUnavailableOnDelete = true;
                    }
                }
            }
            return true;
        }
    }
}
