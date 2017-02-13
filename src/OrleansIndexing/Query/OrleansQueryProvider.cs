using Orleans.Runtime;
using Orleans.Runtime.MembershipService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using Orleans.Streams;

namespace Orleans.Indexing
{
    /// <summary>
    /// Implements <see cref="IOrleansQueryProvider"/>
    /// </summary>
    public class OrleansQueryProvider<TIGrain, TProperties> : IOrleansQueryProvider where TIGrain : IIndexableGrain
    {
        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Call && ((MethodCallExpression)expression).Arguments.Count > 0)
            {
                var genericArgs = ((MethodCallExpression)expression).Arguments[0].Type.GetGenericArguments();
                return CreateQuery(expression, genericArgs[0], genericArgs[1]);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return (IQueryable<TElement>)((IQueryProvider)this).CreateQuery(expression);
        }

        private IQueryable CreateQuery(Expression expression, Type iGrainType, Type iPropertiesType)
        {
            if(expression.NodeType == ExpressionType.Call)
            {
                var methodCall = ((MethodCallExpression)expression);
                IGrainFactory gf;
                IStreamProvider streamProvider;
                if (IsWhereClause(methodCall) 
                    && CheckIsOrleansIndex(methodCall.Arguments[0], iGrainType, iPropertiesType, out gf, out streamProvider)
                    && methodCall.Arguments[1].NodeType == ExpressionType.Quote
                    && ((UnaryExpression)methodCall.Arguments[1]).Operand.NodeType == ExpressionType.Lambda)
                {
                    var whereClause = (LambdaExpression)((UnaryExpression)methodCall.Arguments[1]).Operand;
                    String indexName;
                    object lookupValue;
                    if(TryGetIndexNameAndLookupValue(whereClause, iGrainType, out indexName, out lookupValue))
                    {
                        return (IQueryable)Activator.CreateInstance(typeof(QueryIndexedGrainsNode<,>).MakeGenericType(iGrainType, iPropertiesType), gf, streamProvider, indexName, lookupValue);
                    }
                }
            }
            throw new NotSupportedException();
        }

        private bool CheckIsOrleansIndex(Expression e, Type iGrainType, Type iPropertiesType, out IGrainFactory gf, out IStreamProvider streamProvider)
        {
            if(e.NodeType == ExpressionType.Constant &&
                typeof(QueryActiveGrainsNode<,>).MakeGenericType(iGrainType, iPropertiesType).IsAssignableFrom(((ConstantExpression)e).Value.GetType().GetGenericTypeDefinition().MakeGenericType(iGrainType, iPropertiesType)))
            {
                var qNode = ((QueryGrainsNode)((ConstantExpression)e).Value);
                gf = qNode.GetGrainFactory();
                streamProvider = qNode.GetStreamProvider();
                return true;
            }
            gf = null;
            streamProvider = null;
            return false;
        }

        private bool IsWhereClause(MethodCallExpression call)
        {
            return call.Arguments.Count() == 2 && call.Method.ReflectedType.Equals(typeof(Queryable)) && call.Method.Name == "Where";
        }

        /// <summary>
        /// This method tries to pull out the index name and
        /// lookup value from the given expression tree.
        /// </summary>
        /// <param name="exprTree">the given expression tree</param>
        /// <param name="indexName">the index name that is intended to
        /// be pulled out of the expression tree.</param>
        /// <param name="lookupValue">the lookup value that is intended to
        /// be pulled out of the expression tree.</param>
        /// <returns>determines whether the operation was successful or not</returns>
        private static bool TryGetIndexNameAndLookupValue(LambdaExpression exprTree, Type iGrainType, out string indexName, out object lookupValue)
        {
            if (exprTree.Body is BinaryExpression)
            {
                BinaryExpression operation = (BinaryExpression)exprTree.Body;
                if (operation.NodeType == ExpressionType.Equal)
                {
                    Expression constantExpr = null;
                    Expression fieldExpr = null;
                    if (operation.Right is ConstantExpression || operation.Right is MemberExpression)
                    {
                        constantExpr = operation.Right;
                        fieldExpr = operation.Left;
                    }
                    else if (operation.Left is ConstantExpression || operation.Right is MemberExpression)
                    {
                        constantExpr = operation.Left;
                        fieldExpr = operation.Right;
                    }

                    if (constantExpr != null && fieldExpr != null)
                    {
                        if (constantExpr is ConstantExpression)
                        {
                            lookupValue = ((ConstantExpression)constantExpr).Value;
                            indexName = GetIndexName(exprTree, iGrainType, fieldExpr);
                            return true;
                        }
                        else if(constantExpr is MemberExpression)
                        {
                            object targetObj = Expression.Lambda<Func<object>>(((MemberExpression)operation.Right).Expression).Compile()();
                            lookupValue = ((FieldInfo)((MemberExpression)operation.Right).Member).GetValue(targetObj);
                            indexName = GetIndexName(exprTree, iGrainType, fieldExpr);
                            return true;
                        }
                    }
                }
            }
            throw new NotSupportedException(string.Format("The provided expression is not supported yet: {0}", exprTree));
        }

        /// <summary>
        /// This method tries to pull out the index name from
        /// a given field expression.
        /// </summary>
        /// <param name="exprTree">the original expression tree</param>
        /// <param name="fieldExpr">the field expression that should
        /// contain the indexed field</param>
        /// <returns></returns>
        private static string GetIndexName(LambdaExpression exprTree, Type iGrainType, Expression fieldExpr)
        {
            ParameterExpression iGrainParam = exprTree.Parameters[0];
            if (fieldExpr is MemberExpression)
            {
                Expression innerFieldExpr = ((MemberExpression)fieldExpr).Expression;
                if ((innerFieldExpr.NodeType == ExpressionType.Parameter && innerFieldExpr.Equals(iGrainParam)) ||
                    (innerFieldExpr.NodeType == ExpressionType.Convert && ((UnaryExpression)innerFieldExpr).Operand.Equals(iGrainParam)))
                {
                    return IndexUtils.GetIndexNameOnInterfaceGetter(iGrainType, ((MemberExpression)fieldExpr).Member.Name);
                }
            }
            throw new NotSupportedException(string.Format("The provided expression is not supported yet: {0}", exprTree));
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Executes the query represented by a specified expression tree.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>
        /// The value that results from executing the specified query.
        /// </returns>
        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }
    }
}
