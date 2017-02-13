using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    /// <summary>
    /// Default implementation of IIndexUpdateGenerator<K,V>
    /// that generically implements CreateMemberUpdate
    /// </summary>
    /// <typeparam name="K">the key type of the index</typeparam>
    /// <typeparam name="V">the value type of the index</typeparam>
    //[Serializable]
    //public abstract class IndexUpdateGenerator<K, TProperties> : IIndexUpdateGenerator<K, TProperties>
    //{
    //    public override IMemberUpdate CreateMemberUpdate(TProperties gProps, K befImg)
    //    {
    //        K aftImg = ExtractIndexImage(gProps);
    //        return new MemberUpdate(befImg, aftImg);
    //    }
    //}

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class IndexUpdateGenerator : IIndexUpdateGenerator
    {
        PropertyInfo _prop;
        public IndexUpdateGenerator(PropertyInfo prop)
        {
            _prop = prop;
        }

        public IMemberUpdate CreateMemberUpdate(object gProps, object befImg)
        {
            object aftImg = gProps == null ? null : ExtractIndexImage(gProps);
            return new MemberUpdate(befImg, aftImg);
        }

        public IMemberUpdate CreateMemberUpdate(object aftImg)
        {
            return new MemberUpdate(null, aftImg);
        }

        public object ExtractIndexImage(object gProps)
        {
            return _prop.GetValue(gProps);
        }
    }
}
