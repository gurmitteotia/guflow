// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using Amazon.Lambda.Core;

namespace Guflow.Lambda
{
    public class ExpenseLambdaFunctions
    {
        public string ExpenseApproval(Input input, ILambdaContext context)
        {
            Console.WriteLine($"Store pass workflow id {input.Id} in to database to later use it to send signal");
            Console.WriteLine("Send emails to manager to approve the expense and emails have approve and reject links");
            return "Done";
        }

        public string SubmitToAccount(string input, ILambdaContext context)
        {
            return "AccountDone";
        }

        public string ExpenseRejected(string input, ILambdaContext context)
        {
            return "EmpAction";
        }

        public class Input
        {
            public string Id;
        }
    }
}