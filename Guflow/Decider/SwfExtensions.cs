﻿using System;
using System.Globalization;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal static class SwfExtensions
    {
        public static string SwfFormat(this uint? value)
        {
            if(!value.HasValue)
                return null;
            return value.Value.ToString();
        }

        public static string SwfFormat(this int? value)
        {
            if (!value.HasValue)
                return null;
            return value.Value.ToString();
        }
        public static string SwfFormat(this TimeSpan? value)
        {
            if (!value.HasValue)
                return null;
            return value.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture);
        }

        public static TaskList TaskList(this string taskListName)
        {
            TaskList taskList = null;
            if (!string.IsNullOrEmpty(taskListName))
                taskList = new TaskList() { Name = taskListName };
            return taskList;
        }
    }
}