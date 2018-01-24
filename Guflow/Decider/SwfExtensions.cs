// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Globalization;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal static class SwfExtensions
    {
        public static string SwfFormat(this uint? value)
        {
            return value?.ToString();
        }

        public static string SwfFormat(this int? value)
        {
            return value?.ToString();
        }
        public static string Seconds(this TimeSpan? value)
        {
            return value?.TotalSeconds.ToString(CultureInfo.InvariantCulture);
        }

        public static Amazon.SimpleWorkflow.Model.TaskList TaskList(this string taskListName)
        {
            Amazon.SimpleWorkflow.Model.TaskList taskList = null;
            if (!string.IsNullOrEmpty(taskListName))
                taskList = new Amazon.SimpleWorkflow.Model.TaskList() { Name = taskListName };
            return taskList;
        }
    }
}