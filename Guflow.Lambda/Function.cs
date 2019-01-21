using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Guflow.Lambda
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string return predefined result.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string BookHotelLambda(Input input, ILambdaContext context)
        {
            return $"hotelbooked-{input.Id}-{input.Age}";
        }

        public class Input
        {
            public int Id;
            public int Age;
        }
    }
}
