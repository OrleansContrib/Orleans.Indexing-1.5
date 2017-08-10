using System;
using System.Reflection;

namespace Orleans.Indexing
{
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
