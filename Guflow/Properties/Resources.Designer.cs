﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Reflection;

namespace Guflow.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Guflow.Properties.Resources", typeof(Resources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Activity name {0} and version {1} is already hosted..
        /// </summary>
        internal static string Activity_already_hosted {
            get {
                return ResourceManager.GetString("Activity_already_hosted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ActivityDescription is not supplied for activity {0}..
        /// </summary>
        internal static string Activity_description_missing {
            get {
                return ResourceManager.GetString("Activity_description_missing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Activity execution is already stopped. Start with new instance to begin execution..
        /// </summary>
        internal static string Activity_execution_already_stopped {
            get {
                return ResourceManager.GetString("Activity_execution_already_stopped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mismatch in activity execution count. It is a bug..
        /// </summary>
        internal static string Activity_execution_count_mismatch {
            get {
                return ResourceManager.GetString("Activity_execution_count_mismatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Activity {0} does not have execution method. Please define a method with valid signature and decorate it with ExecuteAttribute.
        /// </summary>
        internal static string Activity_execution_method_missing {
            get {
                return ResourceManager.GetString("Activity_execution_method_missing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to create instance for activity name {0} and version {1}..
        /// </summary>
        internal static string Activity_instance_creation_failed {
            get {
                return ResourceManager.GetString("Activity_instance_creation_failed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Activity instance of type {0} does not match with activity name {1} and version {2}..
        /// </summary>
        internal static string Activity_instance_mismatch {
            get {
                return ResourceManager.GetString("Activity_instance_mismatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Activity name {0} and version {1} is not hosted..
        /// </summary>
        internal static string Activity_not_hosted {
            get {
                return ResourceManager.GetString("Activity_not_hosted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can access activity result only when last event is of type {0}. Last event type of activity is {1}.
        /// </summary>
        internal static string Activity_result_can_not_accessed {
            get {
                return ResourceManager.GetString("Activity_result_can_not_accessed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can not deserialize json data {0} in to type {1}.
        /// </summary>
        internal static string Can_not_deserialize_json_data_into_type {
            get {
                return ResourceManager.GetString("Can_not_deserialize_json_data_into_type", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can not determine the task list to poll on for new task when multiple activities are hosted. Please explicitly pass the task list to begin the execution..
        /// </summary>
        internal static string Can_not_determine_the_task_list_to_poll_for_activity_task {
            get {
                return ResourceManager.GetString("Can_not_determine_the_task_list_to_poll_for_activity_task", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can not determine the task list to poll on for new decisions when multiple workflows are hosted. Please explicitly pass the task list to begin the execution. .
        /// </summary>
        internal static string Can_not_determine_the_task_list_to_poll_for_workflow_decisions {
            get {
                return ResourceManager.GetString("Can_not_determine_the_task_list_to_poll_for_workflow_decisions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t reply to a signal when not send by a workflow..
        /// </summary>
        internal static string Can_not_reply_to_signal {
            get {
                return ResourceManager.GetString("Can_not_reply_to_signal", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Concurrent execution limit should be more than zero..
        /// </summary>
        internal static string Concurrent_execution_limit_should_be_more_than_zero {
            get {
                return ResourceManager.GetString("Concurrent_execution_limit_should_be_more_than_zero", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Workflow item {0} depends on itself.
        /// </summary>
        internal static string Cyclic_dependency {
            get {
                return ResourceManager.GetString("Cyclic_dependency", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Default task list is missing. Please either provide default task list in description or pass the one explicitly to begin the execution.
        /// </summary>
        internal static string Default_task_list_is_missing {
            get {
                return ResourceManager.GetString("Default_task_list_is_missing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Domain {0} is decprecated..
        /// </summary>
        internal static string Domain_deprecated {
            get {
                return ResourceManager.GetString("Domain_deprecated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Domain is missing..
        /// </summary>
        internal static string Domain_name_missing {
            get {
                return ResourceManager.GetString("Domain_name_missing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Domain name required..
        /// </summary>
        internal static string Domain_name_required {
            get {
                return ResourceManager.GetString("Domain_name_required", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Activity with name {0}, version {1} and positional markerName {2} is already added to workflowClosingActions..
        /// </summary>
        internal static string Duplicate_activity {
            get {
                return ResourceManager.GetString("Duplicate_activity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Timer with name {0} is already added to workflowClosingActions..
        /// </summary>
        internal static string Duplicate_timer {
            get {
                return ResourceManager.GetString("Duplicate_timer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Version is empty..
        /// </summary>
        internal static string Empty_version {
            get {
                return ResourceManager.GetString("Empty_version", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Heartbeat on activity {0} is enabled but heartbeat interval is not configured..
        /// </summary>
        internal static string Heartbeat_is_enabled_but_interval_is_missing {
            get {
                return ResourceManager.GetString("Heartbeat_is_enabled_but_interval_is_missing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Host is already executing.
        /// </summary>
        internal static string Host_already_excuting {
            get {
                return ResourceManager.GetString("Host_already_excuting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Host is in faulted state. Please create a new host to start execution..
        /// </summary>
        internal static string Host_is_faulted {
            get {
                return ResourceManager.GetString("Host_is_faulted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Host is stopped. Please create a new host to start execution. .
        /// </summary>
        internal static string Host_is_stopped {
            get {
                return ResourceManager.GetString("Host_is_stopped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid heartbeat interval. Interval must be greater than zero..
        /// </summary>
        internal static string Invalid_heartbeat_interval {
            get {
                return ResourceManager.GetString("Invalid_heartbeat_interval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can not jump to item {0} from item {1} because former is in different branch than later&apos;s branches..
        /// </summary>
        internal static string Invalid_jump {
            get {
                return ResourceManager.GetString("Invalid_jump", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Target method {0} has incompatible parameter {1} which can&apos;t be assigned from source object {2}&apos;s property..
        /// </summary>
        internal static string Invalid_parameter {
            get {
                return ResourceManager.GetString("Invalid_parameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Workflow method {0} has invalid return type. Only {1} and {2} are allowed as return types..
        /// </summary>
        internal static string Invalid_return_type {
            get {
                return ResourceManager.GetString("Invalid_return_type", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t access result. Last event of lambda function is not completed event..
        /// </summary>
        internal static string Lambda_result_can_not_be_accessed {
            get {
                return ResourceManager.GetString("Lambda_result_can_not_be_accessed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Activity {0} has multiple execution method defined. Please provide only one execution method..
        /// </summary>
        internal static string Multiple_activity_execution_methods_defined {
            get {
                return ResourceManager.GetString("Multiple_activity_execution_methods_defined", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Multiple workflow methods are found for event {0}. Please provide only one method..
        /// </summary>
        internal static string Multiple_event_methods {
            get {
                return ResourceManager.GetString("Multiple_event_methods", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Empty activities. Please provide at least one activity to begin hosting..
        /// </summary>
        internal static string No_activity_to_host {
            get {
                return ResourceManager.GetString("No_activity_to_host", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Empty workflows. Please provide at least one workflow to begin hosting..
        /// </summary>
        internal static string No_workflow_to_host {
            get {
                return ResourceManager.GetString("No_workflow_to_host", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Activity type {0} does not derive from {1} class..
        /// </summary>
        internal static string Non_activity_type {
            get {
                return ResourceManager.GetString("Non_activity_type", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Workflow type {0} does not derive from {1} class..
        /// </summary>
        internal static string Non_Workflow_type {
            get {
                return ResourceManager.GetString("Non_Workflow_type", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Null instance of Log is return from factory..
        /// </summary>
        internal static string Null_logger_is_returned {
            get {
                return ResourceManager.GetString("Null_logger_is_returned", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parameter {0} can not be deserialized from string value {1}..
        /// </summary>
        internal static string Parameter_can_not_be_deserialized {
            get {
                return ResourceManager.GetString("Parameter_can_not_be_deserialized", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Read startegy is required..
        /// </summary>
        internal static string Read_strategy_required {
            get {
                return ResourceManager.GetString("Read_strategy_required", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can not find the schedulable item for {0}..
        /// </summary>
        internal static string Schedulable_item_missing {
            get {
                return ResourceManager.GetString("Schedulable_item_missing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TaskList name is required..
        /// </summary>
        internal static string TaskListName_required {
            get {
                return ResourceManager.GetString("TaskListName_required", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Crashing out because of unhandled exception in activity execution..
        /// </summary>
        internal static string Unhandled_activity_exception {
            get {
                return ResourceManager.GetString("Unhandled_activity_exception", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Workflow name {0} and version {1} is already hosted..
        /// </summary>
        internal static string Workflow_already_hosted {
            get {
                return ResourceManager.GetString("Workflow_already_hosted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Workflow name {0} and version {1} is deprecated. .
        /// </summary>
        internal static string Workflow_deprecated {
            get {
                return ResourceManager.GetString("Workflow_deprecated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to WorkflowDescription is not supplied on workflow {0}..
        /// </summary>
        internal static string Workflow_description_not_supplied {
            get {
                return ResourceManager.GetString("Workflow_description_not_supplied", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Workflow execution is already stopped. Start with new instance to begin execution..
        /// </summary>
        internal static string Workflow_execution_already_stopped {
            get {
                return ResourceManager.GetString("Workflow_execution_already_stopped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Workflow name {0} and version {1} is not hosted..
        /// </summary>
        internal static string Workflow_not_hosted {
            get {
                return ResourceManager.GetString("Workflow_not_hosted", resourceCulture);
            }
        }
    }
}
