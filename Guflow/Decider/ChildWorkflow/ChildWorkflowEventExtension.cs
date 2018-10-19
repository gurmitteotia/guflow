// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using Guflow.Properties;

namespace Guflow.Decider
{
    public static class ChildWorkflowEventExtension
    {
        /// <summary>
        /// Deserialize the child workflow completion result in to TType. It supports the the deserialization in primitive and complex (JSON) type.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        public static TType Result<TType>(this ChildWorkflowCompletedEvent @event)
        {
            try
            {
                if (typeof(TType).Primitive())
                    return (TType)Convert.ChangeType(@event.Result, typeof(TType));
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(string.Format(Resources.Can_not_deserialize_json_data_into_type, @event.Result, typeof(TType)), exception);
            }
            return @event.Result.As<TType>();
        }

        /// <summary>
        /// Deserializet the child workflow completion result in to dynamic object.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public static dynamic Result(this ChildWorkflowCompletedEvent @event) => @event.Result.AsDynamic();
    }
}