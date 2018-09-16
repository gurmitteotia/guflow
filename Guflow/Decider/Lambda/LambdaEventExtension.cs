// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using Amazon.SimpleWorkflow;
using Guflow.Properties;

namespace Guflow.Decider
{
    public static class LambdaEventExtension
    {
        /// <summary>
        /// Deserialize the Lambda completion result in to Type. It supports the the deserialization in primitive and complex (JSON) type.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        public static TType Result<TType>(this LambdaCompletedEvent @event)
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
        /// Deserializet the Lambda completion result in to dynamic object.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public static dynamic Result(this LambdaCompletedEvent @event) => @event.Result.AsDynamic();
    }
}