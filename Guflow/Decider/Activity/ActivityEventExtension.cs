// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using Guflow.Properties;

namespace Guflow.Decider
{
    public static class ActivityEventExtension
    {
        /// <summary>
        /// Deserialize the result of ActivityCompletedEvent to type T.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        public static TType Result<TType>(this ActivityCompletedEvent @event)
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
        /// Deserialize the result of ActivityCompletedEvent in to dynamic object.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public static dynamic Result(this ActivityCompletedEvent @event)=> @event.Result.AsDynamic();
       
    }
}